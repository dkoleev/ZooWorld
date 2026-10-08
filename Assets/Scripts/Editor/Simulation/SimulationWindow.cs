using UnityEditor;
using UnityEngine;

namespace ZooWorld.Editor.Simulation
{
    internal class SimulationWindow : EditorWindow
    {
        private enum Unit { Ticks, Seconds }

        private static readonly string[] Columns = { "Species", "Spawned", "Eaten avg", "min", "max", "Alive at end" };
        private static readonly float[] Widths = { 120f, 70f, 80f, 50f, 50f, 90f };

        [SerializeField] private float duration = 3000f;
        [SerializeField] private Unit unit;
        [SerializeField] private int runs = 5;
        [SerializeField] private SimulationRunner runner = new();

        [MenuItem("Zoo World/Simulation")]
        private static void Open() => GetWindow<SimulationWindow>("Simulation");

        // EditorApplication.update, not Update(): a window tabbed behind another one still has to run its series.
        private void OnEnable() => EditorApplication.update += Advance;

        private void OnDisable() => EditorApplication.update -= Advance;

        // Closing the window mid-series must not leave the game with a stopped clock.
        private void OnDestroy() => runner.Cancel();

        private void Advance()
        {
            if (!runner.IsRunning)
                return;

            runner.Update();
            Repaint();
        }

        private void OnGUI()
        {
            var fixedDeltaTime = Time.fixedDeltaTime;
            int ticks;
            using (new EditorGUI.DisabledScope(runner.IsRunning))
            {
                EditorGUILayout.BeginHorizontal();
                duration = EditorGUILayout.FloatField("Duration", duration);
                unit = (Unit)EditorGUILayout.EnumPopup(unit, GUILayout.Width(80f));
                EditorGUILayout.EndHorizontal();

                ticks = SimulationRunner.ToTicks(duration, unit == Unit.Seconds, fixedDeltaTime);
                EditorGUILayout.LabelField(" ", $"= {ticks} ticks = {ticks * fixedDeltaTime:0.##} s of game time");
                runs = Mathf.Max(1, EditorGUILayout.IntField("Runs", runs));
            }

            EditorGUILayout.Space();
            if (runner.IsRunning)
            {
                var bar = EditorGUILayout.GetControlRect(false, 20f);
                EditorGUI.ProgressBar(bar, runner.Progress, runner.Status);
                if (GUILayout.Button("Cancel", GUILayout.Height(28f)))
                    runner.Cancel();
            }
            else if (GUILayout.Button("Run", GUILayout.Height(28f)))
            {
                runner.Start(ticks, runs);
                // Entering Play Mode from inside OnGUI invalidates the layout being built.
                GUIUtility.ExitGUI();
            }

            if (!string.IsNullOrEmpty(runner.Error))
                EditorGUILayout.HelpBox(runner.Error, MessageType.Warning);

            // A series too short for anything to spawn still gets its header, so it does not look like nothing ran.
            if (runner.CompletedRuns > 0)
                DrawReport(fixedDeltaTime);
        }

        private void DrawReport(float fixedDeltaTime)
        {
            EditorGUILayout.Space();
            // The area comes from the camera, so runs at another Game view aspect are not comparable.
            EditorGUILayout.LabelField(
                $"{runner.CompletedRuns} runs x {runner.Ticks} ticks ({runner.Ticks * fixedDeltaTime:0.##} s), " +
                $"play area {runner.AreaSize.x:0.#} x {runner.AreaSize.y:0.#}", EditorStyles.boldLabel);

            DrawRow(EditorStyles.miniBoldLabel, Columns);
            foreach (var row in runner.Rows)
                DrawRow(EditorStyles.label, row.species, row.spawned.ToString("0.#"), row.eaten.ToString("0.#"),
                    row.eatenMin.ToString(), row.eatenMax.ToString(), row.alive.ToString("0.#"));

            EditorGUILayout.Space();
            if (GUILayout.Button("Copy CSV"))
                EditorGUIUtility.systemCopyBuffer = SimulationReport.ToCsv(runner.Rows);
        }

        private static void DrawRow(GUIStyle style, params string[] cells)
        {
            EditorGUILayout.BeginHorizontal();
            for (var i = 0; i < cells.Length; i++)
                GUILayout.Label(cells[i], style, GUILayout.Width(Widths[i]));
            EditorGUILayout.EndHorizontal();
        }
    }
}
