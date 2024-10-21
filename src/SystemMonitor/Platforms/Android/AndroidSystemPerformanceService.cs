using Android.App;
using Android.Content;
using Android.OS;

namespace SystemMonitor.Services
{

    public class AndroidSystemPerformanceService : ISystemPerformanceService
    {
        private readonly ActivityManager _activityManager;
        private readonly Context _context;

        public AndroidSystemPerformanceService(Context context)
        {
            _context = context;
            _activityManager = (ActivityManager)_context.GetSystemService(Context.ActivityService);
        }

        public double GetCpuUsage()
        {
            // Reading from /proc/stat to get CPU load
            var cpuStats = File.ReadAllText("/proc/stat").Split(' ');
            double userTime = double.Parse(cpuStats[1]);
            double systemTime = double.Parse(cpuStats[3]);
            double idleTime = double.Parse(cpuStats[4]);
            double totalTime = userTime + systemTime + idleTime;

            return ((userTime + systemTime) / totalTime) * 100;  // CPU usage percentage
        }

        public double GetMemoryUsage()
        {
            ActivityManager.MemoryInfo memoryInfo = new ActivityManager.MemoryInfo();
            _activityManager.GetMemoryInfo(memoryInfo);

            double availableMemory = memoryInfo.AvailMem / (1024 * 1024);  // Convert to MB
            double totalMemory = memoryInfo.TotalMem / (1024 * 1024);      // Convert to MB

            return ((totalMemory - availableMemory) / totalMemory) * 100;  // Memory usage as percentage
        }

        public double GetBatteryLevel()
        {
            BatteryManager batteryManager = (BatteryManager)_context.GetSystemService(Context.BatteryService);
            int batteryLevel = batteryManager.GetIntProperty((int)BatteryProperty.Capacity);

            return batteryLevel;  // Battery level as percentage
        }

        public double GetGpuUsage()
        {
            throw new NotImplementedException();
        }

        public double GetPsuUsage()
        {
            throw new NotImplementedException();
        }

        public double GetStorageUsage()
        {
            throw new NotImplementedException();
        }

        public double GetNetworkUsage()
        {
            throw new NotImplementedException();
        }
    }
}

