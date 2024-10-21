using System.Diagnostics;

namespace SystemMonitor.Services
{
    public class LinuxSystemPerformanceService : ISystemPerformanceService
    {
        private static readonly char[] separator = [' ', ':'];

        public double GetBatteryLevel()
        {
            var batteryLevel = File.ReadAllText("/sys/class/power_supply/BAT0/capacity");
            if (double.TryParse(batteryLevel.Trim(), out var battery))
            {
                return battery;
            }

            return 0;
        }

        public double GetCpuUsage()
        {
            var cpuStats = File.ReadAllText("/proc/stat").Split(" ", StringSplitOptions.RemoveEmptyEntries);

            if (cpuStats.Length > 4 && double.TryParse(cpuStats[1], out double userTime) && double.TryParse(cpuStats[3], out double systemTime) && double.TryParse(cpuStats[4], out double idleTime))
            {
                double totalTime = userTime + systemTime + idleTime;

                if (totalTime == 0) return 0;

                return (userTime + systemTime) / totalTime * 100;
            }

            return 0;
        }

        public double GetDownloadSpeed()
        {
            throw new NotImplementedException();
        }

        public double GetGpuUsage()
        {
            var output = ExecuteBashCommand("nvidia-smi --query-gpu=utilization.gpu --format=csv,noheader,nounits");

            if (double.TryParse(output.Trim(), out var result))
            {
                return result;
            }

            return 0;
        }

        public double GetMemoryUsage()
        {
            var memInfo = File.ReadAllText("/proc/meminfo").Split("\n");
            if (double.TryParse(memInfo[0].Split(":")[1].Trim().Split(" ")[0], out var totalMemory) && double.TryParse(memInfo[1].Split(":")[1].Trim().Split(" ")[0], out var freeMemory))
            {
                return 100.0 - ((freeMemory / totalMemory) * 100.0);
            }

            return 0;
        }

        public double GetPsuUsage()
        {
            var powerConsumption = File.ReadAllText("/sys/class/power_supply/BAT0/power_now");
            if (double.TryParse(powerConsumption.Trim(), out var power))
            {
                return power / 1_000_000.0;
            }

            return 0;
        }

        public double GetStorageUsage()
        {
            var output = ExecuteBashCommand("df --output=pcent / | tail -1");
            var percentageUsed = output.Replace("%", "").Trim();

            return double.Parse(percentageUsed);
        }

        public double GetUploadSpeed()
        {
            throw new NotImplementedException();
        }

        private string ExecuteBashCommand(string command)
        {
            var escapedArgs = command.Replace("\"", "\\\"");
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            process.Start();

            string result = process.StandardOutput.ReadToEnd();

            process.WaitForExit();

            return result;
        }
    }
}
