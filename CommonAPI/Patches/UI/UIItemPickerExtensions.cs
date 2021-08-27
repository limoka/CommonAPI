using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace CommonAPI
{
    public class PickerNotReadyException : Exception
    {
        public PickerNotReadyException() : base("UIItemPicker is not ready!") { }
    }

    [HarmonyPatch]
    public class UIItemPickerExtensions
    {
        public static Func<ItemProto, bool> currentFilter;

        public static IItemPickerExtension currentExtension;

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
                .InsertAndAdvance(Transpilers.EmitDelegate<Func<ItemProto, bool>>(proto => currentFilter == null || currentFilter.Invoke(proto)))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Brfalse, label))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_1))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_3))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldelem_Ref));

            return matcher.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(UIItemPicker), "Popup", typeof(Vector2), typeof(Action<ItemProto>))]
        [HarmonyPrefix]
        public static void IgnoreFilter(UIItemPicker __instance)
        {
            currentFilter = null;
            currentExtension = null;
        }

        [HarmonyPatch(typeof(UIItemPicker), "OnBoxMouseDown")]
        [HarmonyPrefix]
        public static bool OnBoxMouseDown(UIItemPicker __instance)
        {
            if (currentExtension == null) return true;

            return currentExtension.OnBoxMouseDown(__instance);
        }

        [HarmonyPatch(typeof(UIItemPicker), "TestMouseIndex")]
        [HarmonyPostfix]
        public static void TestMouseIndex(UIItemPicker __instance)
        {
            if (currentExtension == null) return;

            currentExtension.TestMouseIndex(__instance);
        }

        [HarmonyPatch(typeof(UIItemPicker), "_OnOpen")]
        [HarmonyPostfix]
        public static void Open(UIItemPicker __instance)
        {
            if (currentExtension == null) return;

            currentExtension.Open(__instance);
        }

        [HarmonyPatch(typeof(UIItemPicker), "_OnClose")]
        [HarmonyPostfix]
        public static void Close(UIItemPicker __instance)
        {
            if (currentExtension == null) return;
            currentExtension.Close(__instance);
        }

        public static UIItemPicker PreparePicker()
        {
            if (UIRoot.instance == null)
            {
                throw new PickerNotReadyException();
            }

            UIItemPicker itemPicker = UIRoot.instance.uiGame.itemPicker;
            if (!itemPicker.inited || itemPicker.active)
            {
                throw new PickerNotReadyException();
            }

            return itemPicker;
        }

        public static void Popup(Vector2 pos, Action<ItemProto> _onReturn, Func<ItemProto, bool> filter)
        {
            try
            {
                currentExtension = null;
                UIItemPicker itemPicker = PreparePicker();
                if (itemPicker == null)
                {
                    _onReturn?.Invoke(null);
                }

                currentFilter = filter;

                itemPicker.onReturn = _onReturn;
                itemPicker._Open();
                itemPicker.pickerTrans.anchoredPosition = pos;
            }
            catch (PickerNotReadyException)
            {
                _onReturn?.Invoke(null);
            }
        }

        public static void Popup(Vector2 pos, IItemPickerExtension extension)
        {
            try
            {
                currentExtension = extension;
                UIItemPicker itemPicker = PreparePicker();

                extension.OnPopup(itemPicker);

                itemPicker._Open();
                itemPicker.pickerTrans.anchoredPosition = pos;

                extension.PostPopup(itemPicker);
            }
            catch (PickerNotReadyException) { }
        }
    }
}