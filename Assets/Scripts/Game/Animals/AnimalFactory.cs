using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;
using ZooWorld.Core;
using ZooWorld.Core.Animals;
using ZooWorld.Core.World;
using Object = UnityEngine.Object;

namespace ZooWorld.Game.Game.Animals
{
    public class AnimalFactory
    {
        private readonly AnimalWorld _world;
        private readonly IPlayArea _area;
        private readonly IRandom _random;
        private readonly GameSettings _settings;
        private readonly CancellationTokenSource _disposed = new();
        private readonly Transform _root = new GameObject("Animals").transform;

        private readonly Dictionary<AnimalConfig, AsyncOperationHandle<GameObject>> _prefabs = new();
        private readonly Dictionary<AnimalConfig, ObjectPool<AnimalView>> _pools = new();

        private int _nextId;

        public AnimalFactory(AnimalWorld world, IPlayArea area, IRandom random, GameSettings settings)
        {
            _world = world;
            _area = area;
            _random = random;
            _settings = settings;
        }

        public async UniTask SpawnAsync(AnimalConfig config, Vector3 position)
        {
            var pool = await GetPoolAsync(config);
            if (pool == null)
                return;

            var view = pool.Get();
            var wander = new WanderDirection(_area, _random, _settings.WanderInterval);
            var animal = new Animal(_nextId++, config.Diet, view, config.Movement.Create(), wander);
            view.Spawn(animal, _world, pool, position);
            _world.Add(animal);
        }

        public void Dispose()
        {
            _disposed.Cancel();
            _disposed.Dispose();
            foreach (var handle in _prefabs.Values)
                if (handle.IsValid())
                    Addressables.Release(handle);
        }

        private async UniTask<ObjectPool<AnimalView>> GetPoolAsync(AnimalConfig config)
        {
            if (_pools.TryGetValue(config, out var pool))
                return pool;

            if (!_prefabs.TryGetValue(config, out var handle))
            {
                if (config.Prefab == null || !config.Prefab.RuntimeKeyIsValid() || config.Diet == null || config.Movement == null)
                    return Reject(config, "it needs a prefab, a diet and a movement config");

                handle = Addressables.LoadAssetAsync<GameObject>(config.Prefab.RuntimeKey);
                _prefabs.Add(config, handle);
            }

            GameObject prefab = null;
            try
            {
                prefab = await handle.ToUniTask(cancellationToken: _disposed.Token);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                Debug.LogException(exception);
            }

            // Another spawn of the same species may have finished the job while this one waited.
            if (_pools.TryGetValue(config, out pool))
                return pool;

            if (prefab == null || !prefab.TryGetComponent(out AnimalView viewPrefab))
                return Reject(config, "its prefab failed to load or has no AnimalView on the root");
            if (!viewPrefab.IsWired)
                return Reject(config, "the AnimalView on its prefab has an empty Body or Visual reference");

            pool = new ObjectPool<AnimalView>(
                createFunc: () =>
                {
                    var view = Object.Instantiate(viewPrefab, _root);
                    view.gameObject.SetActive(false);
                    return view;
                },
                actionOnRelease: view => view.gameObject.SetActive(false));
            _pools.Add(config, pool);
            return pool;
        }

        private ObjectPool<AnimalView> Reject(AnimalConfig config, string reason)
        {
            Debug.LogError($"Animal '{config.name}' will not spawn: {reason}.", config);
            _pools[config] = null;
            return null;
        }
    }
}
