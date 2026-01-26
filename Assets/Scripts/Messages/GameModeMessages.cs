namespace Messages
{
    public readonly struct TranslationStateChangedMessage
    {
        public readonly bool IsReady;
        public TranslationStateChangedMessage(bool isReady)
        {
            IsReady = isReady;
        }
    }
}