using System;
using System.Management;
using LibreHardwareMonitor.Hardware;

namespace SystemMonitor.Services
{
    public partial class WindowsSystemPerformanceService : ISystemPerformanceService, IDisposable
    {
        private readonly Computer _computer;

        public WindowsSystemPerformanceService()
        {
            _computer = new Computer
            {
                IsCpuEnabled = true,
                IsMemoryEnabled = true,
                IsGpuEnabled = true,
                IsNetworkEnabled = true,
                IsStorageEnabled = true,
                IsPsuEnabled = true,
                IsBatteryEnabled = true
            };

            _computer.Open();
        }

        public void Dispose()
        {
            _computer.Close();
        }

        public double GetBatteryLevel()
        {
            double batteryLevel = 0;

            foreach (IHardware hardware in _computer.Hardware.Where(x => x.HardwareType == HardwareType.Battery))
            {
                hardware.Update();

                foreach (ISensor sensor in hardware.Sensors)
                {
                    if (sensor.SensorType == SensorType.Level && sensor.Name.Contains("Charge"))
                    {
                        batteryLevel = sensor.Value.GetValueOrDefault();
                    }
                }
            }

            return batteryLevel;
        }

        public double GetCpuUsage()
        {
            double cpuLoad = 0;
            foreach (IHardware hardware in _computer.Hardware.Where(x => x.HardwareType == HardwareType.Cpu))
            {
                hardware.Update();

                foreach (ISensor sensor in hardware.Sensors)
                {
                    if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("CPU Total"))
                    {
                        cpuLoad = sensor.Value.GetValueOrDefault();
                    }
                }
            }

            return cpuLoad;
        }

        public double GetGpuUsage()
        {
            double gpuLoad = 0;
            foreach (IHardware hardware in _computer.Hardware.Where(x => x.HardwareType == HardwareType.GpuNvidia || x.HardwareType == HardwareType.GpuAmd || x.HardwareType == HardwareType.GpuIntel))
            {
                hardware.Update();

                foreach (ISensor sensor in hardware.Sensors)
                {
                    if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("GPU Core"))
                    {
                        gpuLoad = sensor.Value.GetValueOrDefault();
                    }
                }
            }

            return gpuLoad;
        }

        public double GetMemoryUsage()
        {
            double availableMemory = 0;
            double totalMemory = 0;
            double usedMemory = 0;

            foreach (IHardware hardware in _computer.Hardware.Where(x => x.HardwareType == HardwareType.Memory))
            {
                hardware.Update(); // Refresh memory sensor data

                foreach (ISensor sensor in hardware.Sensors)
                {
                    if (sensor.SensorType == SensorType.Data && sensor.Name.Equals("Memory Used", StringComparison.OrdinalIgnoreCase))
                    {
                        usedMemory = sensor.Value.GetValueOrDefault();
                    }

                    if (sensor.SensorType == SensorType.Data && sensor.Name.Equals("Memory Available", StringComparison.OrdinalIgnoreCase))
                    {
                        availableMemory = sensor.Value.GetValueOrDefault();
                    }
                }
            }

            totalMemory = availableMemory + usedMemory;

            if (totalMemory == 0) return 0; // Avoid division by zero

            return (usedMemory / totalMemory) * 100.0; // Physical memory usage percentage
        }

        public double GetDownloadSpeed()
        {
            double downloadSpeed = 0;

            foreach (IHardware hardware in _computer.Hardware.Where(x => x.HardwareType == HardwareType.Network))
            {
                hardware.Update();

                foreach (ISensor sensor in hardware.Sensors)
                {
                    if (sensor.SensorType == SensorType.Throughput)
                    {
                        if (sensor.Name.Contains("Download Speed", StringComparison.OrdinalIgnoreCase))
                        {
                            downloadSpeed += sensor.Value.GetValueOrDefault();
                        }                        
                    }
                }
            }

            return downloadSpeed;
        }

        public double GetUploadSpeed()
        {
            double uploadSpeed = 0;

            foreach (IHardware hardware in _computer.Hardware.Where(x => x.HardwareType == HardwareType.Network))
            {
                hardware.Update();

                foreach (ISensor sensor in hardware.Sensors)
                {
                    if (sensor.SensorType == SensorType.Throughput)
                    {
                        if (sensor.Name.Contains("Upload Speed", StringComparison.OrdinalIgnoreCase))
                        {
                            uploadSpeed += sensor.Value.GetValueOrDefault();
                        }
                    }
                }
            }

            return uploadSpeed;
        }

        public double GetStorageUsage()
        {
            double totalUsage = 0;
            foreach (IHardware hardware in _computer.Hardware.Where(x => x.HardwareType == HardwareType.Storage))
            {
                hardware.Update();

                foreach (ISensor sensor in hardware.Sensors)
                {
                    if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("Used Space"))
                    {
                        totalUsage = sensor.Value.GetValueOrDefault();
                    }
                }
            }

            return totalUsage;
        }

        public double GetPsuUsage()
        {
            double psuPower = 0;

            foreach (IHardware hardware in _computer.Hardware.Where(x => x.HardwareType == HardwareType.Psu))
            {
                hardware.Update();

                foreach (ISensor sensor in hardware.Sensors)
                {
                    if (sensor.SensorType == SensorType.Power && sensor.Name.Contains("Power"))
                    {
                        psuPower = sensor.Value.GetValueOrDefault();
                    }
                }
            }

            return psuPower;
        }

    }
}
