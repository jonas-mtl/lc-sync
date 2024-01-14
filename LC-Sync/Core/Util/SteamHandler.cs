using Microsoft.Win32;
using System;
using System.Diagnostics;


public class SteamHandler
{
    private static string steamInstallationPath;

    public static bool IsProcessRunning(string processName)
    {
        Process[] processes = Process.GetProcessesByName(processName);

        return processes.Length > 0;
    }

    public static void FindSteamPath()
    {
        // Check the registry 
        string wow6432NodePath = GetRegistryValue("SOFTWARE\\WOW6432Node\\Valve\\Steam", "InstallPath");

        if (!string.IsNullOrEmpty(wow6432NodePath))
        {
            steamInstallationPath = wow6432NodePath;
            return;
        }

        string nonWow6432NodePath = GetRegistryValue("SOFTWARE\\Valve\\Steam", "InstallPath");

        if (!string.IsNullOrEmpty(nonWow6432NodePath))
        {
            steamInstallationPath = nonWow6432NodePath;
            return;
        }

        steamInstallationPath = null;
    }

    private static string GetRegistryValue(string keyPath, string valueName)
    {
        try
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath))
            {
                if (key != null)
                {
                    object value = key.GetValue(valueName);

                    if (value != null)
                    {
                        return value.ToString();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error accessing registry: {ex.Message}");
        }

        return null;
    }
    public static string SteamInstallationPath
    {
        get { return steamInstallationPath; }
    }

    public static string LCInstallationPath
    {
        get { return steamInstallationPath + "\\steamapps\\common\\Lethal Company"; }
    }

    public static string LCSyncTmpPath
    {
        get { return steamInstallationPath + "\\steamapps\\common\\Lethal Company\\LCSync"; }
    }
    public static string LCPluginsPath
    {
        get { return steamInstallationPath + "\\steamapps\\common\\Lethal Company\\BepInEx\\plugins"; }
    }
}