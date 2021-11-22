using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using CommonAPI.Systems;
using HarmonyLib;
using UnityEngine;

namespace CommonAPI.Patches
{
    [HarmonyPatch]
    public class UIRecipePickerExtPatch
    {

        [HarmonyPatch(typeof(UIRecipePicker), "RefreshIcons")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> AddItemFilter(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(RecipeProto), nameof(RecipeProto.GridIndex))),
                    new CodeMatch(OpCodes.Ldc_I4)
                ).Advance(1);
            Label label = (Label) matcher.Instruction.operand;

            matcher.Advance(-2)
                .InsertAndAdvance(Transpilers.EmitDelegate<Func<RecipeProto, bool>>(proto => UIRecipePickerExtension.currentFilter == null || UIRecipePickerExtension.currentFilter.Invoke(proto)))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Brfalse, label))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_1))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_3))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldelem_Ref));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UIRecipePicker), "Popup", typeof(Vector2), typeof(Action<RecipeProto>))]
        [HarmonyPatch(typeof(UIRecipePicker), "Popup", typeof(Vector2), typeof(Action<RecipeProto>), typeof(ERecipeType))]
        [HarmonyPrefix]
        public static void IgnoreFilter(UIRecipePicker __instance)
        {
            UIRecipePickerExtension.currentFilter = null;
        }
    }
}