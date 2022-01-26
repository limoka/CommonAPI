using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using CommonAPI.Systems;
using HarmonyLib;
using UnityEngine;

namespace CommonAPI.Patches
{
    [HarmonyPatch]
    public class UIRecipePicker_Patch
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
            
            matcher.MatchForward(true,
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(Proto), nameof(Proto.ID))),
                    new CodeMatch(OpCodes.Callvirt))
                .SetInstruction(Transpilers.EmitDelegate<Func<GameHistoryData, int, bool>>(CheckRecipe));

            return matcher.InstructionEnumeration();
        }
        
        public static bool CheckRecipe(GameHistoryData history, int recipeId)
        {
            if (UIRecipePickerExtension.showLocked) return true;
            return history.RecipeUnlocked(recipeId);
        }

        [HarmonyPatch(typeof(UIRecipePicker), "Popup", typeof(Vector2), typeof(Action<RecipeProto>))]
        [HarmonyPatch(typeof(UIRecipePicker), "Popup", typeof(Vector2), typeof(Action<RecipeProto>), typeof(ERecipeType))]
        [HarmonyPrefix]
        public static void IgnoreFilter(UIRecipePicker __instance)
        {
            UIRecipePickerExtension.currentFilter = null;
            UIRecipePickerExtension.showLocked = false;
            UIRecipePickerExtension.currentExtension = null;
        }

        [HarmonyPatch(typeof(UIRecipePicker), "OnBoxMouseDown")]
        [HarmonyPrefix]
        public static bool OnBoxMouseDown(UIRecipePicker __instance)
        {
            if (UIRecipePickerExtension.currentExtension == null) return true;

            if (UIRecipePickerExtension.currentExtension is IMouseHandlerExtension<UIRecipePicker> mouseHandler)
            {
                return mouseHandler.OnBoxMouseDown(__instance);
            }
            return true;
        }

        [HarmonyPatch(typeof(UIRecipePicker), "TestMouseIndex")]
        [HarmonyPostfix]
        public static void TestMouseIndex(UIRecipePicker __instance)
        {
            if (UIRecipePickerExtension.currentExtension == null) return;

            if (UIRecipePickerExtension.currentExtension is IMouseHandlerExtension<UIRecipePicker> mouseHandler)
            {
                mouseHandler.TestMouseIndex(__instance);
            }
        }

        [HarmonyPatch(typeof(UIRecipePicker), "_OnOpen")]
        [HarmonyPostfix]
        public static void Open(UIRecipePicker __instance)
        {
            if (UIRecipePickerExtension.currentExtension == null) return;

            UIRecipePickerExtension.currentExtension.Open(__instance);
        }

        [HarmonyPatch(typeof(UIRecipePicker), "_OnClose")]
        [HarmonyPostfix]
        public static void Close(UIRecipePicker __instance)
        {
            if (UIRecipePickerExtension.currentExtension == null) return;
            UIRecipePickerExtension.currentExtension.Close(__instance);
        }
        
        [HarmonyPatch(typeof(UIRecipePicker), "_OnUpdate")]
        [HarmonyPostfix]
        public static void Update(UIRecipePicker __instance)
        {
            if (UIRecipePickerExtension.currentExtension == null) return;
            
            if (UIRecipePickerExtension.currentExtension is IUpdatePickerExtension<UIRecipePicker> mouseHandler)
            {
                mouseHandler.OnUpdate(__instance);
            }
        }
    }
}