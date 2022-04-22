using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace CommonAPI.Patches
{
    [HarmonyPatch]
    public static class ProtoSet_Patch
    {
        public static bool isEnabled = true;

        private static Sprite icon;
        private static ItemProto missingItem;
        
        static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(ProtoSet<>).MakeGenericType(typeof(ItemProto)), "Select");
        }

        [HarmonyPostfix]
        public static void ChangeSelectDefault(object __instance, int id, ref object __result)
        {
            if (!isEnabled) return;
            if (id <= 0) return;
            
            if (__instance is ProtoSet<ItemProto>)
            {
                if (__result == null)
                {
                    if (icon == null)
                    {
                        icon = CommonAPIPlugin.resource.bundle.LoadAsset<Sprite>("Assets/CommonAPI/Textures/Icons/missing-icon.png");
                    }

                    if (missingItem == null)
                    {
                        missingItem = new ItemProto()
                        {
                            ID = id,
                            Name = "Unknown Item",
                            name = "Unknown Item",
                            Type = EItemType.Material,
                            StackSize = 500,
                            FuelType = 0,
                            IconPath = "",
                            Description = "Unknown Item",
                            description = "Unknown Item",
                            produceFrom = "None",
                            GridIndex = 0,
                            DescFields = Array.Empty<int>(),
                            prefabDesc = PrefabDesc.none,
                            Upgrades = Array.Empty<int>(),
                            recipes = new List<RecipeProto>(),
                            handcrafts = new List<RecipeProto>(),
                            makes = new List<RecipeProto>(),
                            rawMats = new List<IDCNTINC>(),
                            _iconSprite = icon
                        };
                    }

                    __result = missingItem;
                }
            }
        }
    }
}