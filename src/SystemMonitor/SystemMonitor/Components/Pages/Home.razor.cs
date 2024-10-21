using ApexCharts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemMonitor.Components.Charts;
using SystemMonitor.Models;
using static SystemMonitor.Constants;

namespace SystemMonitor.Components.Pages
{
    public partial class Home
    {
        private readonly List<ChartDataPoint> cpuData = [];
        private readonly List<ChartDataPoint> memoryData = [];
        private readonly List<ChartDataPoint> gpuData = [];
        private readonly List<ChartDataPoint> downloadData = [];
        private readonly List<ChartDataPoint> uploadData = [];
        private readonly List<ChartDataPoint> batteryData = [];
        private readonly List<ChartDataPoint> storageData = [];
        private readonly List<ChartDataPoint> psuData = [];
        private List<List<ChartDataPoint>> allDataLists = [];

        private UsageChart cpuUsageChart = new();
        private UsageChart memoryUsageChart = new();
        private UsageChart gpuUsageChart = new();
        private UsageChart downloadChart = new();
        private UsageChart uploadChart = new();
        private UsageChart batteryUsageChart = new();
        private UsageChart storageUsageChart = new();
        private UsageChart psuUsageChart = new();
        private List<UsageChart> allCharts = [];

        private System.Timers.Timer? timer = new();
        private bool isMonitoring = true;
        private int updateInterval = 1;

        private ApexChartOptions<ChartDataPoint> CpuChartOptions { get; set; } = new();
        private ApexChartOptions<ChartDataPoint> GpuChartOptions { get; set; } = new();
        private ApexChartOptions<ChartDataPoint> MemoryChartOptions { get; set; } = new();
        private ApexChartOptions<ChartDataPoint> DownloadChartOptions { get; set; } = new();
        private ApexChartOptions<ChartDataPoint> UploadChartOptions { get; set; } = new();
        private ApexChartOptions<ChartDataPoint> BatteryChartOptions { get; set; } = new();
        private ApexChartOptions<ChartDataPoint> StorageChartOptions { get; set; } = new();
        private ApexChartOptions<ChartDataPoint> PsuChartOptions { get; set; } = new();

        private static ApexChartOptions<ChartDataPoint> GetCreateChartOptions(string title, string yAxisFormat, string color)
        {
            return new()
            {
                Chart = new Chart
                {
                    Background = "#f4f4f4", // Light gray background
                    Toolbar = new ApexCharts.Toolbar
                    {
                        Show = true
                    },
                    Zoom = new Zoom
                    {
                        Enabled = false
                    },
                    Animations = new Animations
                    {
                        Enabled = true,
                        Easing = ApexCharts.Easing.Easeinout,
                        Speed = 500,
                        AnimateGradually = new AnimateGradually
                        {
                            Enabled = true,
                            Delay = 150
                        },
                        DynamicAnimation = new DynamicAnimation
                        {
                            Enabled = true,
                            Speed = 350
                        }
                    }
                },
                Title = new Title()
                {
                    Text = title,
                    Align = Align.Center,  
                    Style = new TitleStyle
                    {
                        FontSize = "20px",  
                        FontWeight = "bold" 
                    }
                },
                Stroke = new Stroke
                {
                    Curve = Curve.Smooth,
                    Width = 3
                },
                Colors = new List<string> { color },
                Fill = new Fill
                {
                    Type = FillType.Gradient,
                    Gradient = new FillGradient
                    {
                        Shade = GradientShade.Light,
                        Type = GradientType.Vertical,
                        ShadeIntensity = 0.5,
                        OpacityFrom = 0.7,
                        OpacityTo = 0.3,
                    }
                },
                Yaxis =
                [
                    new YAxis
                        {
                            Labels = new YAxisLabels
                            {
                                Formatter = yAxisFormat,
                                Style = new AxisLabelStyle
                                {
                                    FontSize = "14px",
                                    Colors = "#333"
                                }
                            }

                        }
                 ],
                Xaxis = new XAxis
                {
                    Labels = new XAxisLabels
                    {
                        Show = false,
                    }
                },
                Tooltip = new Tooltip
                {
                    Enabled = false
                },
                Grid = new ApexCharts.Grid
                {
                    BorderColor = "#e0e0e0",
                    Row = new GridRow
                    {
                        Colors = ["#f0f0f0", "#ffffff"],
                        Opacity = 0.5
                    }
                }
            };
        }

