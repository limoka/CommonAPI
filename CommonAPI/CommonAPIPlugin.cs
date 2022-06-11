using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using BepInEx;
using BepInEx.Logging;
using CommonAPI.Patches;
using CommonAPI.ShotScene;
using CommonAPI.Systems;
using crecheng.DSPModSave;
using HarmonyLib;
using UnityEngine;
using xiaoye97;

[module: UnverifiableCode]
#pragma warning disable 618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore 618

namespace CommonAPI
{
    /// <summary>
    /// Plugin class of Common API. Entry point
    /// </summary>
    [BepInPlugin(GUID, DISPNAME, VERSION)]
    [BepInDependency(LDBToolPlugin.MODGUID)]
    [BepInDependency(DSPModSavePlugin.MODGUID)]
    [BepInProcess("DSPGAME.exe")]
    public class CommonAPIPlugin : BaseUnityPlugin, IModCanSave
    {
        public const string ID = "CommonAPI";
        public const string GUID = "dsp.common-api.CommonAPI";
        public const string DISPNAME = "DSP Common API";

        public const string VERSION = ThisAssembly.AssemblyVersion;

        internal static HashSet<string> LoadedSubmodules;
        internal static Harmony harmony;
        internal static ICommonLogger logger;
        internal static ResourceData resource;
        internal static Action onIntoOtherSave;
        internal static APISubmoduleHandler submoduleHandler;
        
        public static Dictionary<string, Registry> registries = new Dictionary<string, Registry>();
        public static readonly Version buildFor = GameVersionUtil.GetVersion(0,9,26,12891);

        public static bool iconShotMenuEnabled;
        public static KeyCode openIconShotMenuButton;
        

        void Awake()
        {
            logger = new LoggerWrapper(Logger);
            CommonLogger.SetLogger(logger);

            UnityThread.initUnityThread();

            iconShotMenuEnabled = Config.Bind("General", "enableIconShotMenu", false, "Is Icon shot menu enabled. It is useful for mod developers, because it allows to create consistent icons.").Value;

            openIconShotMenuButton = Config.Bind("General", "OpenIconShotMenuButton", KeyCode.F6, "Button used to open special Icon shot menu. It is useful for mod developers, because it allows to create consistent icons.").Value;
            
            harmony = new Harmony(GUID);
            
            var pluginScanner = new PluginScanner();
            submoduleHandler = new APISubmoduleHandler(buildFor, Logger);
            LoadedSubmodules = submoduleHandler.LoadRequested(pluginScanner);
            pluginScanner.ScanPlugins();
            
            string pluginfolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            resource = new ResourceData(ID, "CommonAPI", pluginfolder);
            resource.LoadAssetBundle("commonapi");

            LoadSaveOnLoad.Init();
            harmony.PatchAll(typeof(VFPreloadPatch));
            
            logger.LogInfo("Common API is initialized!");
        }


        private void Update()
        {
            if (iconShotMenuEnabled && DSPGame.MenuDemoLoaded && Input.GetKeyDown(openIconShotMenuButton))
            {
                GeneratorSceneController.LoadIconGeneratorScene();
            }
        }

        internal static void CheckIfUsedOnRightGameVersion() {
            var buildId = GameConfig.gameVersion;

            if (buildFor == buildId)
                return;

            // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
            logger.LogWarning($"This version of CommonAPI was built for build id \"{buildFor.ToFullString()}\", you are running \"{buildId.ToFullString()}\".");
            logger.LogWarning("Should any problems arise, please check for a new version before reporting issues.");
        }
        
        /// <summary>
        /// Return true if the specified submodule is loaded.
        /// </summary>
        /// <param name="submodule">nameof the submodule</param>
        public static bool IsSubmoduleLoaded(string submodule) {
            if (LoadedSubmodules == null) {
                logger.LogWarning("IsLoaded called before submodules were loaded, result may not reflect actual load status.");
                return false;
            }
            return LoadedSubmodules.Contains(submodule);
        }

        /// <summary>
        /// Try load specified module manually. This is useful if you are using ScriptEngine and can't request using attributes.
        /// Do not use unless you can't make use of <see cref="CommonAPISubmoduleDependency"/>.
        /// </summary>
        /// <param name="moduleType">Type of needed module</param>
        /// <returns>Is loading successful?</returns>
        public static bool TryLoadModule(Type moduleType)
        {
            return submoduleHandler.RequestModuleLoad(moduleType);
        }

        public void Import(BinaryReader r)
        {
            
            int ver = r.ReadInt32();
            
            while (true)
            {
                if (r.ReadByte() == 0) break;

                string key = r.ReadString();

                if (registries.ContainsKey(key))
                {
                    r.ReadInt64();
                    registries[key].Import(r);
                }
                else
                {
                    long len = r.ReadInt64();
                    r.ReadBytes((int)len);
                }
            }
            
            StarExtensionSystem.InitOnLoad();
            StarExtensionSystem.Import(r);

            PlanetExtensionSystem.InitOnLoad();
            PlanetExtensionSystem.Import(r);

            if (ver > 0)
            {
                ModProtoHistory.Import(r);
                
            }
        }

        public void Export(BinaryWriter w)
        {
            w.Write(1);

            foreach (var kv in registries)
            {
                w.Write((byte)1);
                w.Write(kv.Key);
                MemoryStream stream = new MemoryStream();
                BinaryWriter tw = new BinaryWriter(stream);
                kv.Value.Export(tw);
                w.Write(stream.Length);
                w.Write(stream.ToArray());
            }

            w.Write((byte)0);
            
            StarExtensionSystem.Export(w);
            PlanetExtensionSystem.Export(w);
            ModProtoHistory.Export(w);
        }

        public void IntoOtherSave()
        {
            onIntoOtherSave?.Invoke();

            StarExtensionSystem.InitOnLoad();
            PlanetExtensionSystem.InitOnLoad();
        }
    }

    [HarmonyPatch]
    public static class VFPreloadPatch
    {
        [HarmonyPatch(typeof(VFPreload), "InvokeOnLoadWorkEnded")]
        [HarmonyPostfix]
        public static void OnMainMenuOpen()
        {
            CommonAPIPlugin.CheckIfUsedOnRightGameVersion();
            LoadSaveOnLoad.LoadSave();
        }
    }
}