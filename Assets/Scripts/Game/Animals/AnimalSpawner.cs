using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using ZooWorld.Core;
using ZooWorld.Core.World;

namespace ZooWorld.Game.Animals
{
    public class AnimalSpawner : ITickable
    {
        private readonly AnimalFactory _factory;
        private readonly AnimalWorld _world;
        private readonly IPlayArea _area;
        private readonly IRandom _random;
        private readonly GameSettings _settings;
        private readonly SpawnTimer _timer;
        private IReadOnlyList<AnimalConfig> _configs = Array.Empty<AnimalConfig>();
        private bool _capReported;

        [Inject]
        public AnimalSpawner(AnimalFactory factory, AnimalWorld world, IPlayArea area, IRandom random,
            GameSettings settings)
        {
            _factory = factory;
            _world = world;
            _area = area;
            _random = random;
            _settings = settings;
            _timer = new SpawnTimer(random, settings.MinSpawnInterval, settings.MaxSpawnInterval);
        }

        /// <summary>Spawning stays idle until the catalog hands over the species.</summary>
        public void Begin(IReadOnlyList<AnimalConfig> configs) => _configs = configs;

        public void Tick()
        {
            if (_configs.Count == 0 || !_timer.Tick(Time.deltaTime))
                return;
            if (_settings.MaxAlive > 0 && _world.AliveCount >= _settings.MaxAlive)
            {
                ReportCapOnce();
                return;
            }

            var config = _configs[_random.Range(0, _configs.Count)];
            var position = _area.GetRandomPoint(_random) + Vector3.up * _settings.SpawnHeight;
            _factory.SpawnAsync(config, position).Forget();
        }

        private void ReportCapOnce()
        {
            if (_capReported)
                return;

            _capReported = true;
            Debug.LogWarning($"Spawning is paused while {_settings.MaxAlive} animals are alive " +
                             "(GameSettings > Max Alive; 0 removes the limit).", _settings);
        }
    }
}
