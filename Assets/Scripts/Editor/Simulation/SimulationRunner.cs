using System;
using System.Collections.Generic;
using System.Diagnostics;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;
using ZooWorld.Core;
using ZooWorld.Core.Events;
using ZooWorld.Core.World;
using ZooWorld.Game;
using ZooWorld.Game.Animals;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace ZooWorld.Editor.Simulation
{
    /// <summary>
    /// Runs the world for a fixed number of ticks, several times over, faster than real time:
    /// physics is stepped by hand in Play Mode while the game's own clock stands still.
    /// </summary>
    [Serializable]
    public class SimulationRunner
    {
        private enum State { Idle, EnteringPlayMode, LoadingScene, WaitingForWorld, Prewarming, Stepping }

        // Real seconds of stepping per editor update; the rest of the frame keeps the editor responsive.
        private const double FrameBudget = 0.05;
        private const double LoadTimeout = 10.0;
        // EnterPlaymode only takes effect on a later editor update; until then nothing says it is coming.
        private const double EnterGrace = 1.0;

        // Serialized so a series survives the domain reload some projects do when entering Play Mode.
        [SerializeField] private State state;
        [SerializeField] private int ticks;
        [SerializeField] private int runs;
        [SerializeField] private bool startedPlayMode;
        [SerializeField] private bool settingsTaken;
        [SerializeField] private SimulationMode previousMode;
        [SerializeField] private float previousScale;
        [SerializeField] private bool previousRunInBackground;

        private readonly List<IReadOnlyDictionary<string, SpeciesTally>> _results = new();
        private readonly Dictionary<string, int> _eaten = new();
        private readonly Stopwatch _frame = new();

        private List<SpeciesRow> _rows = new();
        private AsyncOperation _load;
        private AnimalWorld _world;
        private AnimalSpawner _spawner;
        private IDisposable _subscription;
        private double _deadline;
        private int _run;
        private int _tick;

        public bool IsRunning => state != State.Idle;
        public string Error { get; private set; }
        public IReadOnlyList<SpeciesRow> Rows => _rows;
        public int CompletedRuns { get; private set; }
        public int Ticks => ticks;
        public Vector2 AreaSize { get; private set; }

        public float Progress => runs == 0 || ticks == 0 ? 0f : (_run + (float)_tick / ticks) / runs;
        public string Status => state == State.Stepping ? $"Run {_run + 1} of {runs}" : $"Run {_run + 1} of {runs}: loading";

        public static int ToTicks(float duration, bool inSeconds, float fixedDeltaTime) =>
            Mathf.Max(1, Mathf.RoundToInt(inSeconds ? duration / fixedDeltaTime : duration));

        public void Start(int ticksPerRun, int runCount)
        {
            if (IsRunning)
                return;

            ticks = Mathf.Max(1, ticksPerRun);
            runs = Mathf.Max(1, runCount);
            _results.Clear();
            _rows = new List<SpeciesRow>();
            _run = 0;
            _tick = 0;
            CompletedRuns = 0;
            Error = null;

            startedPlayMode = !EditorApplication.isPlaying;
            if (startedPlayMode)
            {
                state = State.EnteringPlayMode;
                _deadline = EditorApplication.timeSinceStartup + EnterGrace;
                EditorApplication.EnterPlaymode();
            }
            else
            {
                BeginSeries();
            }
        }

        public void Cancel()
        {
            if (IsRunning)
                Finish("Cancelled.");
        }

        public void Update()
        {
            if (state == State.Idle)
                return;

            try
            {
                if (state == State.EnteringPlayMode)
                {
                    if (EditorApplication.isPlaying)
                        BeginSeries();
                    // Unity refuses to enter Play Mode while scripts do not compile, and says so only in the console.
                    else if (!EditorApplication.isPlayingOrWillChangePlaymode && EditorApplication.timeSinceStartup > _deadline)
                        Finish("Could not enter Play Mode: check the console for compile errors.");
                    return;
                }

                if (!EditorApplication.isPlaying)
                {
                    Finish("Play Mode was stopped before the series finished.");
                    return;
                }

                switch (state)
                {
                    case State.LoadingScene:
                        if (_load.isDone)
                        {
                            _deadline = EditorApplication.timeSinceStartup + LoadTimeout;
                            state = State.WaitingForWorld;
                        }
                        break;
                    case State.WaitingForWorld:
                        WaitForWorld();
                        break;
                    case State.Prewarming:
                        if (EditorApplication.timeSinceStartup > _deadline)
                            Finish("Animal prefabs did not load in time.");
                        break;
                    case State.Stepping:
                        Step();
                        break;
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Finish(exception.Message);
            }
        }

        private void BeginSeries()
        {
            previousMode = Physics.simulationMode;
            previousScale = Time.timeScale;
            previousRunInBackground = Application.runInBackground;
            settingsTaken = true;
            // With physics stepped by hand and the clock stopped, the game's own FixedTick and
            // spawner Tick do nothing between our batches.
            Physics.simulationMode = SimulationMode.Script;
            Time.timeScale = 0f;
            // Scene and prefab loading need player frames, which an unfocused editor does not give otherwise.
            Application.runInBackground = true;
            BeginRun();
        }

        // A reload is the reset: a fresh world, and configs edited since the last run are read again.
        private void BeginRun()
        {
            var path = SceneManager.GetActiveScene().path;
            if (string.IsNullOrEmpty(path))
            {
                Finish("Save the scene first: an unsaved scene cannot be reloaded between runs.");
                return;
            }

            _tick = 0;
            _load = EditorSceneManager.LoadSceneAsyncInPlayMode(path, new LoadSceneParameters(LoadSceneMode.Single));
            state = State.LoadingScene;
        }

        private void WaitForWorld()
        {
            var scope = Object.FindAnyObjectByType<GameLifetimeScope>();
            var spawner = scope != null && scope.Container != null ? scope.Container.Resolve<AnimalSpawner>() : null;
            if (spawner == null || spawner.Configs.Count == 0)
            {
                if (EditorApplication.timeSinceStartup > _deadline)
                    Finish(scope == null
                        ? "The scene has no GameLifetimeScope."
                        : $"No animals loaded: check the '{AnimalCatalog.Label}' Addressables label.");
                return;
            }

            state = State.Prewarming;
            PrewarmThenStep(scope, spawner, _run).Forget();
        }

        private async UniTaskVoid PrewarmThenStep(GameLifetimeScope scope, AnimalSpawner spawner, int run)
        {
            // Pools for every species exist after this, so each spawn completes inside its own tick.
            await scope.Container.Resolve<AnimalFactory>().PrewarmAsync(spawner.Configs);

            // The series may have been cancelled, or have failed, while the prefabs were loading.
            if (state != State.Prewarming || run != _run || scope == null)
                return;

            // The HUD answers every meal with a tween, and tweens do not advance while the clock is stopped.
            foreach (var child in Object.FindObjectsByType<LifetimeScope>())
                if (child.Parent == scope)
                    child.Dispose();

            var container = scope.Container;
            _world = container.Resolve<AnimalWorld>();
            _spawner = spawner;
            if (container.Resolve<IPlayArea>() is CameraPlayArea area)
                AreaSize = area.Size;

            _eaten.Clear();
            _subscription = container.Resolve<ISubscriber<AnimalDiedEvent>>().Subscribe(OnDied);
            state = State.Stepping;
        }

        private void OnDied(AnimalDiedEvent message)
        {
            var species = message.animal.Species;
            _eaten[species] = _eaten.GetValueOrDefault(species) + 1;
        }

        private void Step()
        {
            // The debug speed buttons stay clickable; a running clock would tick the world a second time.
            Time.timeScale = 0f;
            var deltaTime = Time.fixedDeltaTime;
            _frame.Restart();
            while (_tick < ticks && _frame.Elapsed.TotalSeconds < FrameBudget)
            {
                // Same order as a game frame: FixedUpdate, the physics step with its collisions, Update.
                _world.Tick(deltaTime);
                Physics.Simulate(deltaTime);
                _spawner.Step(deltaTime);
                _tick++;
            }

            if (_tick >= ticks)
                FinishRun();
        }

        private void FinishRun()
        {
            var alive = new Dictionary<string, int>();
            foreach (var animal in _world.Animals)
                alive[animal.Species] = alive.GetValueOrDefault(animal.Species) + 1;

            var tallies = new Dictionary<string, SpeciesTally>();
            foreach (var species in _eaten.Keys)
                tallies[species] = new SpeciesTally(_eaten[species], alive.GetValueOrDefault(species));
            foreach (var species in alive.Keys)
                if (!tallies.ContainsKey(species))
                    tallies[species] = new SpeciesTally(0, alive[species]);

            _results.Add(tallies);
            _subscription.Dispose();
            _subscription = null;
            _run++;

            if (_run < runs)
                BeginRun();
            else
                Finish(null);
        }

        // Every way out of a series comes through here, so the settings always go back.
        private void Finish(string error)
        {
            _subscription?.Dispose();
            _subscription = null;
            _world = null;
            _spawner = null;

            if (settingsTaken)
            {
                Physics.simulationMode = previousMode;
                Time.timeScale = previousScale;
                Application.runInBackground = previousRunInBackground;
                settingsTaken = false;
            }

            // Runs finished before a cancel or an error are still worth showing.
            _rows = SimulationReport.Summarize(_results);
            CompletedRuns = _results.Count;
            Error = error;
            state = State.Idle;

            if (startedPlayMode)
            {
                // Unconditional: it also withdraws an entry that was requested but has not happened yet.
                EditorApplication.ExitPlaymode();
                return;
            }

            if (!EditorApplication.isPlaying)
                return;

            // The game was already running when the series began: hand it back whole, HUD included.
            var path = SceneManager.GetActiveScene().path;
            if (!string.IsNullOrEmpty(path))
                EditorSceneManager.LoadSceneAsyncInPlayMode(path, new LoadSceneParameters(LoadSceneMode.Single));
        }
    }
}
