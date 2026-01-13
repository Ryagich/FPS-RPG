using System.Collections;
using System.Diagnostics.CodeAnalysis;
using MessagePipe;
using Messages;
using Player;
using UniRx;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;

namespace Sounds
{
    public class SoundsManager : MonoBehaviour
    {
        private SoundsConfig soundsConfig;
        PlayerLifetimeScope playerLifetimeScope;
        
        private IObjectPool<AudioSource> sourcePool = null!;
        private float despawnTimer = 2.0f;
        private GameObject parent;
      
        private readonly CompositeDisposable disposables = new();

        [Inject]
        [SuppressMessage("ReSharper", "ParameterHidesMember")]
        private void Construct
            (
                SoundsConfig soundsConfig,
                ISubscriber<PlaySoundMessage> PlaySoundSubscriber
            )
        {
            this.soundsConfig = soundsConfig;
            
            sourcePool = new ObjectPool<AudioSource>(
                                                     Create, //Метод создания объектов
                                                     OnGet, //Действие при извлечении из пула
                                                     OnRelease, //Действие при возврате в пул
                                                     DestroySource, //Очистка объектов (опционально)
                                                     false, //Коллекция для отслеживания объектов не используется (опционально)
                                                     200, //Минимальный размер пула
                                                     2000 //Максимальный размер пула
                                                    );
            parent = new GameObject("Sounds Parent");

            PlaySoundSubscriber.Subscribe(PlaySound).AddTo(disposables);
        }

        public void SetPlayer
            (
                PlayerLifetimeScope playerLifetimeScope
            )
        {
            this.playerLifetimeScope = playerLifetimeScope;
        }

        private void PlaySound(PlaySoundMessage msg)
        {
            if (msg.SoundSettings.isUISound || Vector3.Distance(playerLifetimeScope.transform.position, msg.Position) 
                <= msg.SoundSettings.DistanceToPlay)
            {
                Get(msg.SoundSettings, msg.Position, msg.Parent);
            }
        }
        
        private AudioSource Get(SoundSettings settings, Vector3 position, Transform soundParent, bool needToReturn = true)
        {
            var source = sourcePool.Get();
            var sourceTrans = source.transform;

            sourceTrans.position = position;
            if (soundParent)
            {
                sourceTrans.SetParent(soundParent);
            }
            else
            {
                sourceTrans.SetParent(parent.transform);
            }
            source.spatialBlend = settings.isUISound ? 0 : 1;
            source.volume =  Random.Range(settings.volume.x, settings.volume.y);
            source.pitch = Random.Range(settings.pitch.x, settings.pitch.y);
            source.minDistance = settings.MinDistance;
            source.maxDistance = settings.MaxDistance;
            var clip = settings.Clips[Random.Range(0, settings.Clips.Count)];
            source.PlayOneShot(clip);

            if (needToReturn)
            {
                StartCoroutine(DespawnTimer(source));
            }

            return source;
        }
        
        private AudioSource Create() => Instantiate(soundsConfig.AudioSourcePrefab);
        private void OnRelease(AudioSource source) => source.transform.SetParent(parent.transform);
        private void OnGet(AudioSource source) { }
        private void DestroySource(AudioSource source) { }

        private IEnumerator DespawnTimer(AudioSource source)
        {
            yield return new WaitForSeconds(despawnTimer);
            sourcePool.Release(source);
        }
    }
}