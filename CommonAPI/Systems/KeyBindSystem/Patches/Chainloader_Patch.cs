using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Bootstrap;
using CommonAPI.Systems;
using HarmonyLib;

namespace CommonAPI.Patches
{
    public static class Chainloader_Patch
    {
        public delegate void RefAction<T>(ref T arg);


        [HarmonyPatch(typeof(Chainloader), nameof(Chainloader.Start))]
        [HarmonyPostfix]
        public static void AfterAllLoaded()
        {
            string path = $"{Paths.ConfigPath}/CommonAPI/keybindmapping";
            if (File.Exists(path))
            {
                FileStream stream = File.OpenRead(path);
                BinaryReader reader = new BinaryReader(stream);
                CustomKeyBindSystem.keyRegistry.Import(reader);
            }
        }

        [HarmonyPatch(typeof(GameOption), nameof(GameOption.ImportXML))]
        [HarmonyPostfix]
        public static void OnLoadXML()
        {
            Directory.CreateDirectory($"{Paths.ConfigPath}/CommonAPI/");
            string path = $"{Paths.ConfigPath}/CommonAPI/keybindmapping";
            FileStream stream = File.Open(path, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(stream);
            
            CustomKeyBindSystem.keyRegistry.Export(writer);
        }

        [HarmonyPatch(typeof(GameOption), nameof(GameOption.ImportXML))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> MigrateIds(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(OpCodes.Ldstr, "OverrideKeysId"),
                    new CodeMatch(OpCodes.Callvirt),
                    new CodeMatch(OpCodes.Callvirt),
                    new CodeMatch(OpCodes.Ldloca_S)
                );

            object arg = matcher.Operand;

            matcher.Advance(3)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloca_S, arg))
                .InsertAndAdvance(Transpilers.EmitDelegate<RefAction<int>>((ref int oldId) =>
                {
                    int newId = CustomKeyBindSystem.keyRegistry.MigrateId(oldId);
                    if (newId != 0)
                    {
                        CommonAPIPlugin.logger.LogDebug($"Migrating KeyBind ID: {oldId} => {newId}");
                        oldId = newId;
                    }
                }));


            return matcher.InstructionEnumeration();
        }
    }
}