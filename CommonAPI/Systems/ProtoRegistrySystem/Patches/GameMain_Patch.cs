using CommonAPI.Systems;
using HarmonyLib;

namespace CommonAPI.Patches
{
    [HarmonyPatch]
    public static class GameMain_Patch
    {
        [HarmonyPatch(typeof(GameMain), "Begin")]
        [HarmonyPostfix]
        public static void OnGameBegin()
        {
            ModProtoHistory.DisplayRemovedMessage();
        }
        
    }
}