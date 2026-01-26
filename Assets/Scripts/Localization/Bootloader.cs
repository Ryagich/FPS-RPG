using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Messages;
using UnityEngine;
using VContainer.Unity;
using YG;
using YG.Insides;

namespace Localization
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Bootloader : IStartable
    {
        private readonly IPublisher<TranslationStateChangedMessage> translationStateChangedMessagePublisher;
        private bool initialized;

        public Bootloader(IPublisher<TranslationStateChangedMessage> translationStateChangedMessagePublisher)
        {
            this.translationStateChangedMessagePublisher = translationStateChangedMessagePublisher;
        }
        
        public async void Start()
        {
            await StartAsync();
        }
        
        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            Debug.Log($"Bootloader starting: YG2Enabled={YG2.isSDKEnabled}");
            initialized = YG2.isSDKEnabled;
            
            YG2.onGetSDKData += () =>
                                {
                                    Debug.Log($"SDK data loaded {DateTimeOffset.UtcNow} : {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");
                                    Debug.Log($"Configuring sentry: UserId='{YG2.player.id}', Username='{YG2.player.name}'");
                                    initialized = true;
                                };
            
        
            Debug.Log("Waiting for SDK data");
            while (!initialized)
            {
                initialized = YG2.isSDKEnabled;
                await UniTask.DelayFrame(1, cancellationToken: cancellation);
            }
            
            Debug.Log("Consider SDK initialized");
            YG2.InitMetrica();
            
            YG2.GetAuth();
            YG2.GetLanguage();
            Debug.Log($"Configuring language: '{YG2.lang}'");
            await LocalizationHelper.InvalidateAsync(YG2.lang);
            translationStateChangedMessagePublisher.Publish(new TranslationStateChangedMessage(true));
            YGInsides.LoadProgress();
        }
    }
}