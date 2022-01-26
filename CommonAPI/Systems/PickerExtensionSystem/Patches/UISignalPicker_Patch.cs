using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CommonAPI.Systems;
using HarmonyLib;

namespace CommonAPI.Patches
{
    [HarmonyPatch]
    public class UISignalPicker_Patch
    {
        
        [HarmonyPatch(typeof(UISignalPicker), "RefreshIcons")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> AddItemFilter(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions);
                
            MatchProto<SignalProto>(matcher, i => i);
            matcher.Advance(30);
            
            MatchProto<ItemProto>(matcher, i => i);
            matcher.Advance(30);
            
            MatchProto<ItemProto>(matcher, i => i);
            matcher.Advance(30);
            
            matcher.MatchForward(false,
                new CodeMatch(x => x.IsLdloc()),
                new CodeMatch(x => x.IsLdloc()),
                new CodeMatch(OpCodes.Ldelem_Ref),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(RecipeProto), nameof(RecipeProto.hasIcon)))
            );


            MatchProto<RecipeProto>(matcher, i => i + 20000, true, 2);
            
            matcher.MatchForward(false,
                new CodeMatch(x => x.IsLdloc()),
                new CodeMatch(x => x.IsLdloc()),
                new CodeMatch(OpCodes.Ldelem_Ref),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(TechProto), nameof(TechProto.Published)))
            );
            
            
            MatchProto<TechProto>(matcher, i => i + 40000, true, 2);
            
            matcher.MatchForward(false,
                new CodeMatch(x => x.IsLdloc()),
                new CodeMatch(x => x.IsLdloc()),
                new CodeMatch(OpCodes.Ldelem_Ref),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(TechProto), nameof(TechProto.Published)))
            );

            MatchProto<TechProto>(matcher, i => i + 40000, true, 2);

            return matcher.InstructionEnumeration();
        }

        private static void MatchProto<T>(CodeMatcher matcher, Func<int, int> index, bool noMatch = false, int offset = 3)
        where T : Proto
        {
            if (!noMatch)
            {
                matcher.MatchForward(false,
                    new CodeMatch(x => x.IsLdloc()),
                    new CodeMatch(x => x.IsLdloc()),
                    new CodeMatch(OpCodes.Ldelem_Ref),
                    new CodeMatch(x => x.opcode == OpCodes.Ldfld && ((FieldInfo) x.operand).Name == "GridIndex"),
                    new CodeMatch(OpCodes.Ldc_I4)
                );
            }

            matcher.GetInstructionAndAdvance(out OpCode arg1Opcode, out object arg1Operand)
                .GetInstructionAndAdvance(out OpCode arg2Opcode, out object arg2Operand)
                .Advance(offset)
                .GetLabel(out Label label)
                .Advance(-offset + 1)
                .InsertAndAdvance(Transpilers.EmitDelegate<Func<T, bool>>(proto =>
                {
                    return UISignalPickerExtension.currentFilter == null || UISignalPickerExtension.currentFilter.Invoke(index(proto.ID));
                }))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Brfalse, label))
                .InsertAndAdvance(new CodeInstruction(arg1Opcode, arg1Operand))
                .InsertAndAdvance(new CodeInstruction(arg2Opcode, arg2Operand))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldelem_Ref));
        }

        [HarmonyPatch(typeof(UISignalPicker), "Popup")]
        [HarmonyPrefix]
        public static void IgnoreFilter(UISignalPicker __instance)
        {
            UISignalPickerExtension.currentFilter = null;
            UISignalPickerExtension.extensions.Clear();
            UISignalPickerExtension.extensions.Add(UISignalPickerExtension.GetTipExtension());
        }

        [HarmonyPatch(typeof(UISignalPicker), "OnBoxMouseDown")]
        [HarmonyPrefix]
        public static bool OnBoxMouseDown(UISignalPicker __instance)
        {
            if (UISignalPickerExtension.extensions.Count == 0) return true;

            foreach (IPickerExtension<UISignalPicker> extension in UISignalPickerExtension.extensions)
            {
                if (extension is IMouseHandlerExtension<UISignalPicker> mouseHandler)
                {
                    return mouseHandler.OnBoxMouseDown(__instance);
                }
            }
            
            return true;
        }

        [HarmonyPatch(typeof(UISignalPicker), "TestMouseIndex")]
        [HarmonyPostfix]
        public static void TestMouseIndex(UISignalPicker __instance)
        {
            if (UISignalPickerExtension.extensions.Count == 0) return;

            foreach (IPickerExtension<UISignalPicker> extension in UISignalPickerExtension.extensions)
            {
                if (extension is IMouseHandlerExtension<UISignalPicker> mouseHandler)
                {
                    mouseHandler.TestMouseIndex(__instance);
                }
            }
        }

        [HarmonyPatch(typeof(UISignalPicker), "_OnOpen")]
        [HarmonyPostfix]
        public static void Open(UISignalPicker __instance)
        {
            if (UISignalPickerExtension.extensions.Count == 0) return;

            foreach (IPickerExtension<UISignalPicker> extension in UISignalPickerExtension.extensions)
            {
                extension.Open(__instance);
            }
        }

        [HarmonyPatch(typeof(UISignalPicker), "_OnClose")]
        [HarmonyPostfix]
        public static void Close(UISignalPicker __instance)
        {
            if (UISignalPickerExtension.extensions.Count == 0) return;

            foreach (IPickerExtension<UISignalPicker> extension in UISignalPickerExtension.extensions)
            {
                extension.Close(__instance);
            }
        }
        
        [HarmonyPatch(typeof(UISignalPicker), "_OnUpdate")]
        [HarmonyPostfix]
        public static void Update(UISignalPicker __instance)
        {
            if (UISignalPickerExtension.extensions.Count == 0) return;

            foreach (IPickerExtension<UISignalPicker> extension in UISignalPickerExtension.extensions)
            {
                if (extension is IUpdatePickerExtension<UISignalPicker> updateHandler)
                {
                    updateHandler.OnUpdate(__instance);
                }
            }
        }
    }
}