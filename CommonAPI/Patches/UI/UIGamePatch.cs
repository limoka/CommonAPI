using HarmonyLib;
using CommonAPI;
// ReSharper disable InconsistentNaming

namespace CommonAPI
{
    [HarmonyPatch]
    public class UIGamePatch
    {

        [HarmonyPatch(typeof(UIGame), "_OnDestroy")]
        [HarmonyPostfix]
        public static void OnDestroy()
        {
            foreach (var window in UIRegistry.windows)
            {
                window._Destroy();
            }
        }

        [HarmonyPatch(typeof(UIGame), "_OnInit")]
        [HarmonyPostfix]
        public static void OnInit(UIGame __instance)
        {
            foreach (var window in UIRegistry.windows)
            {
                window._Init(__instance.gameData);
                window.Open(0);
            }
        }

        [HarmonyPatch(typeof(UIGame), "_OnFree")]
        [HarmonyPostfix]
        public static void OnFree()
        {
            foreach (var window in UIRegistry.windows)
            {
                window._Free();
            }
        }

        [HarmonyPatch(typeof(UIGame), "_OnUpdate")]
        [HarmonyPostfix]
        public static void OnUpdate()
        {
            foreach (var window in UIRegistry.windows)
            {
                window.OnUpdateUI();
            }
        }

        [HarmonyPatch(typeof(UIGame), "ShutInventoryConflictsWindows")]
        [HarmonyPatch(typeof(UIGame), "ShutAllFunctionWindow")]
        [HarmonyPostfix]
        public static void ShutAllFunctionWindow()
        {
            foreach (var window in UIRegistry.windows)
            {
                window.Close();
            }
        }

        [HarmonyPatch(typeof(UIGame), "OnPlayerInspecteeChange")]
        [HarmonyPostfix]
        public static void OnPlayerInspecteeChange(UIGame __instance, EObjectType objType, int objId)
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

            foreach (var window in UIRegistry.windows)
            {
                if (window.ShouldOpen(componentId, protoId))
                {
                    currentWindow = window;
                    break;
                }
            }

            if (currentWindow == null) return;

            if (objId > 0 && UIRegistry.customInspectId != objId)
            {
                if (currentWindow.DoCloseOtherWindows())
                    __instance.ShutAllFunctionWindow();

                if (currentWindow.DoClosePlayerInventory())
                    __instance.ShutPlayerInventory();

                currentWindow.Open(objId);
            }
            else if (objId == 0 && UIRegistry.customInspectId > 0)
            {
                currentWindow.Close();
            }

            UIRegistry.customInspectId = objId;
        }
    }
}