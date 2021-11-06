using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace CommonAPI
{
    [HarmonyPatch]
    public static class UISystem
    {
        public static List<CustomMachineWindow> windows = new List<CustomMachineWindow>();
        public static List<string> registeredPrefabs = new List<string>();
        
        public static int customInspectId;
        public static CustomMachineWindow openWindow;

        public static void RegisterWindow(string prefabPath)
        {
            if (!registeredPrefabs.Contains(prefabPath))
            {
                registeredPrefabs.Add(prefabPath);
                CommonAPIPlugin.logger.LogDebug("Registering machine window, prefab: " + prefabPath);
            }
        }

        public static void RefreshOpenWindow()
        {
            if (openWindow != null)
            {
                openWindow.OnIdChange();
            }
        }
        
        internal static void OnCreate(UIGame uiGame)
        {
            CommonAPIPlugin.logger.LogInfo("Loading custom UI's");
            Transform windowsObject = uiGame.canvasGroup.transform.Find("Windows");
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
        
        internal static void OnPlayerInspecteeChange(UIGame uiGame, EObjectType objType, int objId)
        {
            PlanetFactory factory = GameMain.mainPlayer.factory;
            int componentId = -1;
            int protoId = 0;
            if (factory != null && objType == EObjectType.Entity && objId > 0)
            {
                componentId = factory.entityPool[objId].customType;
                protoId = factory.entityPool[objId].protoId;
            }

            CustomMachineWindow currentWindow = null;

            foreach (var window in windows)
            {
                if (window.ShouldOpen(componentId, protoId))
                {
                    currentWindow = window;
                    break;
                }
            }

            if (currentWindow == null) return;

            if (objId > 0 && customInspectId != objId)
            {
                if (currentWindow.DoCloseOtherWindows())
                    uiGame.ShutAllFunctionWindow();

                if (currentWindow.DoClosePlayerInventory())
                    uiGame.ShutPlayerInventory();

                currentWindow.Open(objId);
            }
            else if (objId == 0 && customInspectId > 0)
            {
                currentWindow.Close();
            }

            customInspectId = objId;
        }
    }
}