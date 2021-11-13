using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CommonAPI.Systems;
using HarmonyLib;
using UnityEngine;

namespace CommonAPI.Patches
{
    [HarmonyPatch]
    public static class UIAssemblerWindowPatch
    {
        [HarmonyPatch(typeof(UIAssemblerWindow), "OnSelectRecipeClick")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> ChangePicker(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(i => i.opcode == OpCodes.Call && ((MethodInfo) i.operand).Name == "Popup"))
                .Advance(-1)
                .SetAndAdvance(OpCodes.Ldarg_0, null)
                .InsertAndAdvance(Transpilers.EmitDelegate<Func<UIAssemblerWindow, Func<RecipeProto, bool>>>(window =>
                {
                    int entityId = window.factorySystem.assemblerPool[window.assemblerId].entityId;
                    ItemProto itemProto = LDB.items.Select(window.factory.entityPool[entityId].protoId);
                    ERecipeType assemblerRecipeType = itemProto.prefabDesc.assemblerRecipeType;
                    int customRecipeType = itemProto.prefabDesc.GetProperty<int>(ExtendedAssemberDesc.RECIPE_TYPE_NAME);
                    
                    return proto =>
                    {
                        if (proto.Type != assemblerRecipeType) return false;
                        
                        if (assemblerRecipeType == ERecipeType.Custom)
                        {
                            return proto.BelongsToType(customRecipeType);
                        }
                        return true;
                    };
                }))
                .SetInstruction(Transpilers.EmitDelegate<Action<Vector2, Action<RecipeProto>, Func<RecipeProto, bool>>>(UIRecipePickerExtension.Popup));

            return matcher.InstructionEnumeration();
        }
    }
}