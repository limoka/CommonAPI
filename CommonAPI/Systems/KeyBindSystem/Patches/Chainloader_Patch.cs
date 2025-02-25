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
        private const int OverrideKeysNewSize = 512;

        private delegate T2 RefAction<T, out T2>(ref T arg);


        [HarmonyPatch(typeof(GameOption), nameof(GameOption.ImportXML))]
        [HarmonyPrefix]
        public static void AfterAllLoaded()
        {
            // Resize overrideKeys and overrideKeysChanged arrays to 512, to prevent out of bounds exceptions
            Array.Resize(ref VFInput.override_keys, OverrideKeysNewSize);
            Array.Resize(ref DSPGame.globalOption.overrideKeys, OverrideKeysNewSize);
            Array.Resize(ref DSPGame.globalOption.overrideKeysChanged, OverrideKeysNewSize);

            CommonAPIPlugin.logger.LogDebug("Loading config file");
            string path = $"{Paths.ConfigPath}/CommonAPI/keybinds";
            try
            {
                FileStream stream = File.OpenRead(path);
                BinaryReader reader = new BinaryReader(stream);
                CustomKeyBindSystem.Import(reader);
                stream.Dispose();
                return;
            }
            catch (Exception)
            {
                CommonAPIPlugin.logger.LogDebug("keybinds not found, fallbacking to load legacy keybindmapping");
            }
            path = $"{Paths.ConfigPath}/CommonAPI/keybindmapping";
            try
            {
                FileStream stream = File.OpenRead(path);
                BinaryReader reader = new BinaryReader(stream);
                CustomKeyBindSystem.keyRegistry.Import(reader);
                stream.Dispose();
            }
            catch (Exception)
            {
                CommonAPIPlugin.logger.LogDebug("Error loading keybind file.");
            }
        }

        [HarmonyPatch(typeof(GameOption), nameof(GameOption.Apply))]
        [HarmonyPostfix]
        public static void SaveKeyBindsOnApplyOptions()
        {
            CommonAPIPlugin.logger.LogDebug("saving config file");
            Directory.CreateDirectory($"{Paths.ConfigPath}/CommonAPI/");
            string path = $"{Paths.ConfigPath}/CommonAPI/keybinds";
            FileStream stream = File.Create(path);
            BinaryWriter writer = new BinaryWriter(stream);

            CustomKeyBindSystem.Export(writer);
            stream.Dispose();
        }

        // Search for:
        //   int num = 256;
        //   this.tempOption.overrideKeys = new CombineKey[num];
        // Set `num` to 512
        [HarmonyPatch(typeof(UIOptionWindow), nameof(UIOptionWindow._OnOpen))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PatchOverrideKeysLength(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeMatcher matcher = new CodeMatcher(instructions, generator)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldc_I4),
                    new CodeMatch(ci => ci.IsStloc()),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldflda, AccessTools.Field(typeof(UIOptionWindow), nameof(UIOptionWindow.tempOption))),
                    new CodeMatch(ci => ci.IsLdloc()),
                    new CodeMatch(OpCodes.Newarr, typeof(CombineKey))
                );
            matcher.Set(OpCodes.Ldc_I4, OverrideKeysNewSize);
            return matcher.InstructionEnumeration();
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

        // Do not save overrideKeys with id >= 256 to options.xml
        //  Search for:
        //   int num2 = this.overrideKeys.Length;
        //  Replace with:
        //   int num2 = Math.Min(this.overrideKeys.Length, 256);
        [HarmonyPatch(typeof(GameOption), nameof(GameOption.ExportXML))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> FixOverrideKeysExport(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeMatcher matcher = new CodeMatcher(instructions, generator)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(GameOption), nameof(GameOption.overrideKeys))),
                    new CodeMatch(OpCodes.Ldlen),
                    new CodeMatch(OpCodes.Conv_I4),
                    new CodeMatch(ci => ci.IsStloc())
                );

            matcher.InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldc_I4, 256),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Math), nameof(Math.Min), new[] { typeof(int), typeof(int) }))
            );

            return matcher.InstructionEnumeration();
        }
    }
}