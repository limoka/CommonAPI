using HarmonyLib;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CommonAPI.Systems.ModLocalization.Patch
{
    public class UIBehaviour_Patch
    {
        [HarmonyPatch(typeof(UIBehaviour), "Awake")]
        [HarmonyPrefix]
        public static void OnAwake(UIBehaviour __instance)
        {
            if (__instance is Text text)
            {
                LocalizationModule.Add(text);
            }
        }
        
        [HarmonyPatch(typeof(UIBehaviour), "OnDestroy")]
        [HarmonyPrefix]
        public static void OnOnDestroy(UIBehaviour __instance)
        {
            if (__instance is Text text)
            {
                LocalizationModule.Remove(text);
            }
        }
    }
}