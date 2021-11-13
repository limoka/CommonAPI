using System;
using System.Collections.Generic;
using System.IO;
using CommonAPI.Nebula;
using CommonAPI.Patches;
using NebulaAPI;

namespace CommonAPI.Systems
{
    [CommonAPISubmodule]
    public static class CustomPlanetSystem
    {
        public static List<PlanetSystemStorage> systems = new List<PlanetSystemStorage>();
        public static TypeRegistry<IPlanetSystem, PlanetSystemStorage> registry = new TypeRegistry<IPlanetSystem, PlanetSystemStorage>();

        internal static Dictionary<int, byte[]> pendingData = new Dictionary<int, byte[]>();

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
            CommonAPIPlugin.harmony.PatchAll(typeof(PlanetSystemHooks));
        }


        [CommonAPISubmoduleInit(Stage = InitStage.Load)]
        internal static void load()
        {
            CommonAPIPlugin.registries.Add($"{CommonAPIPlugin.ID}:PlanetSystemsRegistry", CustomPlanetSystem.registry);
            registry.Register(ComponentSystem.systemID, typeof(ComponentSystem));
            NebulaModAPI.OnPlanetLoadRequest += planetId =>
            {
                NebulaModAPI.MultiplayerSession.Network.SendPacket(new PlanetSystemLoadRequest(planetId));
            };
        }
        
        internal static void ThrowIfNotLoaded()
        {
            if (!Loaded)
            {
                throw new InvalidOperationException(
                    $"{nameof(CustomPlanetSystem)} is not loaded. Please use [{nameof(CommonAPISubmoduleDependency)}(nameof({nameof(CustomPlanetSystem)})]");
            }
        }


        public static void InitOnLoad()
        {
            if (Loaded)
            {
                CommonAPIPlugin.logger.LogInfo("Loading planet system manager");
                GameData data = GameMain.data;

                systems.Clear();
                systems.Capacity = registry.data.Count + 1;
                systems.Add(null);
                for (int i = 1; i < registry.data.Count; i++)
                {
                    PlanetSystemStorage storage = new PlanetSystemStorage();
                    storage.InitOnLoad(data, i);
                    systems.Add(storage);
                }
            }
        }

        public static void InitNewPlanet(PlanetData planet)
        {
            for (int i = 1; i < registry.data.Count; i++)
            {
                PlanetSystemStorage storage = systems[i];
                storage.InitNewPlanet(planet);
            }

            if (!NebulaModAPI.IsMultiplayerActive || NebulaModAPI.MultiplayerSession.LocalPlayer.IsHost) return;
            if (!pendingData.TryGetValue(planet.id, out byte[] bytes)) return;
            pendingData.Remove(planet.id);
            
            using IReaderProvider p = NebulaModAPI.GetBinaryReader(bytes);

            for (int i = 1; i < registry.data.Count; i++)
            {
                PlanetSystemStorage system = systems[i];
                system.GetSystem(planet.factory).Import(p.BinaryReader);
            }
        }


        public static void CreateEntityComponents(PlanetFactory factory, int entityId, PrefabDesc desc, int prebuildId)
        {
            for (int i = 1; i < systems.Count; i++)
            {
                PlanetSystemStorage storage = systems[i];
                IPlanetSystem system = storage.GetSystem(factory);
                if (system is IComponentStateListener listener)
                {
                    listener.OnLogicComponentsAdd(entityId, desc, prebuildId);
                }
            }

            for (int i = 1; i < systems.Count; i++)
            {
                PlanetSystemStorage storage = systems[i];
                IPlanetSystem system = storage.GetSystem(factory);
                if (system is IComponentStateListener listener)
                {
                    listener.OnPostlogicComponentsAdd(entityId, desc, prebuildId);
                }
            }
        }

        public static void RemoveEntityComponents(PlanetFactory factory, int entityId)
        {
            for (int i = 1; i < systems.Count; i++)
            {
                PlanetSystemStorage storage = systems[i];
                IPlanetSystem system = storage.GetSystem(factory);
                if (system is IComponentStateListener listener)
                {
                    listener.OnLogicComponentsRemove(entityId);
                }
            }
        }

        public static void DrawUpdate(PlanetFactory factory)
        {
            if (factory == null) return;

            for (int i = 1; i < systems.Count; i++)
            {
                PlanetSystemStorage storage = systems[i];

                storage.DrawUpdate(factory);
            }
        }

        public static void PowerUpdate(PlanetFactory factory)
        {
            if (factory == null) return;

            for (int i = 1; i < systems.Count; i++)
            {
                PlanetSystemStorage storage = systems[i];

                storage.PowerUpdate(factory);
            }
        }

