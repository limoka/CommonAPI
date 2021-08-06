using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using CommonAPI;
using HarmonyLib;
// ReSharper disable InconsistentNaming

// ReSharper disable UnusedParameter.Global
// ReSharper disable RedundantAssignment

namespace CommonAPI
{


    [HarmonyPatch]
    public static class FactorySystemPatch
    {
        
        //Single thread update calls (Only when game is running in single thread mode)
        [HarmonyPatch(typeof(FactorySystem), "GameTickBeforePower")]
        [HarmonyPostfix]
        public static void PowerTick(FactorySystem __instance, long time, bool isActive)
        {
            CustomFactory.PowerUpdate(__instance.factory);
        }
        
        [HarmonyPatch(typeof(FactorySystem), "GameTick", new Type[] {typeof(long), typeof(bool)})]
        [HarmonyPrefix]
        public static void PreUpdate(FactorySystem __instance, long time, bool isActive)
        {
            CustomFactory.PreUpdate(__instance.factory);
        }

        [HarmonyPatch(typeof(FactorySystem), "GameTickLabOutputToNext", new Type[] {typeof(long), typeof(bool)})]
        [HarmonyPostfix]
        public static void Update(FactorySystem __instance, long time, bool isActive)
        {
            CustomFactory.Update(__instance.factory);
        }
        
        [HarmonyPatch(typeof(PlanetFactory), "GameTick")]
        [HarmonyPostfix]
        public static void PostUpdate(PlanetFactory __instance, long time)
        {
            CustomFactory.PostUpdate(__instance);
        }
        
        //Fall-back calls for systems that do not support multi-thread update calls
        
        [HarmonyPatch(typeof(MultithreadSystem), "PreparePowerSystemFactoryData")]
        [HarmonyPrefix]
        public static void PowerUpdateSinglethread()
        {
            if (!GameMain.multithreadSystem.multithreadSystemEnable) return;
            CustomFactory.PowerUpdateOnlySinglethread(GameMain.data);
        }
        
        [HarmonyPatch(typeof(MultithreadSystem), "PrepareAssemblerFactoryData")]
        [HarmonyPrefix]
        public static void PreUpdateSinglethread()
        {
            if (!GameMain.multithreadSystem.multithreadSystemEnable) return;
            CustomFactory.PreUpdateOnlySinglethread(GameMain.data);
        }
        
        [HarmonyPatch(typeof(MultithreadSystem), "PrepareTransportData")]
        [HarmonyPrefix]
        public static void UpdateSinglethread()
        {
            if (!GameMain.multithreadSystem.multithreadSystemEnable) return;
            CustomFactory.UpdateOnlySinglethread(GameMain.data);
        }
        
        [HarmonyPatch(typeof(TrashSystem), "GameTick")]
        [HarmonyPrefix]
        public static void PostUpdateSinglethread()
        {
            if (!GameMain.multithreadSystem.multithreadSystemEnable) return;
            CustomFactory.PostUpdateOnlySinglethread(GameMain.data);
        }
        
        //Multi-thread update calls, used only if player system support multithreading
        
        [HarmonyPatch(typeof(FactorySystem), "ParallelGameTickBeforePower")]
        [HarmonyPostfix]
        public static void PowerTickMultithread(FactorySystem __instance, long time, bool isActive, int _usedThreadCnt, int _curThreadIdx)
        {
            CustomFactory.PowerUpdateMultithread(__instance.factory, _usedThreadCnt, _curThreadIdx, 4);
        }

        [HarmonyPatch(typeof(FactorySystem), "GameTick", new Type[] {typeof(long), typeof(bool), typeof(int), typeof(int), typeof(int)})]
        [HarmonyPrefix]
        public static void PreUpdateMultithread(FactorySystem __instance, long time, bool isActive, int _usedThreadCnt, int _curThreadIdx)
        {
            CustomFactory.PreUpdateMultithread(__instance.factory, _usedThreadCnt, _curThreadIdx, 4);
        }
        
        [HarmonyPatch(typeof(FactorySystem), "GameTickLabOutputToNext", new Type[] {typeof(long), typeof(bool), typeof(int), typeof(int), typeof(int)})]
        [HarmonyPrefix]
        public static void UpdateMultithread(FactorySystem __instance, long time, bool isActive, int _usedThreadCnt, int _curThreadIdx)
        {
            CustomFactory.UpdateMultithread(__instance.factory, _usedThreadCnt, _curThreadIdx, 4);
        }  
        
        [HarmonyPatch(typeof(CargoTraffic), "GameTickPresentCargoPaths", new Type[] {typeof(long), typeof(bool), typeof(int), typeof(int), typeof(int)})]
        [HarmonyPostfix]
        public static void PostUpdateMultithread(CargoTraffic __instance, long time, bool presentCargos, int _usedThreadCnt, int _curThreadIdx)
        {
            CustomFactory.PostUpdateMultithread(__instance.factory, _usedThreadCnt, _curThreadIdx, 4);
        }
    }


    [HarmonyPatch]
    static class PlanetFactoryCreatePatch
    {
        [HarmonyPatch(typeof(PlanetFactory), "CreateEntityLogicComponents")]
        [HarmonyPostfix]
        public static void Postfix(int entityId, PrefabDesc desc, int prebuildId, PlanetFactory __instance)
        {
            CustomFactory.CreateEntityComponents(__instance, entityId, desc, prebuildId);
        }
    }

    [HarmonyPatch]
    static class PlanetFactoryRemovePatch
    {
        [HarmonyPatch(typeof(PlanetFactory), "RemoveEntityWithComponents")]
        [HarmonyPrefix]
        public static void Prefix(int id, PlanetFactory __instance)
        {
            CustomFactory.RemoveEntityComponents(__instance, id);
        }
    }

    [HarmonyPatch]
    static class PrefabDescPatch
    {
        [HarmonyPatch(typeof(PrefabDesc), "ReadPrefab")]
        [HarmonyPostfix]
        public static void Postfix(PrefabDesc __instance)
        {
            if (__instance.prefab != null)
            {
                __instance.customData = new Dictionary<string, object>();
                CustomDesc[] descs = __instance.prefab.GetComponentsInChildren<CustomDesc>();
                foreach (CustomDesc desc in descs)
                {
                    desc.ApplyProperties(__instance);
                }
            }
        }
    }

    [HarmonyPatch(typeof(BuildTool_Click), "CreatePrebuilds")]
    static class BuildTool_Click2Patch
    {
        public delegate void RefAction<T1, T2>(ref T1 arg1, T2 arg2);

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> AddNewProperty(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldloc_3),
                    new CodeMatch(OpCodes.Ldfld),
                    new CodeMatch(i =>
                        i.opcode == OpCodes.Call && ((MethodInfo) i.operand).Name == "InitParametersArray")
                ).Advance(3)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloca_S, 4))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_3))
                .InsertAndAdvance(Transpilers.EmitDelegate<RefAction<PrebuildData, BuildPreview>>((ref PrebuildData data, BuildPreview preview) =>
                {
                    if (preview.inputObjId != 0)
                    {
                        data.parentId = preview.inputObjId;
                    }
                }));


            return matcher.InstructionEnumeration();
        }
    }
}