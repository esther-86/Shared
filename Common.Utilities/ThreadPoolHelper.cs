using System;
using System.Collections.Generic;
using System.Threading;

namespace RepositoryBuilder
{
    public class ThreadPoolHelper
    {
        // WaitHandle.WaitAll... can only accept an array with a maximum of this length
        public const int MAX_WAIT_HANDLES = 64;

        /// <summary>
        /// Usage: ThreadPoolHelper.For(0, pagesToCreate - 1, IRepository.MAX_RUNNING_THREADS, 
        /// true, (p, pageCreateDoneEvents)
        /// http://msdn.microsoft.com/en-us/library/ff963552.aspx
        /// For loop wrapper so that if the user wants to queue the for loop body in the thread pool
        /// they can do so by setting shouldQueueEntireBody = true.
        /// In this case, this class also has the option to wait until the threads are finished before returning
        /// If shouldQueueEntireBody = false, it should act similar to a sequential for loop,
        /// unless the user took advantage of the List<ManualResetEvent> parameter
        /// where the user manually create a thread in the body and adds the done event to the specified list
        /// to tell this helper function that there are threads that it needs to wait for.
        /// This class has a throttling option: MAX_WAIT_HANDLES because WaitHandle.WaitAll can't accept more than 64 handles
        /// and the throttling is because Laserfiche might be too busy with many existing actions
        /// </summary>
        /// <param name="fromInclusive"></param>
        /// <param name="toInclusive"></param>
        /// <param name="shouldQueueEntireBody"></param>
        /// <param name="body"></param>
        public static void For(int fromInclusive, int toInclusive, int maxRunning, 
            bool shouldQueueEntireBody, Action<int, List<ManualResetEvent>> body)
        {
            // Not using List<ManualResetEvent> doneEvents without throttling because will run into error described here:
            // http://www.codeproject.com/Articles/142341/Solved-The-number-of-WaitHandles-must-be-less-than
            // If does not throttle the program, will get:
            // The current request could not be performed because there are too many existing operations running. (9035)
            // at Laserfiche.RepositoryAccess.Document.GetPLockFromServer(HttpUrl uri, Session session)
            List<ManualResetEvent> doneEvents = new List<ManualResetEvent>();

            // Use counter variable because QueueUserWorkItem only queues the action and does not execute it yet
            // When it actually executes, the value of i already got incremented and so the value is not as expected
            // parentCounter - 1 because before action is invoked, this gets incremented.
            int parentCounter = fromInclusive - 1;
            for (int i = fromInclusive; i <= toInclusive; i++)
            {
                if (shouldQueueEntireBody)
                {
                    ManualResetEvent doneEvent = new ManualResetEvent(false);
                    doneEvents.Add(doneEvent);
                    ThreadPool.QueueUserWorkItem(delegate
                    {
                        try
                        {
                            int counter = Interlocked.Increment(ref parentCounter);
                            body.Invoke(counter, doneEvents);
                            doneEvent.Set();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error: {0}\n\t{1}", ex.Message, ex.StackTrace);
                        }
                    });
                }
                else
                {
                    body.Invoke(i, doneEvents);
                }

                // If we've hit the maxRunning count, wait until any of the running one finished
                // to start another running thread. 
                // Remove the done item from the list of items to wait, 
                // not to clear the list
                if (doneEvents.Count == maxRunning)
                {
                    int doneItemIndex = WaitHandle.WaitAny(doneEvents.ToArray());
                    doneEvents.Remove(doneEvents[doneItemIndex]);
                }
            }

            // Everything that should have been started, have been started
            // Wait for all threads to complete now
            ManualResetEvent[] waitEvents = doneEvents.ToArray();
            if (waitEvents.Length > 0)
            {
                WaitHandle.WaitAll(waitEvents);
            }
        }
    }
}
