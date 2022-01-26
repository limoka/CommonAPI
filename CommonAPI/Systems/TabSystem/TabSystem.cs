using System;
using System.IO;
using System.Reflection;
using CommonAPI.Patches;
using CommonAPI.Systems;

namespace CommonAPI.Systems
{
    [CommonAPISubmodule(Dependencies = new []{typeof(ProtoRegistry)})]
    public class TabSystem
    {
        internal static InstanceRegistry<TabData> tabsRegistry = new InstanceRegistry<TabData>(3);

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
            return tabsRegistry.Register(tabId, tab);
        }
        
        public int GetTabId(string tabId)
        {
            ThrowIfNotLoaded();
            return tabsRegistry.GetUniqueId(tabId);
        }
    }
}