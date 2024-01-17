using LC_Sync.Core.LCSync;
using System;
using System.Collections.Generic;
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
        public static async Task deletePluginsFolder()
        {
            if (Directory.Exists(SteamHandler.LCPluginsPath)) Directory.Delete(SteamHandler.LCPluginsPath, true);
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
                List<string> dllFiles = SearchForDllFiles(modPath);

                if (!Directory.Exists(SteamHandler.LCPluginsPath)) Directory.CreateDirectory(SteamHandler.LCPluginsPath);

                foreach (string dllFile in dllFiles)
                {
                    string destinationPath = Path.Combine(SteamHandler.LCPluginsPath, Path.GetFileName(dllFile));
                    File.Copy(dllFile, destinationPath, true); // overwrite existing files
                }
            }

            Directory.Delete(SteamHandler.LCSyncTmpPath, true);
        }
        static List<string> SearchForDllFiles(string directory)
        {
            List<string> dllFiles = new List<string>();

            try
            {
                // Get all files in the current directory with the ".dll" extension
                string[] currentDirectoryDllFiles = Directory.GetFiles(directory, "*.dll");
                dllFiles.AddRange(currentDirectoryDllFiles);

                // Search for ".dll" files in subdirectories recursively
                string[] subdirectories = Directory.GetDirectories(directory);
                foreach (var subdirectory in subdirectories)
                {
                    List<string> subdirectoryDllFiles = SearchForDllFiles(subdirectory);
                    dllFiles.AddRange(subdirectoryDllFiles);
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Unauthorized access to directory: {directory}");
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine($"Directory not found: {directory}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            return dllFiles;
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