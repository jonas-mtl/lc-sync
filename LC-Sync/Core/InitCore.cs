using HtmlAgilityPack;
using LC_Sync.Core.LCSync;
using LC_Sync.Core.Util;
using LC_Sync.MVVM.View;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml;


namespace LC_Sync.Core
{
    internal class InitCore
    {
        public static string currentlyLoadedModlist = "";
        public static Version currentlyLoadedVersion = null;
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
            await checkForUpdates();
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

        public static async Task checkForUpdates()
        {
            string ghReleaseInfo = await GetLatestReleaseInfo();
            ghReleaseInfo = ghReleaseInfo.Replace("Release ", "");

            string currentVersion = currentlyLoadedVersion.ToString().Remove(currentlyLoadedVersion.ToString().Length - 2);

            Console.WriteLine("Newest version: " + ghReleaseInfo + ", current version: " + currentlyLoadedVersion);

            char[] splitter = { '.' };
            string[] ghVersionSplit = ghReleaseInfo.Split(splitter);
            string[] currentVersionSplit = currentVersion.Split(splitter);

            int[] ghVersionSplitInt = ghVersionSplit.Select(int.Parse).ToArray();
            int[] currentVersionSplitInt = currentVersionSplit.Select(int.Parse).ToArray();

            bool newVersionAvailable = false;
            for (int i = 0; i < ghVersionSplitInt.Length; i++)
            {
                if (ghVersionSplitInt[i] > currentVersionSplitInt[i])
                {
                    newVersionAvailable = true;
                    break;
                }
            }

            if (newVersionAvailable)
            {
                CustomSelectBox prompt = new CustomSelectBox();
                prompt.Title = string.Empty;
                prompt.Text = "A new version is available, do you want to update?";
                prompt.ShowDialog();

                while (!prompt.closed)
                {
                    continue;
                }

                if (prompt.returnValue)
                {
                    string currentPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                    Process.Start(currentPath + @"\LC-Sync-updater.exe");
                    Environment.Exit(0);
                }
            }
        }

        static async Task<string> GetLatestReleaseInfo()
        {
            string url = "https://github.com/jonas-mtl/LCSync/releases/latest";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string html = await client.GetStringAsync(url);

                    HtmlDocument doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    HtmlNode releaseHeader = doc.DocumentNode.SelectSingleNode("//h1[contains(text(), 'Release')]");

                    string releaseText = releaseHeader?.InnerText.Trim();

                    return releaseText ?? "Release information not found";
                }
                catch (Exception ex)
                {
                    return $"Error: {ex.Message}";
                }
            }
        }
    }
}
