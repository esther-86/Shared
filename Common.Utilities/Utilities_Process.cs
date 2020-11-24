#define USE_CMD_MODE

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Common.Utilities
{
    public class Utilities_Process
    {
        public static void LaunchCommandLineAndWait_Async(string args, string fileName, TimeSpan timespanToWait)
        {
            ThreadPool.QueueUserWorkItem(LaunchCommandLineAndWait, 
                new object[] 
                {
                    args, fileName, timespanToWait
                });
        }

        protected static void LaunchCommandLineAndWait(Object stateInfo)
        {
            object[] array = stateInfo as object[];
            string args = (string)array[0];
            string fileName = (string)array[1];
            TimeSpan timespanToWait = (TimeSpan)array[2];
            LaunchCommandLineAndWait(args, fileName, timespanToWait);
        }

        public static string LaunchCommandLineAndWait(string args, string fileName, TimeSpan timespanToWait, bool throwException = true)
        {
#if USE_CMD_MODE
            long tickValue = DateTime.Now.Ticks;
            string outputTxtFile = string.Format("{0}.{1}.output.txt", fileName, tickValue);
            string processOutput = "";
            try
            {
                string cmdArgs = string.Format("\"{0}\" {1} 2> \"{2}\"", fileName, args, outputTxtFile);
                using (Process process = Process.Start("CMD.exe", string.Format("/c \"{0}\"", cmdArgs)))
                {
                    process.WaitForExit((int)timespanToWait.TotalMilliseconds);
                    if (!process.HasExited)
                        process.Kill();

                    int pExitCode = process.ExitCode;
                    if (pExitCode != 0 && throwException)
                    {
                        throw new Exception(string.Format(
                            "Process (pid: {0}) exit code indicates error (not 0):\r\n\tExit code: {1} Tick value: {2}",
                            process.Id, pExitCode, tickValue));
                    }
                }
                processOutput = File.ReadAllText(outputTxtFile);
            }
            finally
            {
                try { File.Delete(outputTxtFile); }
                catch { }
            }
            return processOutput;
#else
            ProcessStartInfo childProcessStartInfo = new ProcessStartInfo();
            childProcessStartInfo.FileName = fileName;
            childProcessStartInfo.Arguments = args;
            childProcessStartInfo.UseShellExecute = false;
            childProcessStartInfo.RedirectStandardOutput = true;

            using (Process process = Process.Start(childProcessStartInfo))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    process.WaitForExit((int)timespanToWait.TotalMilliseconds);
                    if (!process.HasExited)
                    { process.Kill(); }

                    int pExitCode = process.ExitCode;
                    if (pExitCode != 0)
                    {
                        // Read output from child process
                        string childProcessOutput = "";
                        try { childProcessOutput = reader.ReadToEnd(); }
                        catch (Exception ex)
                        { childProcessOutput = "Cannot read output from child process " + ex.Message; }

                        throw new Exception(string.Format("Process (pid: {0}) exit code: {1}\nProcess' output: {2}",
                          process.Id, pExitCode, childProcessOutput));
                    }
                }
            }
#endif
        }
    }
}
