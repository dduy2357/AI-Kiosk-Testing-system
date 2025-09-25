using System.Management;
using System.Net.NetworkInformation;
using Microsoft.Win32;
using System.Diagnostics;
using DesktopApp.Services;
using System.Windows.Forms;

namespace DesktopApp.Helpers
{
    public static class VirtualMachineDetector
    {
        private static MonitoringService _monitoringService;
        private static readonly string[] KnownVmVendors = {
            "VMware", "VirtualBox", "KVM", "Microsoft Corporation", "Xen", "QEMU", "Parallels"
        };

        public static void Initialize(MonitoringService monitoringService)
        {
            _monitoringService = monitoringService ?? throw new ArgumentNullException(nameof(monitoringService));
            _monitoringService.LogInfo("SecureEnvironmentValidator initialized.");
        }

        public static bool IsRunningInVirtualMachine()
        {
            if (CheckSystemInfo())
                return true;
            if (IsRunningInVmByDiskRegistry())
                return true;
            if (CheckVmProcesses())
                return true;
            if (CheckMacAddress())
                return true;

            _monitoringService?.LogInfo("No virtual machine detected.");
            return false;
        }

        private static bool CheckSystemInfo()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher("Select * from Win32_ComputerSystem");
                foreach (var item in searcher.Get())
                {
                    string manufacturer = item["Manufacturer"]?.ToString()?.ToLower() ?? "";
                    string model = item["Model"]?.ToString()?.ToLower() ?? "";
                    _monitoringService?.LogDebug($"Manufacturer: {manufacturer}, Model: {model}");

                    if (KnownVmVendors.Any(v => manufacturer.Contains(v.ToLower())) ||
                        KnownVmVendors.Any(v => model.Contains(v.ToLower())))
                    {
                        _monitoringService?.LogWarning("Detected virtual machine based on Manufacturer/Model.");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _monitoringService?.LogError($"Error while checking virtual machine via WMI: {ex.Message}");
            }
            _monitoringService?.LogInfo("Virtual machine check passed (WMI).");
            return false;
        }

        private static bool IsRunningInVmByDiskRegistry()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\Disk\Enum"))
                {
                    if (key != null)
                    {
                        string value = key.GetValue("0")?.ToString()?.ToLower() ?? "";
                        _monitoringService?.LogDebug($"Disk Enum Registry Value: {value}");

                        if (value.Contains("vmware") || value.Contains("vbox") || value.Contains("virtual"))
                        {
                            _monitoringService?.LogWarning("Detected virtual machine based on Disk Registry.");
                            return true;
                        }
                    }
                }
                // Kiểm tra registry VirtualBox nhưng chỉ trả về true nếu có tiến trình/dịch vụ liên quan
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Oracle\VirtualBox"))
                {
                    if (key != null)
                    {
                        // Kiểm tra thêm xem có tiến trình VirtualBox đang chạy không
                        string[] vbProcesses = new[] { "vboxservice.exe", "vboxtray.exe" };
                        var processes = Process.GetProcesses();
                        foreach (var process in processes)
                        {
                            if (vbProcesses.Any(p => p.Equals(process.ProcessName, StringComparison.OrdinalIgnoreCase)))
                            {
                                _monitoringService?.LogWarning("Detected running VirtualBox based on Registry and Process.");
                                return true;
                            }
                        }
                        _monitoringService?.LogInfo("VirtualBox registry found but no active VirtualBox processes.");
                    }
                }
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\VMware, Inc."))
                {
                    if (key != null)
                    {
                        string[] vmProcesses = new[] { "vmtoolsd.exe", "vmwaretray.exe", "vmwareuser.exe" };
                        var processes = Process.GetProcesses();
                        foreach (var process in processes)
                        {
                            if (vmProcesses.Any(p => p.Equals(process.ProcessName, StringComparison.OrdinalIgnoreCase)))
                            {
                                _monitoringService?.LogWarning("Detected running VMware based on Registry and Process.");
                                return true;
                            }
                        }
                        _monitoringService?.LogInfo("VMware registry found but no active VMware processes.");
                    }
                }
            }
            catch (Exception ex)
            {
                _monitoringService?.LogError($"Error while checking disk registry for VM detection: {ex.Message}");
            }
            _monitoringService?.LogInfo("Virtual machine check passed (Disk Registry).");
            return false;
        }

        private static bool CheckVmProcesses()
        {
            try
            {
                string[] vmProcesses = new[] { "vmtoolsd.exe", "vboxservice.exe", "vboxtray.exe", "vmwaretray.exe", "vmwareuser.exe" };
                var processes = Process.GetProcesses();
                foreach (var process in processes)
                {
                    if (vmProcesses.Any(p => p.Equals(process.ProcessName, StringComparison.OrdinalIgnoreCase)))
                    {
                        _monitoringService?.LogWarning($"Detected virtual machine process: {process.ProcessName}");
                        return true;
                    }
                }
                _monitoringService?.LogInfo("Virtual machine check passed (Processes).");
                return false;
            }
            catch (Exception ex)
            {
                _monitoringService?.LogError($"Error checking VM processes: {ex.Message}");
                return false;
            }
        }

        private static bool CheckMacAddress()
        {
            try
            {
                string[] vmMacPrefixes = new[] { "00:0C:29", "00:50:56", "00:1C:14", "08:00:27" };
                foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
                {
                    var mac = nic.GetPhysicalAddress().ToString();
                    if (!string.IsNullOrEmpty(mac) && vmMacPrefixes.Any(prefix => mac.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
                    {
                        _monitoringService?.LogWarning($"Detected virtual machine MAC: {mac}");
                        return true;
                    }
                }
                _monitoringService?.LogInfo("Virtual machine check passed (MAC Address).");
                return false;
            }
            catch (Exception ex)
            {
                _monitoringService?.LogError($"Error checking MAC Address: {ex.Message}");
                return false;
            }
        }


    }
}