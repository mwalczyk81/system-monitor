using System;
using System.IO;
using System.Linq;
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
            _context = context ?? throw new ArgumentNullException(nameof(context));

            // GetSystemService may return null on some devices/OS versions — use a safe cast and fail fast
            var am = _context.GetSystemService(Context.ActivityService) as ActivityManager;
            _activityManager = am ?? throw new InvalidOperationException("ActivityManager service is not available on this device.");
        }

        public double GetCpuUsage()
        {
            try
            {
                // Reading the first line from /proc/stat (aggregated CPU stats)
                var firstLine = File.ReadLines("/proc/stat").FirstOrDefault();
                if (string.IsNullOrWhiteSpace(firstLine))
                    return 0.0;

                var cpuStats = firstLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (cpuStats.Length < 5)
                    return 0.0;

                if (!double.TryParse(cpuStats[1], out double userTime))
                    return 0.0;
                if (!double.TryParse(cpuStats[3], out double systemTime))
                    return 0.0;
                if (!double.TryParse(cpuStats[4], out double idleTime))
                    return 0.0;

                double totalTime = userTime + systemTime + idleTime;
                if (totalTime <= 0)
                    return 0.0;

                return ((userTime + systemTime) / totalTime) * 100;  // CPU usage percentage
            }
            catch
            {
                return 0.0;
            }
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
            var batteryManager = _context.GetSystemService(Context.BatteryService) as BatteryManager;
            if (batteryManager == null)
            {
                // BatteryManager not available on this device — return -1 to indicate unknown
                return -1;
            }

            int batteryLevel = batteryManager.GetIntProperty((int)BatteryProperty.Capacity);
            return batteryLevel;  // Battery level as percentage
        }

        public double GetGpuUsage() => 0.0;

        public double GetPsuUsage() => 0.0;

        public double GetStorageUsage() => 0.0;

        public double GetDownloadSpeed() => 0.0;

        public double GetUploadSpeed() => 0.0;
    }
}

