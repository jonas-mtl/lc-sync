using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;


namespace LC_Sync.Core.LCSync
{
    public class TSMod
    {
        public string ModNamespace { get; set; }
        public string ModName { get; set; }
        public string ModDownloadUrl { get; set; }
        public List<TSMod> ModDependencies { get; set; }

        public TSMod(string modNamespace, string modName)
        {
            ModNamespace = modNamespace;
            ModName = modName;
        }

        public async Task InitializeAsync()
        {
            JToken modDataJSON = await LCSyncData.LCSyncRequests.GetTSModDataAsync(this.ModNamespace, this.ModName);

            if (modDataJSON == null) { return; }

            ModDownloadUrl = modDataJSON["latest"]?["download_url"]?.ToString();
            ModDependencies = await GetDependencies(modDataJSON["latest"]?["dependencies"]);

            // Add dependencies to final mod list
            LCSyncData.TSMods = LCSyncData.TSMods.Union(ModDependencies).Distinct().ToList();
        }

        private async Task<List<TSMod>> GetDependencies(JToken dependenciesToken)
        {
            List<TSMod> dependencies = new List<TSMod> { };

            if (dependenciesToken is JArray dependenciesArray)
            {
                foreach (JToken dependencyToken in dependenciesArray)
                {
                    string dependency = dependencyToken?.ToString();

                    if (!string.IsNullOrEmpty(dependency))
                    {
                        string[] segments = dependency.Split('-');

                        // Create new TSMod as dependency
                        if (segments.Length > 0)
                        {
                            TSMod newMod = new TSMod(segments[0].ToString(), segments[1].ToString());
                            await newMod.InitializeAsync();

                            dependencies.Add(newMod);
                        }
                    }
                }
            }

            return dependencies;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            TSMod other = (TSMod)obj;
            return ModNamespace == other.ModNamespace && ModName == other.ModName;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (ModNamespace?.GetHashCode() ?? 0);
                hash = hash * 23 + (ModName?.GetHashCode() ?? 0);
                return hash;
            }
        }
    }
}