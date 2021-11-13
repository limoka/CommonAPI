using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace CommonAPI.Systems.Patches
{
    [HarmonyPatch]
    public class GameLoaderPatch
    {
        public delegate void RefAction<T1>(ref T1 arg1);

        [HarmonyPatch(typeof(GameLoader), "FixedUpdate")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> AddModificationWarn(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldloc_0),
                    new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(string), nameof(string.IsNullOrEmpty))),
                    new CodeMatch(OpCodes.Brtrue)
                )
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloca_S, 0))
                .InsertAndAdvance(
                    Transpilers.EmitDelegate<RefAction<string>>((ref string text) =>
                    {
                        if (UtilSystem.messageHandlers.Count > 0)
                        {
                            foreach (Func<string> handler in UtilSystem.messageHandlers)
                            {
                                string message = handler();
                                if (!string.IsNullOrEmpty(message))
                                {
                                    text = text + "\r\n" + message;
                                }
                            }
                        }
                    }));


            return matcher.InstructionEnumeration();
        }
    }
}