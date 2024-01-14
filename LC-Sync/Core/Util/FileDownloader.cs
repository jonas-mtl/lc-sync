using System;
using System.IO;
using System.Net.Http;
using System.IO.Compression;
using System.Threading.Tasks;
using LC_Sync.Core.LCSync;
using LC_Sync.Core.Util;

public class FileDownloader
{
    public static async Task<string> DownloadModAsync(TSMod tSMod)
    {
        Log.Info("Downloading " + tSMod.ModName + "/" + tSMod.ModNamespace + "...");
        string path = await DownloadAndUnzipAsync(tSMod.ModDownloadUrl, SteamHandler.LCSyncTmpPath);
        return path;
    }

    public static async Task<string> DownloadAndUnzipAsync(string url, string destinationFolder)
    {
        try
        {
            string tempFilePath = Path.Combine(destinationFolder + "\\", Guid.NewGuid().ToString("N") + ".zip");

            if (!Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
            }

            // Download file
            using (HttpClient client = new HttpClient())
            {
                byte[] fileData = await client.GetByteArrayAsync(url);
                File.WriteAllBytes(tempFilePath, fileData);
            }

            if (IsZipFile(tempFilePath))
            {
                // Unzip the file to the destination folder
                string unzippedFolderPath = Path.Combine(destinationFolder, Guid.NewGuid().ToString("N"));
                ZipFile.ExtractToDirectory(tempFilePath, unzippedFolderPath);

                File.Delete(tempFilePath);
                return unzippedFolderPath;
            }
            else
            {
                return null;
            }
        }
        catch (Exception ex)
        {
            Log.Errored($"Error: {ex.Message}");
            return null;
        }
    }

    private static bool IsZipFile(string filePath)
    {
        try
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (BinaryReader br = new BinaryReader(fs))
            {
                byte[] header = br.ReadBytes(4);
                return (header.Length >= 4 &&
                        header[0] == 0x50 && header[1] == 0x4B &&
                        header[2] == 0x03 && header[3] == 0x04);
            }
        }
        catch
        {
            return false;
        }
    }

    public static async Task DownloadFileAsync(string url, string filePath)
    {
        using (HttpClient client = new HttpClient())
        using (HttpResponseMessage response = await client.GetAsync(url))
        using (Stream contentStream = await response.Content.ReadAsStreamAsync())
        using (Stream fileStream = File.Create(filePath))
        {
            await contentStream.CopyToAsync(fileStream);
        }
    }
}