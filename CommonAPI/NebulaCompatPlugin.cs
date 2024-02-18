using System;
using System.Reflection;
using BepInEx;
using CommonAPI.Nebula;
using CommonAPI.Systems;
using NebulaAPI;
using NebulaAPI.Interfaces;

namespace CommonAPI
{
    [BepInPlugin(GUID, NAME, CommonAPIPlugin.VERSION)]
    [BepInDependency(NebulaModAPI.API_GUID)]
    [BepInDependency(CommonAPIPlugin.GUID)]
    public class NebulaCompatPlugin : BaseUnityPlugin, IMultiplayerMod
    {
        public const string ID = "common-api-nebula-compat";
        public const string GUID = "dsp.common-tools." + ID;
        public const string NAME = "Common API Nebula Compatibility";


        private void Start()
        {
            //Moved Custom Star and Planet Systems behavior here for easier compatibility with Nebula
            if (CommonAPIPlugin.IsSubmoduleLoaded(nameof(PlanetExtensionSystem)))
            {
                NebulaModAPI.OnPlanetLoadRequest += planetId =>
                {
                    NebulaModAPI.MultiplayerSession.Network.SendPacket(new PlanetSystemLoadRequest(planetId));
                };
            }

            if (CommonAPIPlugin.IsSubmoduleLoaded(nameof(StarExtensionSystem)))
            {
                NebulaModAPI.OnStarLoadRequest += starIndex =>
                {
                    NebulaModAPI.MultiplayerSession.Network.SendPacket(new StarExtensionLoadRequest(starIndex));
                };
            }
            
            NebulaModAPI.RegisterPackets(Assembly.GetExecutingAssembly());
            CommonAPIPlugin.onIntoOtherSave = CheckNebulaInIntoOtherSave;
            PlanetExtensionSystem.onInitNewPlanet = HandleNebulaPacket;
            
            Logger.LogInfo("Common API Nebula Compatibility ready!");
        }

        public static void CheckNebulaInIntoOtherSave()
        {
            if (NebulaModAPI.IsMultiplayerActive && !NebulaModAPI.MultiplayerSession.LocalPlayer.IsHost)
            {
                foreach (var kv in CommonAPIPlugin.registries)
                {
                    kv.Value.InitUnitMigrationMap();
                }
            }
        }

        public static void HandleNebulaPacket(PlanetData planet)
        {
            if (!NebulaModAPI.IsMultiplayerActive || NebulaModAPI.MultiplayerSession.LocalPlayer.IsHost) return;
            if (!PlanetExtensionSystem.pendingData.TryGetValue(planet.id, out byte[] bytes)) return;
            PlanetExtensionSystem.pendingData.Remove(planet.id);
            
            using IReaderProvider p = NebulaModAPI.GetBinaryReader(bytes);

            for (int i = 1; i < PlanetExtensionSystem.registry.data.Count; i++)
            {
                PlanetExtensionStorage extension = PlanetExtensionSystem.extensions[i];
                extension.GetExtension(planet.factory).Import(p.BinaryReader);
            }
        }

        public bool CheckVersion(string hostVersion, string clientVersion)
        {
            return hostVersion.Equals(clientVersion);
        }

        public string Version => CommonAPIPlugin.VERSION;
    }
}