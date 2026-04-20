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

            try
            {
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
            }
            catch { /* return 0 if hardware access fails */ }

            return batteryLevel;
        }

        public double GetCpuUsage()
        {
            double cpuLoad = 0;

            try
            {
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
            }
            catch { /* return 0 if hardware access fails */ }

            return cpuLoad;
        }

        public double GetGpuUsage()
        {
            double gpuLoad = 0;

            try
            {
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
            }
            catch { /* return 0 if hardware access fails */ }

            return gpuLoad;
        }

        public double GetMemoryUsage()
        {
            double availableMemory = 0;
            double totalMemory = 0;
            double usedMemory = 0;

            try
            {
                foreach (IHardware hardware in _computer.Hardware.Where(x => x.HardwareType == HardwareType.Memory))
                {
                    hardware.Update();

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
            }
            catch { /* return 0 if hardware access fails */ }

            totalMemory = availableMemory + usedMemory;

            if (totalMemory == 0) return 0;

            return (usedMemory / totalMemory) * 100.0;
        }

        public double GetDownloadSpeed()
        {
            double downloadSpeed = 0;

            try
            {
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
            }
            catch { /* return 0 if hardware access fails */ }

            return downloadSpeed;
        }

        public double GetUploadSpeed()
        {
            double uploadSpeed = 0;

            try
            {
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
            }
            catch { /* return 0 if hardware access fails */ }

            return uploadSpeed;
        }

        public double GetStorageUsage()
        {
            double totalUsage = 0;

            try
            {
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
            }
            catch { /* return 0 if hardware access fails */ }

            return totalUsage;
        }

        public double GetPsuUsage()
        {
            double psuPower = 0;

            try
            {
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
            }
            catch { /* return 0 if hardware access fails */ }

            return psuPower;
        }

    }
}
