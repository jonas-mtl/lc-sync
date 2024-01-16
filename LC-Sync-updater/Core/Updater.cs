using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LC_Sync_updater.Core
{
    internal class Updater
    {

        public static async Task DownloadNewestVersion()
        {
            string url = "https://github.com/jonas-mtl/LCSync/releases/latest/download/LCSync.zip";
            string zipFilePath = "LCSync.zip";
            string extractPath = "."; 

            await DownloadAndExtractAsync(url, zipFilePath, extractPath);
        }

        static async Task DownloadAndExtractAsync(string url, string zipFilePath, string extractPath)
        {
            using (HttpClient client = new HttpClient())
            {
                // Download the file
                using (HttpResponseMessage response = await client.GetAsync(url))
                using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
                {
                    using (FileStream streamToWriteTo = File.Create(zipFilePath))
                    {
                        await streamToReadFrom.CopyToAsync(streamToWriteTo);
                    }
                }
            }

            if (File.Exists(extractPath)) File.Delete(extractPath);
            
            ZipFile.ExtractToDirectory(zipFilePath, extractPath);
            File.Delete(zipFilePath);
        }

        public static void CopyDirectory(string sourceDir, string destDir)
        {
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            foreach (string filePath in Directory.GetFiles(sourceDir))
            {
                string fileName = Path.GetFileName(filePath);
                string destFilePath = Path.Combine(destDir, fileName);
                File.Copy(filePath, destFilePath, true);
            }

            foreach (string subDirPath in Directory.GetDirectories(sourceDir))
            {
                string subDirName = Path.GetFileName(subDirPath);
                string destSubDirPath = Path.Combine(destDir, subDirName);
                CopyDirectory(subDirPath, destSubDirPath);
            }
        }
    }
}
