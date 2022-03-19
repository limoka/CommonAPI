using System;
using System.IO;
using System.Reflection;
using CommonAPI.Patches;
using CommonAPI.Systems;
using UnityEngine;

namespace CommonAPI.Systems
{
    [CommonAPISubmodule(Dependencies = new []{typeof(ProtoRegistry)})]
    public static class TabSystem
    {
        internal static InstanceRegistry<TabData> tabsRegistry = new InstanceRegistry<TabData>(3);
        private static GameObject tabPrefab;
        
        /// <summary>
        /// Return true if the submodule is loaded.
        /// </summary>
        public static bool Loaded {
            get => _loaded;
            internal set => _loaded = value;
        }

        private static bool _loaded;


        [CommonAPISubmoduleInit(Stage = InitStage.SetHooks)]
        internal static void SetHooks()
        {
            CommonAPIPlugin.harmony.PatchAll(typeof(UIItemPickerPatch));
            CommonAPIPlugin.harmony.PatchAll(typeof(UIRecipePickerPatch));
            CommonAPIPlugin.harmony.PatchAll(typeof(UIReplicatorPatch));
        }

        [CommonAPISubmoduleInit(Stage = InitStage.Load)]
        internal static void Load()
        {
        }

        internal static void ThrowIfNotLoaded()
        {
            if (!Loaded)
            {
                throw new InvalidOperationException(
                    $"{nameof(TabSystem)} is not loaded. Please use [{nameof(CommonAPISubmoduleDependency)}(nameof({nameof(TabSystem)})]");
            }
        }

        public static int RegisterTab(string tabId, TabData tab)
        {
            ThrowIfNotLoaded();
            int tabIndex = tabsRegistry.Register(tabId, tab);
            tab.tabIndex = tabIndex;
            
            return tabIndex;
        }
        
        public static int GetTabId(string tabId)
        {
            ThrowIfNotLoaded();
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