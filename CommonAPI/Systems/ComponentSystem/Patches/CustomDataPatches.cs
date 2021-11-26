using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using UnityEngine;

// ReSharper disable InconsistentNaming

// ReSharper disable UnusedParameter.Global
// ReSharper disable RedundantAssignment

namespace CommonAPI.Patches
{
    
    [HarmonyPatch]
    static class EntityDataSetNullPatch
    {
        [HarmonyPatch(typeof(EntityData), "SetNull")]
        [HarmonyPostfix]
        public static void SetNull(ref EntityData __instance)
        {
            __instance.customId = 0;
            __instance.customType = -1;
            __instance.customData = null;
        }
    }
}