        public static void PreUpdate(PlanetFactory factory)
        {
            if (factory == null) return;

            for (int i = 1; i < systems.Count; i++)
            {
                PlanetSystemStorage storage = systems[i];
                storage.PreUpdate(factory);
            }
        }

        public static void Update(PlanetFactory factory)
        {
            if (factory == null) return;

            for (int i = 1; i < systems.Count; i++)
            {
                PlanetSystemStorage storage = systems[i];
                storage.Update(factory);
            }
        }

        public static void PostUpdate(PlanetFactory factory)
        {
            if (factory == null) return;

            for (int i = 1; i < systems.Count; i++)
            {
                PlanetSystemStorage storage = systems[i];
                storage.PostUpdate(factory);
            }
        }

        public static void PowerUpdateOnlySinglethread(GameData data)
        {
            for (int i = 0; i < data.factoryCount; i++)
            {
                PlanetFactory factory = data.factories[i];
                if (factory == null) continue;

                for (int j = 1; j < systems.Count; j++)
                {
                    PlanetSystemStorage storage = systems[j];
                    if (storage.PowerUpdateSupportsMultithread()) return;

                    storage.PowerUpdate(factory);
                }
            }
        }

        public static void PreUpdateOnlySinglethread(GameData data)
        {
            for (int i = 0; i < data.factoryCount; i++)
            {
                PlanetFactory factory = data.factories[i];
                if (factory == null) continue;

                for (int j = 1; j < systems.Count; j++)
                {
                    PlanetSystemStorage storage = systems[j];
                    if (storage.PreUpdateSupportsMultithread()) return;

                    storage.PreUpdate(factory);
                }
            }
        }

        public static void UpdateOnlySinglethread(GameData data)
        {
            for (int i = 0; i < data.factoryCount; i++)
            {
                PlanetFactory factory = data.factories[i];
                if (factory == null) continue;

                for (int j = 1; j < systems.Count; j++)
                {
                    PlanetSystemStorage storage = systems[j];
                    if (storage.UpdateSupportsMultithread()) return;

                    storage.Update(factory);
                }
            }
        }

        public static void PostUpdateOnlySinglethread(GameData data)
        {
            for (int i = 0; i < data.factoryCount; i++)
            {
                PlanetFactory factory = data.factories[i];
                if (factory == null) continue;


                for (int j = 1; j < systems.Count; j++)
                {
                    PlanetSystemStorage storage = systems[j];
                    if (storage.PostUpdateSupportsMultithread()) return;

                    storage.PostUpdate(factory);
                }
            }
        }

        public static void PowerUpdateMultithread(PlanetFactory factory, int usedThreadCount, int currentThreadIdx, int minimumCount)
        {
            for (int i = 1; i < systems.Count; i++)
            {
                PlanetSystemStorage storage = systems[i];
                storage.PowerUpdateMultithread(factory, usedThreadCount, currentThreadIdx, minimumCount);
            }
        }

        public static void PreUpdateMultithread(PlanetFactory factory, int usedThreadCount, int currentThreadIdx, int minimumCount)
        {
            for (int i = 1; i < systems.Count; i++)
            {
                PlanetSystemStorage storage = systems[i];
                storage.PreUpdateMultithread(factory, usedThreadCount, currentThreadIdx, minimumCount);
            }
        }

        public static void UpdateMultithread(PlanetFactory factory, int usedThreadCount, int currentThreadIdx, int minimumCount)
        {
            for (int i = 1; i < systems.Count; i++)
            {
                PlanetSystemStorage storage = systems[i];
                storage.UpdateMultithread(factory, usedThreadCount, currentThreadIdx, minimumCount);
            }
        }

        public static void PostUpdateMultithread(PlanetFactory factory, int usedThreadCount, int currentThreadIdx, int minimumCount)
        {
            for (int i = 1; i < systems.Count; i++)
            {
                PlanetSystemStorage storage = systems[i];
                storage.PostUpdateMultithread(factory, usedThreadCount, currentThreadIdx, minimumCount);
            }
        }


        public static void Import(BinaryReader r)
        {
            int ver = r.ReadInt32();
            bool wasLoaded = r.ReadBoolean();

            if (wasLoaded)
            {
                registry.ImportAndMigrate(systems, r);
            }
        }

        public static void Export(BinaryWriter w)
        {
            w.Write(0);
            w.Write(Loaded);

            if (Loaded)
            {
                registry.ExportContainer(systems, w);
            }
        }
    }
}