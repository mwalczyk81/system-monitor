using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using LibreHardwareMonitor.Hardware;

namespace SystemMonitor.Services
{

    public class WindowsSystemPerformanceService : ISystemPerformanceService, IDisposable
    {
        private readonly Computer _computer = new() { IsGpuEnabled = true };

        public WindowsSystemPerformanceService()
        {
            _computer.Open();
        }

        public void Dispose()
        {
            _computer.Close();
        }

        public double GetBatteryLevel()
        {
            throw new NotImplementedException();
        }

        public double GetCpuUsage()
        {
            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            double cpuUsage = 0.0;
            foreach (var obj in searcher.Get())
            {
                cpuUsage += Convert.ToDouble(obj["LoadPercentage"]);
            }
            return cpuUsage / Environment.ProcessorCount;  // Average CPU usage
        }

        public double GetGpuUsage()
        {
            double gpuLoad = 0;

            foreach (IHardware hardware in _computer.Hardware)
            {
                // Check if the hardware is a GPU (NVIDIA or AMD)
                if (hardware.HardwareType == HardwareType.GpuNvidia || hardware.HardwareType == HardwareType.GpuAmd)
                {
                    hardware.Update(); // Update hardware data to get fresh values

                    // Iterate through the sensors to find the GPU load sensor
                    foreach (ISensor sensor in hardware.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("GPU Core"))
                        {
                            gpuLoad = sensor.Value.GetValueOrDefault(); // Get GPU load as a percentage
                        }
                    }
                }
            }

            return gpuLoad;
        }

        public double GetMemoryUsage()
        {
            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
            foreach (var obj in searcher.Get())
            {
                var freeMemory = Convert.ToDouble(obj["FreePhysicalMemory"]);
                var totalMemory = Convert.ToDouble(obj["TotalVisibleMemorySize"]);
                return 100.0 - ((freeMemory / totalMemory) * 100.0);  // Used memory percentage
            }
            return 0.0;
        }
    }

}
