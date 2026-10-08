using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using ZooWorld.Game.Animals;

namespace ZooWorld.Game
{
    public class GameBootstrap : IAsyncStartable
    {
        private readonly AnimalCatalog _catalog;
        private readonly AnimalSpawner _spawner;

        [Inject]
        public GameBootstrap(AnimalCatalog catalog, AnimalSpawner spawner)
        {
            _catalog = catalog;
            _spawner = spawner;
        }

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            var configs = await _catalog.LoadAsync(cancellation);
            if (configs.Count == 0)
            {
                Debug.LogError(
                    $"No AnimalConfig asset carries the '{AnimalCatalog.Label}' Addressables label. Nothing will spawn.");
                return;
            }

            _spawner.Begin(configs);
        }
    }
}