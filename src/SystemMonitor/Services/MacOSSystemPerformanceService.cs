using System;
using System.Diagnostics;

namespace SystemMonitor.Services
{
    public class MacOSSystemPerformanceService : ISystemPerformanceService
    {
        // Battery level using `pmset -g batt`
        public double GetBatteryLevel()
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "/bin/sh",
                Arguments = "-c \"pmset -g batt | grep -Eo '[0-9]+%' | tr -d '%'\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(startInfo);
            using var reader = process.StandardOutput;
            string result = reader.ReadToEnd();
            if (double.TryParse(result.Trim(), out double batteryLevel))
            {
                return batteryLevel;
            }
            return -1;  // Return -1 if unable to fetch battery data
        }

        // CPU usage using `top`
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

            // Extract the user and system CPU usage
            string[] parts = result.Split(',');
            if (parts.Length >= 2)
            {
                if (double.TryParse(parts[0].Split(' ')[2].Replace("%", ""), out double userCpu) &&
                    double.TryParse(parts[1].Split(' ')[2].Replace("%", ""), out double systemCpu))
                {
                    return userCpu + systemCpu;  // Total CPU usage (user + system)
                }
            }

            return 0;  // Return 0 if parsing fails
        }

        public double GetGpuUsage()
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "/bin/sh",
                Arguments = "-c \"ioreg -l | grep -i 'gpu'\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            using var reader = process.StandardOutput;
            string result = reader.ReadToEnd();

            // Since `ioreg` doesn't directly provide GPU utilization, you'll need to parse the output to find relevant information.
            // This example looks for general GPU-related info, but you might need a more advanced method for real-time usage.

            if (!string.IsNullOrEmpty(result))
            {
                // Return some basic information for now (like 1 to indicate GPU is active)
                return 1;  // This is just a placeholder until a better method is implemented.
            }

            return 0;  // Return 0 if no GPU information is available.
        }


        // Memory usage using `vm_stat` and `sysctl`
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

            if (double.TryParse(result.Split(':')[1].Trim().Replace(".", ""), out double freePages))
            {
                freePages *= 4096;  // Convert pages to bytes (assuming 4KB pages)

                // Get total memory from `sysctl hw.memsize`
                var totalMemoryInfo = new ProcessStartInfo
                {
                    FileName = "/bin/sh",
                    Arguments = "-c \"sysctl hw.memsize | awk '{print $2}'\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var totalMemoryProcess = Process.Start(totalMemoryInfo);
                using var totalMemoryReader = totalMemoryProcess.StandardOutput;
                string totalMemoryResult = totalMemoryReader.ReadToEnd();

                if (double.TryParse(totalMemoryResult.Trim(), out double totalMemory))
                {
                    return 100.0 - ((freePages / totalMemory) * 100.0);  // Memory usage percentage
                }
            }

            return 0;  // Return 0 if parsing fails
        }

        // Storage usage using `df`
        public double GetStorageUsage()
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "/bin/sh",
                Arguments = "-c \"df / | tail -1 | awk '{print $5}' | tr -d '%'\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(startInfo);
            using var reader = process.StandardOutput;
            string result = reader.ReadToEnd();

            if (double.TryParse(result.Trim(), out double storageUsage))
            {
                return storageUsage;  // Storage usage percentage
            }

            return 0;  // Return 0 if unable to fetch storage data
        }

        public double GetPsuUsage()
        {
            throw new NotImplementedException();  // PSU monitoring not typically available on macOS
        }

        public double GetDownloadSpeed()
        {
            throw new NotImplementedException();
        }

        public double GetUploadSpeed()
        {
            throw new NotImplementedException();
        }
    }
}
