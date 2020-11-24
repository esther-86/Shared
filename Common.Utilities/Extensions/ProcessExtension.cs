using System;
using System.Diagnostics;
using System.Management;

namespace Common.Utilities
{
    public static class ProcessExtension
    {
        public static bool NoLongerExists(this string processName, string remoteMachineName = null, bool killProcessIfExists = false)
        {
            if (remoteMachineName == null)
                remoteMachineName = Environment.MachineName;

            Process[] matchingProcesses = Process.GetProcessesByName(processName, remoteMachineName);
            if (killProcessIfExists)
            {
                foreach (Process p in matchingProcesses)
                {
                    Error.Ignore(() => p.KillRemoteOrNot(), string.Format("trying to kill process {0} on {1}", processName, remoteMachineName));
                }
            }
            return (matchingProcesses.Length == 0);
        }

        // http://stackoverflow.com/questions/348112/kill-a-process-on-a-remote-machine-in-c-sharp
        public static void KillRemoteOrNot(this Process processToKill)
        {
            if (processToKill.MachineName.ToLower().Equals(Environment.MachineName.ToLower()))
            {
                processToKill.Kill();
                return;
            }

            ManagementScope managementScope = new ManagementScope(string.Format(@"\\{0}\root\cimv2", processToKill.MachineName));
            managementScope.Connect();

            // WMI query
            SelectQuery query = new SelectQuery("select * from Win32_process where name = '" + processToKill.ProcessName + "'");
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(managementScope, query))
            {
                ManagementObjectCollection processes = searcher.Get();
                foreach (ManagementObject process in processes)
                {
                    process.InvokeMethod("Terminate", null);
                }
            }
        }
    }
}
