using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Inventory.Ammo
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class AmmoStorage
    {
        //TODO: Костылим, братья.
        
        public bool isReadyToUse;
        public List<Ammo> Ammo { get; } = new();
        private readonly Dictionary<string, Ammo> ammoByID = new();
        
        //ОПа а шо это такое, а? Костыльчик? Не порядок
        public async UniTask WaitUntilReady()
        {
            await UniTask.WaitUntil(() => isReadyToUse);
        }
        
        public AmmoStorage()
        {
            LoadAmmo(configs =>
                     {
                         Ammo.Clear();
                         ammoByID.Clear();

                         foreach (var config in configs)
                         {
                             if (string.IsNullOrEmpty(config.ID))
                             {
                                 Debug.LogError($"BuildingConfig '{config.name}' has empty Id");
                                 continue;
                             }

                             if (ammoByID.ContainsKey(config.ID))
                             {
                                 Debug.LogError($"Duplicate BuildingConfig Id: {config.ID}");
                                 continue;
                             }

                             var entry = new Ammo(config, config.Max);
                             Ammo.Add(entry);
                             ammoByID.Add(config.ID, entry);
                         }
                         isReadyToUse = true;
                     });
        }
        
        private async void LoadAmmo(Action<List<AmmoConfig>> onComplete)
        {
            var handle = Addressables.LoadAssetsAsync<AmmoConfig>("AmmoConfig", null);
            var results = await handle.Task;
            onComplete?.Invoke(results as List<AmmoConfig>);
        }
    }
}