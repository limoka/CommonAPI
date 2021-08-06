using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using UnityEngine;

// ReSharper disable InconsistentNaming

// ReSharper disable UnusedParameter.Global
// ReSharper disable RedundantAssignment

namespace CommonAPI
{
    
    [HarmonyPatch]
    static class EntityDataSetNullPatch
    {
        [HarmonyPatch(typeof(EntityData), "SetNull")]
        [HarmonyPostfix]
        public static void SetNull(EntityData __instance)
        {
            __instance.customId = 0;
            __instance.customType = -1;
            __instance.customData = null;
        }
    }

    [HarmonyPatch(typeof(PrebuildData), "SetNull")]
    static class PrebuildDataSetNullPatch
    {
        [HarmonyPostfix]
        public static void Postfix(PrebuildData __instance)
        {
            __instance.parentId = 0;
        }
    }

    [HarmonyPatch]
    static class PrefabDescCustomDataPatch
    {

        [HarmonyPatch(typeof(PrefabDesc), "Free")]
        [HarmonyPostfix]
        public static void Free(PrefabDesc __instance)
        {
            __instance.customData = null;
        }
    }

    [HarmonyPatch(typeof(AssemblerComponent), "SetEmpty")]
    static class AssemblerComponentSetEmptyPatch
    {
        [HarmonyPostfix]
        public static void Postfix(AssemblerComponent __instance)
        {
            __instance.isDisabled = false;
        }
    }

    //TODO add state displays to UI
    [HarmonyPatch(typeof(AssemblerComponent), "InternalUpdate")]
    static class AssemblerComponentUpdatePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref AssemblerComponent __instance, uint __result)
        {
            if (__instance.isDisabled)
            {
                __instance.replicating = false;
                __result = 0;
                return false;
            }

            return true;
        }

    }
    
    [HarmonyPatch(typeof(InserterComponent), "SetEmpty")]
    static class InserterComponentSetEmptyPatch
    {
        [HarmonyPostfix]
        public static void Postfix(InserterComponent __instance)
        {
            __instance.isDisabled = false;
        }
    }
    
    [HarmonyPatch(typeof(InserterComponent), "InternalUpdate")]
    static class InserterComponentUpdatePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(ref InserterComponent __instance)
        {
            return !__instance.isDisabled;
        }

    }
    
    [HarmonyPatch]
    static class CargoPathPatch
    {
        [HarmonyPatch(typeof(CargoPath), "Clear")]
        [HarmonyPostfix]
        public static void Clear(CargoPath __instance)
        {
            __instance.renderBegin = true;
            __instance.renderEnd = true;
        }
        
        [HarmonyPatch(typeof(CargoPath), MethodType.Constructor, new Type[] { typeof(CargoContainer) })]
        [HarmonyPostfix]
        public static void Constructor(CargoPath __instance)
        {
            __instance.renderBegin = true;
            __instance.renderEnd = true;
        }

    }
    
}