using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using BepInEx;
using BepInEx.Logging;
using CommonAPI.Systems;
using crecheng.DSPModSave;
using HarmonyLib;
using NebulaAPI;

[module: UnverifiableCode]
#pragma warning disable 618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore 618

namespace CommonAPI
{
    /// <summary>
    /// Plugin class of Common API. Entry point
    /// </summary>
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency(NebulaModAPI.API_GUID)]
    public class CommonAPIPlugin : BaseUnityPlugin, IModCanSave, IMultiplayerMod
    {
        public const string ID = "CommonAPI";
        public const string GUID = "org.kremnev8.api." + ID;
        public const string NAME = "DSP Common API";
        
        public const string VERSION = "1.0.0";

        internal static HashSet<string> LoadedSubmodules;
        internal static Harmony harmony;
        internal static ManualLogSource logger;
        
        public static Dictionary<string, Registry> registries = new Dictionary<string, Registry>();
        public static readonly Version buildFor = GameVersionUtil.GetVersion(0, 8, 22, 9331);
        
        

        void Awake()
        {
            logger = Logger;

            UnityThread.initUnityThread();
            
            logger.LogInfo($"Current version: {GameConfig.gameVersion.ToFullString()}");
            
            harmony = new Harmony(GUID);
            
            var pluginScanner = new PluginScanner();
            var submoduleHandler = new APISubmoduleHandler(buildFor, Logger);
            LoadedSubmodules = submoduleHandler.LoadRequested(pluginScanner);
            pluginScanner.ScanPlugins();

            NebulaModAPI.RegisterPackets(Assembly.GetExecutingAssembly());

            LoadSaveOnLoad.Init();
            harmony.PatchAll(typeof(VFPreloadPatch));
            
            logger.LogInfo("Common API is initialized!");
        }
        
        internal static void CheckIfUsedOnRightGameVersion() {
            var buildId = GameConfig.gameVersion;

            if (buildFor == buildId)
                return;

            logger.LogWarning($"This version of CommonAPI was built for build id \"{buildFor}\", you are running \"{buildId}\".");
            logger.LogWarning("Should any problems arise, please check for a new version before reporting issues.");
        }
        
        /// <summary>
        /// Return true if the specified submodule is loaded.
        /// </summary>
        /// <param name="submodule">nameof the submodule</param>
        public static bool IsLoaded(string submodule) {
            if (LoadedSubmodules == null) {
                logger.LogWarning("IsLoaded called before submodules were loaded, result may not reflect actual load status.");
                return false;
            }
            return LoadedSubmodules.Contains(submodule);
        }

        public void Import(BinaryReader r)
        {
            
            r.ReadInt32();
            
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
            
            CustomStarSystem.InitOnLoad();
            CustomStarSystem.Import(r);

            CustomPlanetSystem.InitOnLoad();
            CustomPlanetSystem.Import(r);
        }

        public void Export(BinaryWriter w)
        {
            w.Write(0);

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
            
            CustomStarSystem.Export(w);
            CustomPlanetSystem.Export(w);
        }

        public void IntoOtherSave()
        {
            if (NebulaModAPI.IsMultiplayerActive && !NebulaModAPI.MultiplayerSession.LocalPlayer.IsHost)
            {
                foreach (var kv in registries)
                {
                    kv.Value.InitUnitMigrationMap();
                }
            }
            
            CustomStarSystem.InitOnLoad();
            CustomPlanetSystem.InitOnLoad();
        }

        public bool CheckVersion(string hostVersion, string clientVersion)
        {
            return hostVersion.Equals(clientVersion);
        }

        public string Version => VERSION;
    }

    [HarmonyPatch]
    public static class VFPreloadPatch
    {
        [HarmonyPatch(typeof(VFPreload), "InvokeOnLoadWorkEnded")]
        [HarmonyPostfix]
        public static void OnMainMenuOpen()
        {
            CommonAPIPlugin.CheckIfUsedOnRightGameVersion();
        }
    }
}