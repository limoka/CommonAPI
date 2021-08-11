using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace CommonAPI
{
    [HarmonyPatch]
    public class UIRecipePickerExtensions
    {
        public static Func<RecipeProto, bool> currentFilter;

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
                .InsertAndAdvance(Transpilers.EmitDelegate<Func<RecipeProto, bool>>(proto => currentFilter == null || currentFilter.Invoke(proto)))
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
            currentFilter = null;
        }

        public static void Popup(Vector2 pos, Action<RecipeProto> _onReturn, Func<RecipeProto, bool> filter)
        {
            currentFilter = filter;
            if (UIRoot.instance == null)
            {
                _onReturn?.Invoke(null);
                return;
            }

            UIRecipePicker recipePicker = UIRoot.instance.uiGame.recipePicker;
            if (!recipePicker.inited || recipePicker.active)
            {
                _onReturn?.Invoke(null);
                return;
            }

            recipePicker.filter = ERecipeType.None;
            recipePicker.onReturn = _onReturn;
            recipePicker._Open();
            recipePicker.pickerTrans.anchoredPosition = pos;
        }
    }
}