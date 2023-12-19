using HarmonyLib;
using UnityEngine.UI;

namespace CommonAPI.Systems.ModLocalization.Patch
{
    public class Text_Patch
    {

        [HarmonyPatch(typeof(Text), nameof(Text.font), MethodType.Getter)]
        [HarmonyPrefix]
        public static void Prefix(Text __instance)
        {
            LocalizationModule.Get(__instance)?.OnGetFont();
        }
    }
}