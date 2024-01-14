using System;
using System.Net.Http;
using System.Threading.Tasks;
using LC_Sync.Core.Util;
using Newtonsoft.Json.Linq;


namespace LC_Sync.Core.LCSync
{
    internal class LCSyncRequests
    {
        public LCSyncRequests() { }

        private static readonly HttpClient client = new HttpClient();

        public async Task<HttpResponseMessage> RequestURL(string url)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return response;
                }
                else
                {
                    // Handle unsuccessful response
                    Log.Errored($"Request failed with status code: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occurred during the request
                Log.Errored($"Error: {ex.Message}");
                return null;
            }
        }

        public async Task<JToken> GetSrcbinDataAsync(string binKey)
        {
            string url = $"https://cdn.sourceb.in/bins/{binKey}/0";
            HttpResponseMessage response = await RequestURL(url);

            if (response == null) { return null; }

            string jsonContent = await response.Content.ReadAsStringAsync();
            JToken jsonData = JToken.Parse(jsonContent);
            return jsonData;
        }

        public async Task<JToken> GetTSModDataAsync(string modNamespace, string modName)
        {
            string url = $"https://thunderstore.io/api/experimental/package/{modNamespace}/{modName}";
            HttpResponseMessage response = await RequestURL(url);

            if (response == null) { return null; }

            string jsonContent = await response.Content.ReadAsStringAsync();
            JToken jsonData = JToken.Parse(jsonContent);
            return jsonData;
        }

        public async Task<JToken> GetTSModPackageIndex()
        {
            string url = $"https://thunderstore.io/api/experimental/package-index/";
            HttpResponseMessage response = await RequestURL(url);

            if (response == null) { return null; }

            string jsonContent = await response.Content.ReadAsStringAsync();
            JToken jsonData = JToken.Parse(jsonContent);
            return jsonData;
        }

    }
}
