using System;
using CommonAPI.Systems;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CommonAPI.Patches
{
    //Loading custom resources
    [HarmonyPatch]
    static class ResourcesPatch
    {
        [HarmonyPatch(typeof(Resources), "Load", typeof(string), typeof(Type))]
        [HarmonyPrefix]
        public static bool Prefix(ref string path, Type systemTypeInstance, ref Object __result)
        {
            foreach (ResourceData resource in ProtoRegistry.modResources)
            {
                if (!path.Contains(resource.keyWord) || !resource.HasAssetBundle()) continue;

                if (resource.bundle.Contains(path + ".prefab") && systemTypeInstance == typeof(GameObject))
                {
                    Object myPrefab = resource.bundle.LoadAsset(path + ".prefab");
                    CommonAPIPlugin.logger.LogDebug($"Loading registered asset {path}: {(myPrefab != null ? "Success" : "Failure")}");

                    if (!ProtoRegistry.modelMats.ContainsKey(path))
                    {
                        __result = myPrefab;
                        return false;
                    }

                    LodMaterials mats = ProtoRegistry.modelMats[path];
                    if (myPrefab != null && mats.HasLod(0))
                    {
                        MeshRenderer[] renderers = ((GameObject) myPrefab).GetComponentsInChildren<MeshRenderer>();
                        foreach (MeshRenderer renderer in renderers)
                        {
                            Material[] newMats = new Material[renderer.sharedMaterials.Length];
                            for (int i = 0; i < newMats.Length; i++)
                            {
                                newMats[i] = mats[0][i];
                            }

                            renderer.sharedMaterials = newMats;
                        }
                    }

                    __result = myPrefab;
                    return false;
                }

                foreach (string extension in ProtoRegistry.spriteFileExtensions)
                {
                    if (!resource.bundle.Contains(path + extension)) continue;

                    Object mySprite = resource.bundle.LoadAsset(path + extension, systemTypeInstance);

                    CommonAPIPlugin.logger.LogDebug($"Loading registered asset {path}: {(mySprite != null ? "Success" : "Failure")}");

                    __result = mySprite;
                    return false;
                }
                
                foreach (string extension in ProtoRegistry.audioClipFileExtensions)
                {
                    if (!resource.bundle.Contains(path + extension)) continue;

                    Object myAudioClip = resource.bundle.LoadAsset(path + extension, systemTypeInstance);
                    
                    CommonAPIPlugin.logger.LogDebug($"Loading registered asset {path}: {(myAudioClip != null ? "Success" : "Failure")}");

                    __result = myAudioClip;
                    return false;
                }
            }

            return true;
        }
    }
}