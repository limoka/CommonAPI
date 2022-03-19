using System.Collections.Generic;
using CommonAPI.Systems;
using HarmonyLib;
using UnityEngine;

namespace CommonAPI.Patches
{
    [HarmonyPatch]
    public class UIItemPickerPatch
    {
        private static List<UITabButton> tabs;

        [HarmonyPatch(typeof(UIItemPicker), "_OnCreate")]
        [HarmonyPostfix]
        public static void Create(UIItemPicker __instance)
        {
            var datas = TabSystem.GetAllTabs();
            tabs = new List<UITabButton>(datas.Length - 3);

            foreach (TabData tab in datas)
            {
                if (tab == null) continue;

                GameObject button = Object.Instantiate(TabSystem.GetTabPrefab(), __instance.pickerTrans, false);
                ((RectTransform)button.transform).anchoredPosition = new Vector2(70 * tab.tabIndex - 54, -75);
                UITabButton tabButton = button.GetComponent<UITabButton>();
                Sprite sprite = Resources.Load<Sprite>(tab.tabIconPath);
                tabButton.Init(sprite, tab.tabName, tab.tabIndex, __instance.OnTypeButtonClick);
                tabs.Add(tabButton);
            }
        }

        [HarmonyPatch(typeof(UIItemPicker), "OnTypeButtonClick")]
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