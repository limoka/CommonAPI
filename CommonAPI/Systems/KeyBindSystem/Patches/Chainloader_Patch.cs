using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Bootstrap;
using CommonAPI.Systems;
using HarmonyLib;
using UnityEngine;

namespace CommonAPI.Patches
{
    public static class Chainloader_Patch
    {
        public delegate T2 RefAction<T, out T2>(ref T arg);


        [HarmonyPatch(typeof(GameOption), nameof(GameOption.ImportXML))]
        [HarmonyPrefix]
        public static void AfterAllLoaded()
        {
            CommonAPIPlugin.logger.LogDebug("Loading config file");
            string path = $"{Paths.ConfigPath}/CommonAPI/keybindmapping";
            try
            {
                FileStream stream = File.OpenRead(path);
                BinaryReader reader = new BinaryReader(stream);
                CustomKeyBindSystem.keyRegistry.Import(reader);
                stream.Close();
            }
            catch (Exception e)
            {
                CommonAPIPlugin.logger.LogDebug("Error loading keybind file.");
            }
        }

        [HarmonyPatch(typeof(GameOption), nameof(GameOption.ImportXML))]
        [HarmonyPostfix]
        public static void OnLoadXML()
        {
            CommonAPIPlugin.logger.LogDebug("saving config file");
            Directory.CreateDirectory($"{Paths.ConfigPath}/CommonAPI/");
            string path = $"{Paths.ConfigPath}/CommonAPI/keybindmapping";
            FileStream stream = File.Open(path, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(stream);

            CustomKeyBindSystem.keyRegistry.Export(writer);
            stream.Close();
        }

        [HarmonyPatch(typeof(GameOption), nameof(GameOption.ImportXML))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> MigrateIds(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeMatcher matcher = new CodeMatcher(instructions, generator)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldstr, "OverrideKeysId"),
                    new CodeMatch(OpCodes.Callvirt),
                    new CodeMatch(OpCodes.Callvirt),
                    new CodeMatch(OpCodes.Ldloca_S)
                );

            object arg = matcher.Operand;

            // Find loop end code
            int targetPos = matcher.Clone()
                .MatchForward(false,
                    new CodeMatch(OpCodes.Stloc_S))
                .Advance(-3).Pos;

            matcher.CreateLabelAt(targetPos, out Label label);

            // Insert check
            matcher.Advance(3)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloca_S, arg))
                .InsertAndAdvance(Transpilers.EmitDelegate<RefAction<int, bool>>((ref int oldId) =>
                {
                    if (CustomKeyBindSystem.keyRegistry.removedIntIds.Contains(oldId))
                    {
                        CommonAPIPlugin.logger.LogDebug($"Discarding KeyBind ID: {oldId}");
                        return false;
                    }

                    int newId = CustomKeyBindSystem.keyRegistry.MigrateId(oldId);
                    if (newId != 0)
                    {
                        CommonAPIPlugin.logger.LogDebug($"Migrating KeyBind ID: {oldId} => {newId}");
                        oldId = newId;
                    }

                    return true;
                }))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Brfalse, label));


            return matcher.InstructionEnumeration();
        }
    }
}