using System;
using HarmonyLib;
using UnityEngine;
using UnityEngine.PostProcessing;

namespace CommonAPI.ShotScene.Patches
{
    [HarmonyPatch]
    public static class PostProcessingMaterialFactoryPatch
    {
        public static Material modifiedMaterial;
        
        [HarmonyPatch(typeof(MaterialFactory), "Get")]
        [HarmonyPostfix]
        public static void ChangeShader(MaterialFactory __instance, string shaderName, ref Material __result)
        {
            if (!shaderName.Equals("Hidden/Post FX/Uber Shader")) return;
            if (!GeneratorSceneController.isShotSceneLoaded) return;
            
            if (modifiedMaterial == null)
            {
                Shader shader = (Shader)CommonAPIPlugin.resource.bundle.LoadAsset("Assets/CommonAPI/PostProcessingV1/Shaders/Uber.shader");
                modifiedMaterial = new Material(shader)
                {
                    name = $"PostFX - {shaderName.Substring(shaderName.LastIndexOf("/", StringComparison.Ordinal) + 1)}",
                    hideFlags = HideFlags.DontSave
                };
            }
            __result = modifiedMaterial;
        }
    }
}