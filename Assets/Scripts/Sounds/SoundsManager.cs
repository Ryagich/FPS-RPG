using System.Collections.Generic;
using MessagePipe;
using Messages;
using UniRx;
using UnityEngine;
using UnityEngine.Pool;
using VContainer.Unity;

namespace Sounds
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class SoundsManager : IFixedTickable
    {
        public Transform PlayerTransform;
        
        private readonly SoundsConfig soundsConfig;
        
        private readonly IObjectPool<AudioSource> sourcePool;
        private readonly float despawnTimer = 2.0f;

        private readonly GameObject parent;
        private readonly CompositeDisposable disposables = new();

        private readonly List<ActiveSound> activeSounds = new();

        private struct ActiveSound
        {
            public AudioSource Source;
            public float TimeLeft;
        }

        private SoundsManager
        (
            SoundsConfig soundsConfig,
            ISubscriber<PlaySoundMessage> playSoundSubscriber
        )
        {
            this.soundsConfig = soundsConfig;

            sourcePool = new ObjectPool<AudioSource>(
                                                     Create,
                                                     OnGet,
                                                     OnRelease,
                                                     DestroySource,
                                                     false,
                                                     200,
                                                     2000
                                                    );
            parent = new GameObject("Sounds Parent");
            playSoundSubscriber
                .Subscribe(PlaySound)
                .AddTo(disposables);
        }

        public void FixedTick()
        {
            var dt = Time.fixedDeltaTime;

            for (var i = activeSounds.Count - 1; i >= 0; i--)
            {
                var sound = activeSounds[i];
                sound.TimeLeft -= dt;

                if (sound.TimeLeft <= 0f)
                {
                    sourcePool.Release(sound.Source);
                    activeSounds.RemoveAt(i);
                }
                else
                {
                    activeSounds[i] = sound;
                }
            }
        }

        private void PlaySound(PlaySoundMessage msg)
        {
            if (!PlayerTransform)
                return;
            if (msg.SoundSettings.isUISound ||
                Vector3.Distance(PlayerTransform.position, msg.Position)
             <= msg.SoundSettings.DistanceToPlay)
            {
                Spawn(msg.SoundSettings, msg.Position, msg.Parent);
            }
        }

        private void Spawn(SoundSettings settings, Vector3 position, Transform soundParent)
        {
            var source = sourcePool.Get();
            var trans = source.transform;

            trans.position = position;
            trans.SetParent(soundParent ? soundParent : parent.transform);

            source.spatialBlend = settings.isUISound ? 0 : 1;
            source.volume = Random.Range(settings.volume.x, settings.volume.y);
            source.pitch = Random.Range(settings.pitch.x, settings.pitch.y);
            source.minDistance = settings.MinDistance;
            source.maxDistance = settings.MaxDistance;

            var clip = settings.Clips[Random.Range(0, settings.Clips.Count)];
            source.PlayOneShot(clip);

            activeSounds.Add(new ActiveSound
            {
                Source = source,
                TimeLeft = despawnTimer
            });
        }

        private AudioSource Create() =>
            Object.Instantiate(soundsConfig.AudioSourcePrefab);

        private void OnGet(AudioSource source) { }

        private void OnRelease(AudioSource source)
        {
            source.Stop();
            source.transform.SetParent(parent.transform);
        }

        private void DestroySource(AudioSource source)
        {
            Object.Destroy(source.gameObject);
        }
    }
}
