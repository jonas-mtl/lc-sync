using LC_Sync.Core.LCSync;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace LC_Sync.Core.Util
{
    public class FileHandler
    {
        public static void removeMods()
        {
            if (Directory.Exists(SteamHandler.LCInstallationPath + "\\BepInEx")) Directory.Delete(SteamHandler.LCInstallationPath + "\\BepInEx", true);
            else
            {
                Log.Info("BepInEx directory not found!");
            }
            File.Delete(SteamHandler.LCInstallationPath + "\\winhttp.dll");
            File.Delete(SteamHandler.LCInstallationPath + "\\doorstop_config.ini");
            File.Delete(SteamHandler.LCInstallationPath + "\\lcsync.json");
        }

        public static async Task InstallModAsync(TSMod tsMod)
        {
            string modPath = await FileDownloader.DownloadModAsync(tsMod);
            if (string.IsNullOrEmpty(modPath))
            {
                Log.Errored("Mod " + tsMod.ModName + " could not be downloaded.");
                return;
            }

            if (HandleBepInExPackInstallation(modPath)) return; 

            string bepInExPath = FindBepInExFolder(modPath);

            if (bepInExPath != null)
            {
                // If "BepInEx" folder exists, copy its contents to LCPluginsPath
                CopyDirectoryContents(bepInExPath, SteamHandler.LCInstallationPath + "\\BepInEx");
            }
            else
            {
                // If "BepInEx" folder doesn't exist, copy all .dll files in modPath to LCPluginsPath
                string[] dllFiles = Directory.GetFiles(modPath, "*.dll");
                foreach (string dllFile in dllFiles)
                {
                    string destinationPath = Path.Combine(SteamHandler.LCPluginsPath, Path.GetFileName(dllFile));
                    File.Copy(dllFile, destinationPath, true); // overwrite existing files
                }
            }

            Directory.Delete(SteamHandler.LCSyncTmpPath, true);
        }

        private static string FindBepInExFolder(string directory)
        {
            // Search for "BepInEx" folder recursively in the specified directory
            string bepInExPath = Directory.GetDirectories(directory, "BepInEx", SearchOption.AllDirectories).FirstOrDefault();

            return bepInExPath;
        }

        private static bool HandleBepInExPackInstallation(string directory)
        {
            if (!string.IsNullOrEmpty(Directory.GetDirectories(directory, "BepInExPack", SearchOption.AllDirectories).FirstOrDefault()))
            {
                CopyDirectoryContents(directory + "\\BepInExPack", SteamHandler.LCInstallationPath);
                return true;
            }

            return false;
        }

        private static void CopyDirectoryContents(string sourcePath, string destinationPath)
        {
            // Create the destination directory if it doesn't exist
            Directory.CreateDirectory(destinationPath);

            // Copy all files
            foreach (string filePath in Directory.GetFiles(sourcePath))
            {
                string destinationFilePath = Path.Combine(destinationPath, Path.GetFileName(filePath));
                File.Copy(filePath, destinationFilePath, true); // true to overwrite existing files
            }

            // Copy all subdirectories recursively
            foreach (string subdirectoryPath in Directory.GetDirectories(sourcePath))
            {
                string subdirectoryName = Path.GetFileName(subdirectoryPath);
                string destinationSubdirectoryPath = Path.Combine(destinationPath, subdirectoryName);
                CopyDirectoryContents(subdirectoryPath, destinationSubdirectoryPath);
            }
        }

        public static void getPreviousKey()
        {
            if (File.Exists(SteamHandler.LCInstallationPath + "\\.lcsync"))
            {
                string key = File.ReadAllText(SteamHandler.LCInstallationPath + "\\.lcsync");
                LCSyncData.StoredSrcBinKey = key;
            }
        }

        public static void setPreviousKey(string key)
        {
            File.WriteAllText(SteamHandler.LCInstallationPath + "\\.lcsync", key);
            LCSyncData.StoredSrcBinKey = key;
            Console.WriteLine($"Stored key ({key}) locally.\n");
        }
    }
}