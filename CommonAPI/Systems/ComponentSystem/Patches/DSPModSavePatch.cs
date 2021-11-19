using crecheng.DSPModSave;
using HarmonyLib;

namespace CommonAPI.Patches
{
    // This patch is needed because DSPModSave author is inactive and
    // this is the only way to fix issue CommonAPI runs into.
    [HarmonyPatch]
    public static class DSPModSavePatch
    {

        [HarmonyPatch(typeof(DSPModSave), "EnterGame")]
        [HarmonyPrefix]
        public static bool CancelIntoOtherSaveEarly()
        {
            return false;
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameData), "NewGame")]
        public static void ActualEnterGameCall()
        {
            CommonAPIPlugin.logger.LogInfo("New game is being loaded. IntoOtherSave is called.");
            foreach (var d in DSPModSave.AllModData)
            {
                d.Value.IntoOtherSave();
            }
        }
        
    }
}