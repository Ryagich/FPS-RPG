using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Inventory.Ammo
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class AmmoStorage
    {
        private readonly List<Ammo> ammo = new();
        private readonly Dictionary<string, Ammo> ammoById = new();

        private readonly UniTaskCompletionSource readyTcs = new();

        public IReadOnlyList<Ammo> Ammo => ammo;
        public bool IsReady => readyTcs.Task.Status == UniTaskStatus.Succeeded;

        public UniTask Ready => readyTcs.Task;

        public AmmoStorage()
        {
            _ = LoadAsync();
        }

        private async UniTaskVoid LoadAsync()
        {
            try
            {
                var handle = Addressables.LoadAssetsAsync<AmmoConfig>(
                    "AmmoConfig",
                    null
                );

                var configs = await handle.Task;

                ammo.Clear();
                ammoById.Clear();

                foreach (var config in configs)
                {
                    if (string.IsNullOrEmpty(config.ID))
                    {
                        Debug.LogError($"AmmoConfig '{config.name}' has empty ID");
                        continue;
                    }

                    if (ammoById.ContainsKey(config.ID))
                    {
                        Debug.LogError($"Duplicate AmmoConfig ID: {config.ID}");
                        continue;
                    }

                    var entry = new Ammo(config, config.Max);
                    ammo.Add(entry);
                    ammoById.Add(config.ID, entry);
                }

                readyTcs.TrySetResult();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                readyTcs.TrySetException(e);
            }
        }

        public Ammo GetById(string id)
        {
            if (!IsReady)
                throw new InvalidOperationException(
                    "AmmoStorage is not ready yet"
                );

            if (!ammoById.TryGetValue(id, out var ammo))
                throw new ArgumentException(
                    $"Ammo with ID '{id}' not found"
                );

            return ammo;
        }
    }
}
