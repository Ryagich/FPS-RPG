using UnityEngine;
using VContainer.Unity;

namespace Sounds
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class PlayerSetterInSoundsManager : IStartable
    {
        public PlayerSetterInSoundsManager
            (
                Transform transform, 
                SoundsManager soundsManager
            )
        {
            soundsManager.PlayerTransform = transform;
        }

        public void Start() { }
    }
}