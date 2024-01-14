using System.Collections.Generic;
using System.Threading.Tasks;


namespace LC_Sync.Core.LCSync
{
    internal class RequiredMods
    {
        // Standard required mods 
        public static List<ModInfo> requiredMods = new List<ModInfo> { 
            new ModInfo() {ModName = "LC_API", ModNamespace = "2018"}
        };

        RequiredMods() { }

        public static async Task<List<TSMod>> GetRequiredMods() {
            List<TSMod> requiredModList = new List<TSMod> { };

            foreach (ModInfo mod in requiredMods) {
                TSMod newMod = new TSMod(mod.ModNamespace, mod.ModName);
                await newMod.InitializeAsync();

                requiredModList.Add(newMod);
            }

            return requiredModList;
        }
    }

    public class ModInfo
    {
        public string ModName { get; set; }
        public string ModNamespace { get; set; }
    }
}
