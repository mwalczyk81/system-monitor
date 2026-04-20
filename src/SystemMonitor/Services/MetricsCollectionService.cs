using SystemMonitor.Models;
using static SystemMonitor.Constants;

namespace SystemMonitor.Services
{
    public class MetricsCollectionService : IMetricsCollectionService
    {
        private readonly ISystemPerformanceService _performanceService;
        private readonly Queue<ChartDataPoint> _cpuData = new();
        private readonly Queue<ChartDataPoint> _memoryData = new();
        private readonly Queue<ChartDataPoint> _gpuData = new();
        private readonly Queue<ChartDataPoint> _downloadData = new();
        private readonly Queue<ChartDataPoint> _uploadData = new();
        private readonly Queue<ChartDataPoint> _batteryData = new();
        private readonly Queue<ChartDataPoint> _storageData = new();
        private readonly Queue<ChartDataPoint> _psuData = new();
        private readonly List<Queue<ChartDataPoint>> _allQueues;
        private readonly object _dataLock = new();
        private System.Timers.Timer? _timer;
        private CancellationTokenSource? _cts;

        public event Action? DataUpdated;

        public IReadOnlyCollection<ChartDataPoint> CpuData => _cpuData;
        public IReadOnlyCollection<ChartDataPoint> MemoryData => _memoryData;
        public IReadOnlyCollection<ChartDataPoint> GpuData => _gpuData;
        public IReadOnlyCollection<ChartDataPoint> DownloadData => _downloadData;
        public IReadOnlyCollection<ChartDataPoint> UploadData => _uploadData;
        public IReadOnlyCollection<ChartDataPoint> BatteryData => _batteryData;
        public IReadOnlyCollection<ChartDataPoint> StorageData => _storageData;
        public IReadOnlyCollection<ChartDataPoint> PsuData => _psuData;

        public MetricsCollectionService(ISystemPerformanceService performanceService)
        {
            _performanceService = performanceService;
            _allQueues = [_cpuData, _memoryData, _gpuData, _downloadData, _uploadData, _batteryData, _storageData, _psuData];

            _cts = new CancellationTokenSource();
            _timer = new System.Timers.Timer(TimeSpan.FromSeconds(Polling.DefaultIntervalSeconds));
            _timer.Elapsed += async (_, _) => await CollectAsync();
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        public void Start() => _timer?.Start();

        public void Stop() => _timer?.Stop();

        public void Reset()
        {
            lock (_dataLock)
            {
                foreach (var queue in _allQueues)
                    queue.Clear();
            }
            DataUpdated?.Invoke();
        }

        public void SetInterval(int seconds)
        {
            if (_timer == null) return;
            _timer.Stop();
            _timer.Interval = TimeSpan.FromSeconds(seconds).TotalMilliseconds;
            _timer.Start();
        }

        private async Task CollectAsync()
        {
            if (_cts?.IsCancellationRequested == true) return;

            double cpuUsage = _performanceService.GetCpuUsage();
            double memoryUsage = _performanceService.GetMemoryUsage();
            double gpuUsage = _performanceService.GetGpuUsage();
            double batteryLevel = _performanceService.GetBatteryLevel();
            double storageUsage = _performanceService.GetStorageUsage();
            double psuUsage = _performanceService.GetPsuUsage();
            double downloadSpeed = _performanceService.GetDownloadSpeed();
            double uploadSpeed = _performanceService.GetUploadSpeed();

            lock (_dataLock)
            {
                var now = DateTime.Now;
                _cpuData.Enqueue(new ChartDataPoint { Time = now, Value = cpuUsage });
                _memoryData.Enqueue(new ChartDataPoint { Time = now, Value = memoryUsage });
                _gpuData.Enqueue(new ChartDataPoint { Time = now, Value = gpuUsage });
                _downloadData.Enqueue(new ChartDataPoint { Time = now, Value = downloadSpeed });
                _uploadData.Enqueue(new ChartDataPoint { Time = now, Value = uploadSpeed });
                _batteryData.Enqueue(new ChartDataPoint { Time = now, Value = batteryLevel });
                _storageData.Enqueue(new ChartDataPoint { Time = now, Value = storageUsage });
                _psuData.Enqueue(new ChartDataPoint { Time = now, Value = psuUsage });

                foreach (var queue in _allQueues.Where(q => q.Count > Polling.MaxDataPoints))
                    queue.Dequeue();
            }

            DataUpdated?.Invoke();

            await Task.CompletedTask;
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _timer?.Stop();
            _timer?.Dispose();
        }
    }
}
