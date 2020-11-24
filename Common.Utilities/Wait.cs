using System;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Utilities
{
    public class Wait
    {
        public static void Until(Action<CancellationToken> action, string waitDesc, TimeSpan? timeout = null, bool throwTimeoutException = true)
        {
            Func<CancellationToken, bool> func = (cancellationToken) =>
            {
                action.Invoke(cancellationToken);
                return true;
            };
            Wait.Until(func, waitDesc, timeout, throwTimeoutException);
        }

        // Abort a task to not litter cancellationToken.ThrowIfCancellationRequested(); everywhere, especially when a validator code takes a long time to run
        // TODO: Re-implement because some people from forum said that it's not good, but don't understand why. Also, this needs more testing with a real long test
        // http://stackoverflow.com/a/21715122/5885721 
        // https://msdn.microsoft.com/en-us/library/dd997396(v=vs.110).aspx
        public static TimeSpan DefaultTimeoutSleep = TimeSpan.FromMilliseconds(500);
        public static void Until(Func<CancellationToken, bool> func, string waitDesc, TimeSpan? timeout = null, bool throwTimeoutException = true, bool abortUponTimeout = true)
        {
            CancellationTokenSource canceller = new CancellationTokenSource();
            CancellationToken cancellationToken = canceller.Token;

            Task task = Task.Factory.StartNew(() =>
            {
                using (canceller.Token.Register(Thread.CurrentThread.Abort))
                {
                    bool done = false;
                    do
                    {
                        // Were we already canceled?
                        cancellationToken.ThrowIfCancellationRequested();

                        done = func.Invoke(cancellationToken);
                        if (!done)
                        {
                            // If the task didn't return true, need to loop again
                            //      add a timeout to slow down the loop
                            Thread.Sleep(DefaultTimeoutSleep);
                        }
                    }
                    while (!done);
                }
            }, 
            cancellationToken);

            if (!timeout.HasValue)
                task.Wait();
            else
            {
                bool taskFinished = task.Wait(timeout.Value);
                if (!taskFinished)
                {
                    canceller.Cancel();
                    if (throwTimeoutException)
                        throw new TimeoutException("Timed out when waiting for " + waitDesc);
                }
            }
        }
    }
}
