using Sounds;

namespace Dependencies
{
    public readonly struct MovementSoundSettings
    {
        public SoundConfig Value { get; }

        public MovementSoundSettings(SoundConfig value) => Value = value;
    }
}
