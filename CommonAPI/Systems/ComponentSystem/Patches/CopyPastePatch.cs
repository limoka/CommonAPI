using System;
using System.IO;
using CommonAPI.Systems;
using HarmonyLib;
using UnityEngine;
// ReSharper disable Harmony003

// ReSharper disable InconsistentNaming

namespace CommonAPI.Patches
{
    [HarmonyPatch]
    public static class CopyPastePatch
    {
        [HarmonyPatch(typeof(BuildingParameters), "CopyFromFactoryObject")]
        [HarmonyPrefix]
        public static bool Copy(ref BuildingParameters __instance, int objectId, PlanetFactory factory, ref bool __result)
        {
            if (objectId <= 0)
            {
                return true;
            }

            int customId = factory.entityPool[objectId].customId;
            int customType = factory.entityPool[objectId].customType;
            if (customId == 0)
            {
                return true;
            }

            FactoryComponent component = ComponentExtension.GetComponent(factory,customType, customId);
            if (component is ICopyPasteSettings storage)
            {
                __instance.SetEmpty();
                __instance.type = BuildingType.Other;
                __instance.itemId = factory.entityPool[objectId].protoId;
                __instance.modelIndex = factory.entityPool[objectId].modelIndex;

                MemoryStream stream = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(stream);
                
                storage.WriteCopyData(writer);

                int byteLen = (int)stream.Length;
                int len = Mathf.CeilToInt(byteLen / 4f);
                
                if (len * 4 != byteLen)
                {
                    int addLen = len * 4 - byteLen;
                    for (int i = 0; i < addLen; i++)
                    {
                        writer.Write((byte)0);
                    }
                }
                
                __instance.parameters = new int[len + 2];
                __instance.parameters[0] = customId;
                __instance.parameters[1] = customType;

                byte[] data = stream.ToArray();
                
                for (int i = 0; i < len; i++)
                {
                    __instance.parameters[i+2] = BitConverter.ToInt32(data, i*4);
                }
                
                writer.Dispose();

                __result = true;
                return false;
            }

            return true;
        }
        
        //PasteToFactoryObject

        [HarmonyPatch(typeof(BuildingParameters), "PasteToFactoryObject")]
        [HarmonyPrefix]
        public static bool Paste(ref BuildingParameters __instance, int objectId, PlanetFactory factory, ref bool __result)
        {
            if (objectId <= 0 || __instance.type != BuildingType.Other || __instance.parameters == null) return true;
            if (__instance.parameters.Length <= 2) return true;
            
            int customId = __instance.parameters[0];
            int customType = __instance.parameters[1];
            if (customId <= 0) return true;
            
            int targetCustomId = factory.entityPool[objectId].customId;
            int targetCustomType = factory.entityPool[objectId].customType;
            if (targetCustomId <= 0) return true;
            
            FactoryComponent component = ComponentExtension.GetComponent(factory,targetCustomType, targetCustomId);
            if (component is ICopyPasteSettings storage)
            {
                int len = __instance.parameters.Length - 2;
                byte[] bytes = new byte[len * 4];
                for (int i = 0; i < len; i++)
                {
                    byte[] intBytes = BitConverter.GetBytes(__instance.parameters[i + 2]);
                    Array.Copy(intBytes, 0, bytes, i*4, 4);
                }
                MemoryStream stream = new MemoryStream(bytes);
                BinaryReader reader = new BinaryReader(stream);
                
                FactoryComponent originalObject = ComponentExtension.GetComponent(factory,customType, customId);
                
                if (storage.CanPasteSettings(originalObject, reader))
                {
                    stream.Position = 0;
                    storage.PasteSettings(customType, reader);
                    reader.Dispose();
                    __result = true;
                    return false;
                }
            }

            return true;
        }

        [HarmonyPatch(typeof(BuildingParameters), "CopiedTipText")]
        [HarmonyPrefix]
        public static bool CopyTipText(ref BuildingParameters __instance, ref string __result)
        {
            if (__instance.type == BuildingType.Other && __instance.parameters != null)
            {
                if (__instance.parameters.Length > 1)
                {
                    int customId = __instance.parameters[0];
                    int customType = __instance.parameters[1];
                    if (customId > 0)
                    {
                        PlanetFactory factory = GameMain.mainPlayer.factory;
                        FactoryComponent component = ComponentExtension.GetComponent(factory, customType, customId);
                        if (component is ICopyPasteSettings storage)
                        {
                            __result = storage.GetCopyMessage();
                            return false;
                        }
                    }
                }
            }
            
            return true;
        }
        
        [HarmonyPatch(typeof(BuildingParameters), "PastedTipText")]
        [HarmonyPrefix]
        public static bool PasteTipText(ref BuildingParameters __instance, ref string __result)
        {
            if (__instance.type == BuildingType.Other && __instance.parameters != null)
            {
                if (__instance.parameters.Length > 1)
                {
                    int customId = __instance.parameters[0];
                    int customType = __instance.parameters[1];
                    if (customId > 0)
                    {
                        PlanetFactory factory = GameMain.mainPlayer.factory;
                        
                        FactoryComponent component = ComponentExtension.GetComponent(factory, customType, customId);
                        if (component is ICopyPasteSettings storage)
                        {
                            __result = storage.GetPasteMessage();
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }
    
}