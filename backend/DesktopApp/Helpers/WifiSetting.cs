using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Timers;
using System.Threading.Tasks;

namespace DesktopApp.Helpers
{
    public static class WifiSetting
    {
        private static readonly System.Timers.Timer _timer;
        public static event Action<string> WiFiStatusChanged; // Current network name
        public static event Action<List<string>> AvailableNetworksChanged; // List of SSIDs

        static WifiSetting()
        {
            //_timer = new System.Timers.Timer(10000); // Update every 10 seconds
            //_timer.Elapsed += async (s, e) => await UpdateWiFiStatusAsync();
            //_timer.AutoReset = true;
            //_timer.Start();
            //Task.Run(UpdateWiFiStatusAsync); // Initial update
        }

        public static async Task InitializeAsync()
        {
            await UpdateWiFiStatusAsync();
        }

        //public static async Task UpdateWiFiStatusAsync()
        //{
        //    try
        //    {
        //        string status = GetCurrentWiFiStatus();
        //        WiFiStatusChanged?.Invoke(status);

        //        List<string> networks = await GetAvailableNetworksAsync();
        //        AvailableNetworksChanged?.Invoke(networks);
        //    }
        //    catch
        //    {
        //        WiFiStatusChanged?.Invoke("Wi-Fi Error");
        //        AvailableNetworksChanged?.Invoke(new List<string>());
        //    }
        //}

        public static async Task UpdateWiFiStatusAsync()
        {
            string status = await GetCurrentWiFiStatusAsync();
            WiFiStatusChanged?.Invoke(status);
            List<string> networks = await GetAvailableNetworksAsync();
            AvailableNetworksChanged?.Invoke(networks);
        }

        private static async Task<string> GetCurrentWiFiStatusAsync()
        {
            return await GetConnectedSSIDAsync();
        }


        private static string GetCurrentWiFiStatus()
        {
            try
            {
                var network = NetworkInterface.GetAllNetworkInterfaces()
                    .FirstOrDefault(n => n.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 && n.OperationalStatus == OperationalStatus.Up);
                return network != null ? network.Name : "No Wi-Fi";
            }
            catch
            {
                return "Wi-Fi Error";
            }
        }

        private static async Task<List<string>> GetAvailableNetworksAsync()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "netsh",
                        Arguments = "wlan show networks",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                // Parse SSIDs from netsh output
                var networks = new List<string>();
                var matches = Regex.Matches(output, @"SSID \d+ : (.+)");
                foreach (Match match in matches)
                {
                    if (match.Groups.Count > 1)
                    {
                        string ssid = match.Groups[1].Value.Trim();
                        if (!string.IsNullOrEmpty(ssid))
                        {
                            networks.Add(ssid);
                        }
                    }
                }

                return networks.Distinct().ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        public static async Task ConnectToNetworkAsync(string ssid, string password = null)
        {
            try
            {
                // Check if profile exists, if not, create one for secured networks
                if (!string.IsNullOrEmpty(password))
                {
                    var profileXml = $@"<?xml version=""1.0""?>
                        <WLANProfile xmlns=""http://www.microsoft.com/networking/WLAN/profile/v1"">
                            <name>{ssid}</name>
                            <SSIDConfig>
                                <SSID>
                                    <name>{ssid}</name>
                                </SSID>
                            </SSIDConfig>
                            <connectionType>ESS</connectionType>
                            <connectionMode>auto</connectionMode>
                            <MSM>
                                <security>
                                    <authEncryption>
                                        <authentication>WPA2PSK</authentication>
                                        <encryption>AES</encryption>
                                        <useOneX>false</useOneX>
                                    </authEncryption>
                                    <sharedKey>
                                        <keyType>passPhrase</keyType>
                                        <protected>false</protected>
                                        <keyMaterial>{password}</keyMaterial>
                                    </sharedKey>
                                </security>
                            </MSM>
                        </WLANProfile>";

                    // Save profile to temp file
                    string tempFile = System.IO.Path.GetTempFileName();
                    await System.IO.File.WriteAllTextAsync(tempFile, profileXml);

                    // Add profile using netsh
                    var addProfileProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "netsh",
                            Arguments = $"wlan add profile filename=\"{tempFile}\"",
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };
                    addProfileProcess.Start();
                    await addProfileProcess.WaitForExitAsync();
                    System.IO.File.Delete(tempFile);
                }

                // Connect to the network
                var connectProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "netsh",
                        Arguments = $"wlan connect name=\"{ssid}\"",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                connectProcess.Start();
                await connectProcess.WaitForExitAsync();

                await UpdateWiFiStatusAsync();
            }
            catch
            {
                // Log error if needed
            }
        }

        public static async Task<(string Status, List<string> Networks)> GetWiFiStatusAsync()
        {
            string status = GetCurrentWiFiStatus();
            List<string> networks = await GetAvailableNetworksAsync();
            return (status, networks);
        }

        public static async void GetListWiFiAsync()
        {
            List<string> networks = await GetAvailableNetworksAsync();
            AvailableNetworksChanged?.Invoke(networks);
        }

        public static async Task<string> GetConnectedSSIDAsync()
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "netsh",
                        Arguments = "wlan show interfaces",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                var match = Regex.Match(output, @"^\s*SSID\s*:\s(.+)$", RegexOptions.Multiline);
                if (match.Success)
                {
                    string ssid = match.Groups[1].Value.Trim();
                    return string.IsNullOrEmpty(ssid) ? "No Wi-Fi" : ssid;
                }

                return "No Wi-Fi";
            }
            catch
            {
                return "Wi-Fi Error";
            }
        }

    }
}