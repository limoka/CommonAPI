using System;
using System.IO;
using System.Reflection;
using CommonAPI.Patches;
using CommonAPI.Systems;
using UnityEngine;

namespace CommonAPI.Systems
{
    public class TabSystem : BaseSubmodule
    {
        internal static InstanceRegistry<TabData> tabsRegistry = new InstanceRegistry<TabData>(3);
        private static GameObject tabPrefab;

        internal static TabSystem Instance => CommonAPIPlugin.GetModuleInstance<TabSystem>();
        internal override Type[] Dependencies => new[] { typeof(ProtoRegistry) };

        internal override void SetHooks()
        {
            CommonAPIPlugin.harmony.PatchAll(typeof(UIItemPickerPatch));
            CommonAPIPlugin.harmony.PatchAll(typeof(UIRecipePickerPatch));
            CommonAPIPlugin.harmony.PatchAll(typeof(UIReplicatorPatch));
        }


        public static int RegisterTab(string tabId, TabData tab)
        {
            Instance.ThrowIfNotLoaded();
            int tabIndex = tabsRegistry.Register(tabId, tab);
            tab.tabIndex = tabIndex;
            
            return tabIndex;
        }
        
        public static int GetTabId(string tabId)
        {
            Instance.ThrowIfNotLoaded();
            return tabsRegistry.GetUniqueId(tabId);
        }

        public static TabData[] GetAllTabs()
        {
            return tabsRegistry.data.ToArray();
        }

        public static GameObject GetTabPrefab()
        {
            if (tabPrefab == null)
            {
                tabPrefab = CommonAPIPlugin.resource.bundle.LoadAsset<GameObject>("Assets/CommonAPI/UI/tab-button.prefab");
            }

            return tabPrefab;
        }
    }
}