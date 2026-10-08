using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ZooWorld.Game.Animals
{
    public class AnimalCatalog : IDisposable
    {
        public const string Label = "animal";

        private AsyncOperationHandle<IList<AnimalConfig>> _handle;

        /// <summary>Returns an empty list when no asset carries the label or loading fails.</summary>
        public async UniTask<IReadOnlyList<AnimalConfig>> LoadAsync(CancellationToken cancellation)
        {
            _handle = Addressables.LoadAssetsAsync<AnimalConfig>(Label);
            try
            {
                var configs = await _handle.ToUniTask(cancellationToken: cancellation);
                return new List<AnimalConfig>(configs);
            }
            catch (Exception exception) when (!(exception is OperationCanceledException))
            {
                Debug.LogException(exception);
                return Array.Empty<AnimalConfig>();
            }
        }

        public void Dispose()
        {
            if (_handle.IsValid())
                Addressables.Release(_handle);
        }
    }
}
