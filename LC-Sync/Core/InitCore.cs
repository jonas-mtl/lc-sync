using LC_Sync.Core.Util;
using System;
using System.IO;
using System.Windows.Media;


namespace LC_Sync.Core
{
    internal class InitCore
    { 
        public static void SetupCore()
        {
            SteamHandler.FindSteamPath();

            string steamInstallationPath = SteamHandler.SteamInstallationPath;


            SolidColorBrush messageBoxBackground = new SolidColorBrush(Color.FromRgb(89, 3, 3));
            if (string.IsNullOrEmpty(steamInstallationPath))
            {
                Log.ShowCustomMessageBox("ERROR", "Steam path not found!", messageBoxBackground);
                Environment.Exit(1);
            }

            if (!Directory.Exists(SteamHandler.LCInstallationPath))
            {
                Log.ShowCustomMessageBox("ERROR", "Lethal Company not found!", messageBoxBackground);
                Environment.Exit(1);
            }

            if (SteamHandler.IsProcessRunning("Lethal Company"))
            {
                Log.ShowCustomMessageBox("ERROR", "Close Lethal Company first!", messageBoxBackground);
                Environment.Exit(1);

            }
        }
    }
}
