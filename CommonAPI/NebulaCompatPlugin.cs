using System;
using System.Reflection;
using BepInEx;
using CommonAPI.Nebula;
using CommonAPI.Systems;
using NebulaAPI;

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
            if (CommonAPIPlugin.IsSubmoduleLoaded(nameof(CustomPlanetSystem)))
            {
                NebulaModAPI.OnPlanetLoadRequest += planetId =>
                {
                    NebulaModAPI.MultiplayerSession.Network.SendPacket(new PlanetSystemLoadRequest(planetId));
                };
            }

            if (CommonAPIPlugin.IsSubmoduleLoaded(nameof(CustomStarSystem)))
            {
                NebulaModAPI.OnStarLoadRequest += starIndex =>
                {
                    NebulaModAPI.MultiplayerSession.Network.SendPacket(new StarSystemLoadRequest(starIndex));
                };
            }
            
            NebulaModAPI.RegisterPackets(Assembly.GetExecutingAssembly());
            CommonAPIPlugin.onIntoOtherSave = CheckNebulaInIntoOtherSave;
            CustomPlanetSystem.onInitNewPlanet = HandleNebulaPacket;
            
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
            if (!CustomPlanetSystem.pendingData.TryGetValue(planet.id, out byte[] bytes)) return;
            CustomPlanetSystem.pendingData.Remove(planet.id);
            
            using IReaderProvider p = NebulaModAPI.GetBinaryReader(bytes);

            for (int i = 1; i < CustomPlanetSystem.registry.data.Count; i++)
            {
                PlanetSystemStorage system = CustomPlanetSystem.systems[i];
                system.GetSystem(planet.factory).Import(p.BinaryReader);
            }
        }

        public bool CheckVersion(string hostVersion, string clientVersion)
        {
            return hostVersion.Equals(clientVersion);
        }

        public string Version => CommonAPIPlugin.VERSION;
    }
}