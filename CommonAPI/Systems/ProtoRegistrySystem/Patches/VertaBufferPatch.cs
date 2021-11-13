using System.IO;
using CommonAPI.Systems;
using HarmonyLib;

namespace CommonAPI.Patches
{
    [HarmonyPatch]
    static class VertaBufferPatch
    {
        [HarmonyPatch(typeof(VertaBuffer), "LoadFromFile")]
        [HarmonyPrefix]
        public static bool Prefix(ref string filename)
        {
            foreach (var resource in ProtoRegistry.modResources)
            {
                if (!filename.ToLower().Contains(resource.keyWord.ToLower()) || !resource.HasVertaFolder()) continue;

                string newName = $"{resource.vertaFolder}/{filename}";
                if (!File.Exists(newName)) continue;

                filename = newName;
                CommonAPIPlugin.logger.LogDebug("Loading registered verta file " + filename);
                break;
            }

            return true;
        }
    }
}