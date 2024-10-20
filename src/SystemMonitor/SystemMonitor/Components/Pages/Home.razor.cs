using ApexCharts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemMonitor.Components.Charts;
using SystemMonitor.Models;

namespace SystemMonitor.Components.Pages
{
    public partial class Home
    {
        private List<ChartDataPoint> cpuData = new List<ChartDataPoint>();
        private List<ChartDataPoint> memoryData = new List<ChartDataPoint>();
        private List<ChartDataPoint> gpuData = new List<ChartDataPoint>();
        private UsageChart cpuUsageChart = new();
        private UsageChart memoryUsageChart = new();
        private UsageChart gpuUsageChart = new();
        private System.Timers.Timer? timer = new();
        private bool isMonitoring = true;
        private ApexChartOptions<ChartDataPoint> CpuChartOptions { get; set; } = new();
        private ApexChartOptions<ChartDataPoint> GpuChartOptions { get; set; } = new();
        private ApexChartOptions<ChartDataPoint> MemoryChartOptions { get; set; } = new();
        private int updateInterval = 1;

        protected override void OnInitialized()
        {
            // Set up the timer to update every second
            timer = new System.Timers.Timer(TimeSpan.FromSeconds(updateInterval));
            timer.Elapsed += async (sender, e) => await UpdatePerformanceData();
            timer.AutoReset = true;
            timer.Enabled = true;
            CpuChartOptions = CreateChartOptions();
            MemoryChartOptions = CreateChartOptions();
            GpuChartOptions = CreateChartOptions();
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

        private ApexChartOptions<ChartDataPoint> CreateChartOptions()
        {
            return new ApexChartOptions<ChartDataPoint>
            {
                Chart = new Chart
                {
                    Background = "#f4f4f4", // Light gray background
                    Toolbar = new ApexCharts.Toolbar
                    {
                        Show = true
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
                Stroke = new Stroke
                {
                    Curve = Curve.Smooth,
                    Width = 3
                },
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
                Yaxis = new List<YAxis>
                {
                    new YAxis
                    {
                        Labels = new YAxisLabels
                        {
                            Formatter = "function(val) { return val + '%'; }",
                            Style = new AxisLabelStyle
                            {
                                FontSize = "12px",
                                Colors = "#333"
                            }
                        }

                    }
                },
                Xaxis = new XAxis
                {
                    Labels = new XAxisLabels
                    {
                        Formatter = "function(value) { return new Date(value).toLocaleTimeString('en-US', { hour: '2-digit', minute:'2-digit', second:'2-digit' }); }",
                        Show = false,
                        Style = new AxisLabelStyle
                        {
                            FontSize = "12px",
                            Colors = "#333"
                        }
                    }
                },
                Tooltip = new Tooltip
                {
                    Enabled = true,
                    X = new TooltipX
                    {
                        Formatter = "function(value) { return new Date(value).toLocaleTimeString(); }"
                    }
                },
                Grid = new ApexCharts.Grid
                {
                    BorderColor = "#e0e0e0",
                    Row = new GridRow
                    {
                        Colors = new List<string> { "#f0f0f0", "#ffffff" },
                        Opacity = 0.5
                    }
                }
            };
        }

        private async Task UpdatePerformanceData()
        {

            if (!isMonitoring) return;

            double cpuUsage = PerformanceService.GetCpuUsage();
            double memoryUsage = PerformanceService.GetMemoryUsage();
            double gpuUsage = PerformanceService.GetGpuUsage();

            cpuData.Add(new ChartDataPoint { Time = DateTime.Now, Value = cpuUsage });
            memoryData.Add(new ChartDataPoint { Time = DateTime.Now, Value = memoryUsage });
            gpuData.Add(new ChartDataPoint { Time = DateTime.Now, Value = gpuUsage });

            // Limit the history to the last 100 data points
            if (cpuData.Count > 100) cpuData.RemoveAt(0);
            if (memoryData.Count > 100) memoryData.RemoveAt(0);
            if (gpuData.Count > 100) gpuData.RemoveAt(0);

            // Update the charts with the new data without redrawing
            await cpuUsageChart.UpdateChartAsync();
            await memoryUsageChart.UpdateChartAsync();
            await gpuUsageChart.UpdateChartAsync();
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
