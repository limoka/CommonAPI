using HarmonyLib;
using UnityEngine;

namespace CommonAPI
{
    [HarmonyPatch]
    public class UIItemPickerPatch
    {
        private static UITabButton[] tabs;

        [HarmonyPatch(typeof(UIItemPicker), "_OnCreate")]
        [HarmonyPostfix]
        public static void Create(UIItemPicker __instance)
        {
            tabs = new UITabButton[TabData.tabsRegistry.idMap.Count];
            
            for (int i = 0; i < TabData.tabsRegistry.idMap.Count; i++)
            {
                TabData data = TabData.tabsRegistry.data[i + 3];
                GameObject buttonPrefab = CommonAPIPlugin.resource.bundle.LoadAsset<GameObject>("Assets/CommonAPI/UI/tab-button.prefab");
                GameObject button = Object.Instantiate(buttonPrefab, __instance.pickerTrans, false);
                ((RectTransform)button.transform).anchoredPosition = new Vector2(156 + 70 * i, -75);
                UITabButton tabButton = button.GetComponent<UITabButton>();
                Sprite sprite = Resources.Load<Sprite>(data.tabIconPath);
                tabButton.Init(sprite, data.tabName, i + 3, __instance.OnTypeButtonClick);
                tabs[i] = tabButton;
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