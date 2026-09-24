using System.Threading.Tasks;
using YG;

namespace Localization
{
    public static class YG2Awaiter
    {
        private static TaskCompletionSource<bool> completion;

        public static Task WaitForSDKDataAsync()
        {
            if (YG2.isSDKEnabled)
            {
                return Task.CompletedTask;
            }

            completion ??= new TaskCompletionSource<bool>();

            void Handler()
            {
                YG2.onGetSDKData -= Handler;
                completion.TrySetResult(true);
            }

            YG2.onGetSDKData += Handler;
            return completion.Task;
        }
    }
}
