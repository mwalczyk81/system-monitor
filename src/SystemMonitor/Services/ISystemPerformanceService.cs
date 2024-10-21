namespace SystemMonitor.Services
{
    public interface ISystemPerformanceService
    {
        double GetCpuUsage();
        double GetMemoryUsage();
        double GetBatteryLevel();
        double GetGpuUsage();
        public double GetPsuUsage();
        public double GetStorageUsage();
        public double GetDownloadSpeed();
        public double GetUploadSpeed();
    }
}