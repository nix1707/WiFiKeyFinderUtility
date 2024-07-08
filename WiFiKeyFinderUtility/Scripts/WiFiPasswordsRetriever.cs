using System.Diagnostics;
using System.Text.RegularExpressions;
using WiFiKeyFinderUtility.Models;

namespace WiFiKeyFinderUtility.Scripts;

public static class WiFiPasswordsRetriever
{
    private static string RunNetsh(string args)
    {
        Process processWifi = new();
        processWifi.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        processWifi.StartInfo.FileName = "netsh";
        processWifi.StartInfo.Arguments = args;

        processWifi.StartInfo.UseShellExecute = false;
        processWifi.StartInfo.RedirectStandardError = true;
        processWifi.StartInfo.RedirectStandardInput = true;
        processWifi.StartInfo.RedirectStandardOutput = true;
        processWifi.StartInfo.CreateNoWindow = true;
        processWifi.Start();
        string output = processWifi.StandardOutput.ReadToEnd();
        string _ = processWifi.StandardError.ReadToEnd();

        processWifi.WaitForExit();
        return output;
    }
    private static string GetWifiNetworks() =>
        RunNetsh("wlan show profiles");

    private static string ReadPassword(string wifiName) =>
        RunNetsh($"wlan show profile name=\"{wifiName}\" key=clear");

    private static string GetWifiPassword(string wifiName)
    {
        var password = ReadPassword(wifiName);

        using var reader = new StringReader(password);
        string line;

        while ((line = reader.ReadLine()) != null)
        {
            Regex regex = new(@"Key Content * : (?<after>.*)");
            Match match = regex.Match(line);

            if (match.Success)
            {
                var currentPassword = match.Groups["after"].Value;
                return currentPassword;
            }   
        }
        return "unknown characters";
    }

    public static List<WiFiInfo> GetWiFiInfos()
    {
        List<WiFiInfo> wifiInfos = [];
        string wifiNetworks = GetWifiNetworks();

        using var reader = new StringReader(wifiNetworks); 

        string line;

        while ((line = reader.ReadLine()) != null)
        {
            Regex regex = new(@"All User Profile * : (?<after>.*)");
            Match match = regex.Match(line);

            if (match.Success)
            {
                string wifiName = match.Groups["after"].Value;
                string currentPassword = GetWifiPassword(wifiName);

                wifiInfos.Add(new WiFiInfo { SSID = wifiName, Password = currentPassword });
            }

        }
        return wifiInfos;
    }
}
