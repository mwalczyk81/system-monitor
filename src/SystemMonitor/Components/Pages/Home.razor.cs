using ApexCharts;
using Microsoft.AspNetCore.Components;
using SystemMonitor.Components.Charts;
using SystemMonitor.Models;
using SystemMonitor.Services;
using static SystemMonitor.Constants;

namespace SystemMonitor.Components.Pages
{
    public partial class Home : IDisposable
    {
        [Inject] private IMetricsCollectionService MetricsService { get; set; } = default!;

        private UsageChart cpuUsageChart = new();
        private UsageChart memoryUsageChart = new();
        private UsageChart gpuUsageChart = new();
        private UsageChart downloadChart = new();
        private UsageChart uploadChart = new();
        private UsageChart batteryUsageChart = new();
        private UsageChart storageUsageChart = new();
        private UsageChart psuUsageChart = new();
        private List<UsageChart> allCharts = [];

        private int updateInterval = Polling.DefaultIntervalSeconds;

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
                    Background = "#f4f4f4",
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
            MetricsService.DataUpdated += OnDataUpdated;

            CpuChartOptions = GetCreateChartOptions("CPU Usage", ChartFormats.YAxis.Percent, "#ff0000");
            MemoryChartOptions = GetCreateChartOptions("Memory Usage", ChartFormats.YAxis.Percent, "#00ff00");
            GpuChartOptions = GetCreateChartOptions("GPU Usage", ChartFormats.YAxis.Percent, "#0000ff");
            DownloadChartOptions = GetCreateChartOptions("Download Speed", ChartFormats.YAxis.BytesPerSecond, "#ffcc00");
            UploadChartOptions = GetCreateChartOptions("Upload Speed", ChartFormats.YAxis.BytesPerSecond, "#ffcc00");
            BatteryChartOptions = GetCreateChartOptions("Battery Level", ChartFormats.YAxis.Percent, "#ff6600");
            StorageChartOptions = GetCreateChartOptions("Storage Usage", ChartFormats.YAxis.Percent, "#9933ff");
            PsuChartOptions = GetCreateChartOptions("PSU Usage", ChartFormats.YAxis.Percent, "#33ccff");
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                allCharts = [cpuUsageChart, memoryUsageChart, gpuUsageChart, downloadChart, uploadChart, batteryUsageChart, storageUsageChart, psuUsageChart];
            }
        }

        private void OnDataUpdated()
        {
            InvokeAsync(async () =>
            {
                await Task.WhenAll(allCharts.Select(c => c.UpdateChartAsync()));
                StateHasChanged();
            });
        }

        private void PauseMonitoring() => MetricsService.Stop();
        private void StartMonitoring() => MetricsService.Start();

        private void OnUpdateIntervalChanged(int newInterval)
        {
            updateInterval = newInterval;
            MetricsService.SetInterval(newInterval);
        }

        private void ResetCharts() => MetricsService.Reset();

        public void Dispose()
        {
            MetricsService.DataUpdated -= OnDataUpdated;
        }
    }
}
