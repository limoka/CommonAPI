using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using CommonAPI.Systems;
using HarmonyLib;
using UnityEngine;

namespace CommonAPI.Patches
{

    [HarmonyPatch]
    public class UIItemPickerExtPatches
    {
        [HarmonyPatch(typeof(UIItemPicker), "RefreshIcons")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> AddItemFilter(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(ItemProto), nameof(ItemProto.GridIndex))),
                    new CodeMatch(OpCodes.Ldc_I4)
                ).Advance(1);
            Label label = (Label) matcher.Instruction.operand;

            matcher.Advance(-2)
                .InsertAndAdvance(Transpilers.EmitDelegate<Func<ItemProto, bool>>(proto => UIItemPickerExtension.currentFilter == null || UIItemPickerExtension.currentFilter.Invoke(proto)))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Brfalse, label))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_1))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_3))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldelem_Ref));

            matcher.MatchForward(true,
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Proto), nameof(Proto.ID))),
                    new CodeMatch(OpCodes.Callvirt))
                .SetInstruction(Transpilers.EmitDelegate<Func<GameHistoryData, int, bool>>(CheckItem));


            return matcher.InstructionEnumeration();
        }

        public static bool CheckItem(GameHistoryData history,int itemId)
        {
            if (UIItemPickerExtension.showLocked) return true;
            return history.ItemUnlocked(itemId);
        }

        [HarmonyPatch(typeof(UIItemPicker), "Popup", typeof(Vector2), typeof(Action<ItemProto>))]
        [HarmonyPrefix]
        public static void IgnoreFilter(UIItemPicker __instance)
        {
            UIItemPickerExtension.currentFilter = null;
            UIItemPickerExtension.currentExtension = null;
            UIItemPickerExtension.showLocked = false;
        }

        [HarmonyPatch(typeof(UIItemPicker), "OnBoxMouseDown")]
        [HarmonyPrefix]
        public static bool OnBoxMouseDown(UIItemPicker __instance)
        {
            if (UIItemPickerExtension.currentExtension == null) return true;

            return UIItemPickerExtension.currentExtension.OnBoxMouseDown(__instance);
        }

        [HarmonyPatch(typeof(UIItemPicker), "TestMouseIndex")]
        [HarmonyPostfix]
        public static void TestMouseIndex(UIItemPicker __instance)
        {
            if (UIItemPickerExtension.currentExtension == null) return;

            UIItemPickerExtension.currentExtension.TestMouseIndex(__instance);
        }

        [HarmonyPatch(typeof(UIItemPicker), "_OnOpen")]
        [HarmonyPostfix]
        public static void Open(UIItemPicker __instance)
        {
            if (UIItemPickerExtension.currentExtension == null) return;

            UIItemPickerExtension.currentExtension.Open(__instance);
        }

        [HarmonyPatch(typeof(UIItemPicker), "_OnClose")]
        [HarmonyPostfix]
        public static void Close(UIItemPicker __instance)
        {
            if (UIItemPickerExtension.currentExtension == null) return;
            UIItemPickerExtension.currentExtension.Close(__instance);
        }
    }
}