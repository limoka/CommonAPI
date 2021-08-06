using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CommonAPI
{
    [HarmonyPatch]
    public static class UIRegistry
    {
        public static List<CustomMachineWindow> windows = new List<CustomMachineWindow>();
        public static List<string> registeredPrefabs = new List<string>();
        
        public static int customInspectId;

        public static void RegisterWindow(string prefabPath)
        {
            if (!registeredPrefabs.Contains(prefabPath))
            {
                registeredPrefabs.Add(prefabPath);
                CommonAPIPlugin.logger.LogDebug("Registering machine window, prefab: " + prefabPath);
            }
        }
        
        [HarmonyPatch(typeof(UIGame), "_OnCreate")]
        [HarmonyPostfix]
        // ReSharper disable once InconsistentNaming
        public static void OnCreate(UIGame __instance)
        {
            CommonAPIPlugin.logger.LogInfo("Loading custom UI's");
            Transform windowsObject = __instance.canvasGroup.transform.Find("Windows");
            if (windowsObject == null) return;

            foreach (var path in registeredPrefabs)
            {
                GameObject windowPrefab = Resources.Load<GameObject>(path);
                if (windowPrefab == null)
                {
                    CommonAPIPlugin.logger.LogError($"Error loading UI prefab: {path}!");
                    continue;
                }
                GameObject windowObject = Object.Instantiate(windowPrefab, windowsObject, false);

                CustomMachineWindow machineWindow = windowObject.GetComponent<CustomMachineWindow>();
                machineWindow._Create();
                windows.Add(machineWindow);
            }

            registeredPrefabs.Clear();
        }
    }
}