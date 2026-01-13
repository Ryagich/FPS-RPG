using Sounds;
using UnityEngine;

namespace Messages
{
    public readonly struct PlaySoundMessage
    {
        public readonly SoundSettings SoundSettings;
        public readonly Vector3 Position;
        public readonly Transform Parent;
        
        public PlaySoundMessage(SoundSettings soundSettings, Vector3 position, Transform parent)
        {
            SoundSettings = soundSettings;
            Position = position;
            Parent = parent;
        }
    }
}