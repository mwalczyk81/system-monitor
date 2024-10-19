
namespace SystemMonitor.Services
{

    public class LinuxSystemPerformanceService : ISystemPerformanceService
    {
        public double GetBatteryLevel()
        {
            throw new NotImplementedException();
        }

        public double GetCpuUsage()
        {
            var cpuStats = File.ReadAllText("/proc/stat").Split(" ");
            double userTime = double.Parse(cpuStats[1]);
            double systemTime = double.Parse(cpuStats[3]);
            double idleTime = double.Parse(cpuStats[4]);
            double totalTime = userTime + systemTime + idleTime;

            return ((userTime + systemTime) / totalTime) * 100;  // CPU usage percentage
        }

        public double GetGpuUsage()
        {
            throw new NotImplementedException();
        }

        public double GetMemoryUsage()
        {
            var memInfo = File.ReadAllText("/proc/meminfo").Split("\n");
            double totalMemory = double.Parse(memInfo[0].Split(":")[1].Trim().Split(" ")[0]);
            double freeMemory = double.Parse(memInfo[1].Split(":")[1].Trim().Split(" ")[0]);

            return 100.0 - ((freeMemory / totalMemory) * 100.0);  // Used memory percentage
        }
    }

}
