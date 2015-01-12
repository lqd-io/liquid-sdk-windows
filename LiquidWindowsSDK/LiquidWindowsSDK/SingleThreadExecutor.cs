using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.System.Threading;

namespace LiquidWindowsSDK
{
    internal class SingleThreadExecutor
    {
        internal static SemaphoreSlim _syncLock = new SemaphoreSlim(1);

        internal static async Task RunAsync(Func<Task> asyncAction)
        {
            await ThreadPool.RunAsync(async operation =>
            {
                await _syncLock.WaitAsync();
                try
                {
                    await asyncAction();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
                _syncLock.Release();
            });
        }

        internal static async Task RunAsync(Action asyncAction)
        {
            await ThreadPool.RunAsync(async operation =>
            {
                await _syncLock.WaitAsync();
                try
                {
                    asyncAction();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
                _syncLock.Release();
            });
        }
    }
}