        protected override void OnInitialized()
        {
            timer = new System.Timers.Timer(TimeSpan.FromSeconds(updateInterval));
            timer.Elapsed += async (sender, e) => await UpdatePerformanceData();
            timer.AutoReset = true;
            timer.Enabled = true;

            CpuChartOptions = GetCreateChartOptions("CPU Usage", ChartFormats.YAxis.Percent, "#ff0000"); 
            MemoryChartOptions = GetCreateChartOptions("Memory Usage", ChartFormats.YAxis.Percent, "#00ff00"); 
            GpuChartOptions = GetCreateChartOptions("GPU Usage", ChartFormats.YAxis.Percent, "#0000ff");
            DownloadChartOptions = GetCreateChartOptions("Download Speed", ChartFormats.YAxis.BytesPerSecond, "#ffcc00");
            UploadChartOptions = GetCreateChartOptions("Upload Speed", ChartFormats.YAxis.BytesPerSecond, "#ffcc00");
            BatteryChartOptions = GetCreateChartOptions("Battery Level", ChartFormats.YAxis.Percent, "#ff6600");  
            StorageChartOptions = GetCreateChartOptions("Storage Usage", ChartFormats.YAxis.Percent, "#9933ff");  
            PsuChartOptions = GetCreateChartOptions("PSU Usage", ChartFormats.YAxis.Percent, "#33ccff");  

            allDataLists = [cpuData, memoryData, gpuData, downloadData, uploadData, batteryData, storageData, psuData];
        }

        private void PauseMonitoring() => isMonitoring = false;
        private void StartMonitoring() => isMonitoring = true;

        private void OnUpdateIntervalChanged(int newInterval)
        {
            updateInterval = newInterval;

            // Restart the timer with the new interval
            if (timer != null)
            {
                timer.Stop();
                timer.Interval = updateInterval * 1000;
                timer.Start();
            }
        }

        private async Task UpdatePerformanceData()
        {

            if (!isMonitoring) return;

            double cpuUsage = PerformanceService.GetCpuUsage();
            double memoryUsage = PerformanceService.GetMemoryUsage();
            double gpuUsage = PerformanceService.GetGpuUsage();
            double batteryLevel = PerformanceService.GetBatteryLevel();
            double storageUsage = PerformanceService.GetStorageUsage();
            double psuUsage = PerformanceService.GetPsuUsage();
            double downloadSpeed = PerformanceService.GetDownloadSpeed();   
            double uploadSpeed = PerformanceService.GetUploadSpeed();

            cpuData.Add(new ChartDataPoint { Time = DateTime.Now, Value = cpuUsage });
            memoryData.Add(new ChartDataPoint { Time = DateTime.Now, Value = memoryUsage });
            gpuData.Add(new ChartDataPoint { Time = DateTime.Now, Value = gpuUsage });
            downloadData.Add(new ChartDataPoint { Time = DateTime.Now, Value = downloadSpeed });
            uploadData.Add(new ChartDataPoint { Time = DateTime.Now, Value = uploadSpeed });
            batteryData.Add(new ChartDataPoint { Time = DateTime.Now, Value = batteryLevel });
            storageData.Add(new ChartDataPoint { Time = DateTime.Now, Value = storageUsage });
            psuData.Add(new ChartDataPoint { Time = DateTime.Now, Value = psuUsage });


            // Limit the history to the last 100 data points
            foreach (var dataList in allDataLists.Where(x => x.Count > 100))
            {
                dataList.RemoveAt(0);
            }

            allCharts = [cpuUsageChart, memoryUsageChart, gpuUsageChart, downloadChart, uploadChart, batteryUsageChart, storageUsageChart, psuUsageChart];

            var updateTasks = new List<Task>();

            foreach (var chart in allCharts)
            {
                updateTasks.Add(chart.UpdateChartAsync());
            }


            await Task.WhenAll(updateTasks);

            await InvokeAsync(StateHasChanged);
        }

        private void ResetCharts()
        {
            cpuData.Clear();
            memoryData.Clear();
            gpuData.Clear();
            StateHasChanged();
        }

        public void Dispose()
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
            }
        }
    }
}
