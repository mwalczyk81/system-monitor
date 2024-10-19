namespace SystemMonitor.Services
{
    public interface ISystemPerformanceService
    {
        double GetCpuUsage();
        double GetMemoryUsage();
        double GetBatteryLevel();
        double GetGpuUsage();
    }
}