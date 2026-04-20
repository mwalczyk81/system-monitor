using SystemMonitor.Models;

namespace SystemMonitor.Services
{
    public interface IMetricsCollectionService : IDisposable
    {
        event Action? DataUpdated;

        IReadOnlyCollection<ChartDataPoint> CpuData { get; }
        IReadOnlyCollection<ChartDataPoint> MemoryData { get; }
        IReadOnlyCollection<ChartDataPoint> GpuData { get; }
        IReadOnlyCollection<ChartDataPoint> DownloadData { get; }
        IReadOnlyCollection<ChartDataPoint> UploadData { get; }
        IReadOnlyCollection<ChartDataPoint> BatteryData { get; }
        IReadOnlyCollection<ChartDataPoint> StorageData { get; }
        IReadOnlyCollection<ChartDataPoint> PsuData { get; }

        void Start();
        void Stop();
        void Reset();
        void SetInterval(int seconds);
    }
}
