using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;
using YG;
using YG.Insides;

namespace Localization
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Bootloader : IStartable
    {
        private readonly BootCompletion bootCompletion;

        public Bootloader(BootCompletion bootCompletion)
        {
            this.bootCompletion = bootCompletion;
        }
        
        public async void Start()
        {
            await StartAsync();
        }
        
        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            Debug.Log($"Bootloader starting: YG2Enabled={YG2.isSDKEnabled}");
            await YG2Awaiter.WaitForSDKDataAsync();

            YG2.InitMetrica();
            YG2.GetAuth();
            YG2.GetLanguage();
            Debug.Log($"Configuring language: '{YG2.lang}'");
            await LocalizationHelper.InvalidateAsync(YG2.lang);
            YGInsides.LoadProgress();
            YG2.GameReadyAPI();

            bootCompletion.Signal();
        }
    }
}
