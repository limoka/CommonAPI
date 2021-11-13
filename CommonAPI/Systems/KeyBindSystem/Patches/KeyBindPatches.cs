using System;
using System.Linq;
using CommonAPI.Systems;
using HarmonyLib;

namespace CommonAPI.Patches
{
    [HarmonyPatch]
    public class KeyBindPatches
    {
        [HarmonyPatch(typeof(UIOptionWindow), "_OnCreate")]
        [HarmonyPrefix]
        private static void AddKeyBind(UIOptionWindow __instance)
        {
            PressKeyBind[] newKeys = CustomKeyBindSystem.customKeys.Values.ToArray();
            if (newKeys.Length == 0) return;

            int index = DSPGame.key.builtinKeys.Length;
            Array.Resize(ref DSPGame.key.builtinKeys, index + CustomKeyBindSystem.customKeys.Count);

            for (int i = 0; i < newKeys.Length; i++)
            {
                DSPGame.key.builtinKeys[index + i] = newKeys[i].defaultBind;
            }
        }
    }
}