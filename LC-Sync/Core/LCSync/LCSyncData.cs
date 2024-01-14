using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LC_Sync.Core.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace LC_Sync.Core.LCSync
{
    internal class LCSyncData
    {
        public static LCSyncRequests LCSyncRequests = new LCSyncRequests();
        public static List<TSMod> TSMods = new List<TSMod>();
        public static List<ModInfo> TSPackageIndex = new List<ModInfo>();
        public static string SrcbinKey { get; set; }
        public static string StoredSrcBinKey = null;

        public LCSyncData() { }

        public static void ClearMods()
        {
            TSMods = new List<TSMod>();
        }

        public static async Task<bool> UpdateSrcbinModsAsync()
        {
            JToken srcbinDataJSON = await LCSyncRequests.GetSrcbinDataAsync(SrcbinKey);

            if (srcbinDataJSON == null) return false;

            // Add standard required mods like LC_API
            List<TSMod> requiredMods = await RequiredMods.GetRequiredMods();
            TSMods = TSMods.Union(requiredMods).Distinct().ToList();

            // Check if "mods" array exists in the JSON
            if (srcbinDataJSON["mods"] is JArray modsArray)
            {
                foreach (JToken modToken in modsArray)
                {
                    // Deserialize the JSON object into TSMod properties
                    string modNamespace = modToken.Value<string>("modNamespace");
                    string modName = modToken.Value<string>("modName");

                    // Create a new instance of TSMod
                    TSMod newMod = new TSMod(modNamespace, modName);
                    await newMod.InitializeAsync();

                    TSMods.Add(newMod);
                }
            }
            else
            {
                Log.Errored("Invalid Sourcebin JSON");
                return false;
            }

            return true;
        }

        public static async Task<string> UploadSrcbinModsAsync()
        {
            string jsonPath = SteamHandler.LCInstallationPath + "\\lcsync.json";

            JObject jsonObject = new JObject();
            jsonObject["mods"] = new JArray();

            foreach (TSMod mod in TSMods)
            {
                JObject modObject = new JObject();
                modObject["modName"] = mod.ModName;
                modObject["modNamespace"] = mod.ModNamespace;
                ((JArray)jsonObject["mods"]).Add(modObject);
            }

            string jsonString = jsonObject.ToString();
            File.WriteAllText(jsonPath, jsonString);

            HttpResponseMessage response = await UploadFile("https://sourceb.in/api/bins", jsonPath);
            string responseContent = await response.Content.ReadAsStringAsync();
            JToken responseContentJson = JToken.Parse(responseContent);
            File.Delete(jsonPath);

            return responseContentJson["key"].ToString();
        }

        static async Task<HttpResponseMessage> UploadFile(string apiUrl, string filePath)
        {
            using (var httpClient = new HttpClient())
            using (var fileStream = File.OpenRead(filePath))
            using (var streamReader = new StreamReader(fileStream))
            {
                // Read the content of the file
                string fileContent = streamReader.ReadToEnd();

                // Build your JSON payload including the actual file content
                string jsonPayload = $"{{\"files\": [{{\"content\": {JsonConvert.ToString(fileContent)}, \"languageId\": 33}}]}}";

                using (HttpClient client = new HttpClient())
                {
                    StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                    if (response.IsSuccessStatusCode)
                    {
                        Log.Info("Post request successful!");
                        return response;
                    }
                    else
                    {
                        Log.Errored($"Error: {response.StatusCode}");
                    }
                }
            }

            return null;
        }

        public async static Task InitPackageIndex()
        {
            string jsonPath = SteamHandler.LCInstallationPath + "\\package-index.json";

            if (!File.Exists(jsonPath))
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                Log.Info("Downloading mod package index...");

                string url = "https://thunderstore.io/c/lethal-company/api/v1/package/";

                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        HttpResponseMessage response = await client.GetAsync(url);

                        if (response.IsSuccessStatusCode)
                        {
                            string jsonContent = await response.Content.ReadAsStringAsync();
                            parsePackageString(jsonContent);

                            File.WriteAllText(jsonPath, jsonContent);
                        }
                        else
                        {
                            Log.Errored($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Errored($"An error occurred: {ex.Message}");
                }

                TimeSpan elapsed = stopwatch.Elapsed;
                Log.Info($"DONE! ({Log.FormatElapsedTime(elapsed)})\n");

            }
            else
            {
                Log.Info("Loading mod package index...");

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                await Task.Delay(100);
                string packageContent = File.ReadAllText(jsonPath);
                parsePackageString(packageContent);

                TimeSpan elapsed = stopwatch.Elapsed;
                Log.Info($"DONE! ({Log.FormatElapsedTime(elapsed)}, Indexed {TSPackageIndex.Count} mods)\n");
            }
        }

        public static void parsePackageString(string jsonContent)
        {
            JToken jsonToken = JToken.Parse(jsonContent);

            if (jsonToken is JArray jsonArray)
            {
                foreach (JObject jsonObject in jsonArray)
                {
                    ModInfo modInfo = new ModInfo() { ModNamespace = jsonObject["owner"].ToString(), ModName = jsonObject["name"].ToString() };

                    TSPackageIndex.Add(modInfo);
                }
            }
        }
    }
}