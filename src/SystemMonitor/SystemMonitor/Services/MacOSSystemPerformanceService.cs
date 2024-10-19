using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace SystemMonitor.Services
{

    public class MacOSSystemPerformanceService : ISystemPerformanceService
    {
        public double GetBatteryLevel()
        {
            throw new NotImplementedException();
        }

        public double GetCpuUsage()
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "/bin/sh",
                Arguments = "-c \"top -l 1 | grep 'CPU usage'\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(startInfo);
            using var reader = process.StandardOutput;
            string result = reader.ReadToEnd();
            string[] parts = result.Split(',');
            double userCpu = double.Parse(parts[0].Split(' ')[2].Replace("%", ""));
            double systemCpu = double.Parse(parts[1].Split(' ')[2].Replace("%", ""));
            return userCpu + systemCpu;  // Total CPU usage
        }

        public double GetGpuUsage()
        {
            throw new NotImplementedException();
        }

        public double GetMemoryUsage()
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "/bin/sh",
                Arguments = "-c \"vm_stat | grep 'Pages free'\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(startInfo);
            using var reader = process.StandardOutput;
            string result = reader.ReadToEnd();
            double freePages = double.Parse(result.Split(':')[1].Trim().Replace(".", "")) * 4096;
            double totalMemory = Process.GetCurrentProcess().WorkingSet64;

            return 100.0 - ((freePages / totalMemory) * 100.0);  // Used memory percentage
        }
    }

}
