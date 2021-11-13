using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using CommonAPI.Systems;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CommonAPI.Patches
{
    [HarmonyPatch]
    public class UIRecipePickerPatch
    {
        private static UITabButton[] tabs;

        [HarmonyPatch(typeof(UIRecipePicker), "_OnCreate")]
        [HarmonyPostfix]
        public static void Create(UIRecipePicker __instance)
        {
            tabs = new UITabButton[TabSystem.tabsRegistry.idMap.Count];
            
            for (int i = 0; i < TabSystem.tabsRegistry.idMap.Count; i++)
            {
                TabData data = TabSystem.tabsRegistry.data[i + 3];
                GameObject buttonPrefab = TabSystem.resource.bundle.LoadAsset<GameObject>("Assets/CommonAPI/UI/tab-button.prefab");
                GameObject button = Object.Instantiate(buttonPrefab, __instance.pickerTrans, false);
                ((RectTransform)button.transform).anchoredPosition = new Vector2(156 + 70 * i, -75);
                UITabButton tabButton = button.GetComponent<UITabButton>();
                Sprite sprite = Resources.Load<Sprite>(data.tabIconPath);
                tabButton.Init(sprite, data.tabName, i + 3, __instance.OnTypeButtonClick);
                tabs[i] = tabButton;
            }
        }

        [HarmonyPatch(typeof(UIRecipePicker), "OnTypeButtonClick")]
        [HarmonyPostfix]
        public static void OnTypeClicked(int type)
        {
            foreach (UITabButton tab in tabs)
            {
                tab.TabSelected(type);
            }
        }
    }
}