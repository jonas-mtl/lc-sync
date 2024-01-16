using LC_Sync.Core.LCSync;
using LC_Sync.Core.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;


namespace LC_Sync.Core
{
    internal class InitCore
    {
        public static string currentlyLoadedModlist = "";
        public static List<ModInfo> currentlyLoadedMods = new List<ModInfo>();

        public static bool updatingPackageIndex = true;
        public static async Task SetupCoreAsync()
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

            FileHandler.getPreviousKey();

            await Task.Run(() =>
            {
                Task task = LCSyncData.InitPackageIndex();
                updatingPackageIndex = false;
            });
            Log.Info($"DONE! \n");

            await getLoadedModsList();
        }

        public static async Task getLoadedModsList()
        {
            // Upadte modlist in CreateModView
            currentlyLoadedModlist = "";
            if (!string.IsNullOrEmpty(LCSyncData.StoredSrcBinKey))
            {
                currentlyLoadedMods = await LCSyncData.getSrcbinModsAsModInfo();
                if (currentlyLoadedMods != null)
                {
                    foreach (ModInfo mod in currentlyLoadedMods)
                    {
                        currentlyLoadedModlist += $"- {mod.ModName} by {mod.ModNamespace}\n";
                    }
                }
            }
        }
    }
}
