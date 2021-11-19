using System;
using System.Collections.Generic;
using System.IO;
using CommonAPI.Patches;

namespace CommonAPI.Systems
{
    [CommonAPISubmodule]
    public static class CustomStarSystem
    {
        public static List<StarSystemStorage> systems = new List<StarSystemStorage>();
        public static TypeRegistry<IStarSystem, StarSystemStorage> registry = new TypeRegistry<IStarSystem, StarSystemStorage>();

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
            CommonAPIPlugin.harmony.PatchAll(typeof(StarSystemHooks));
        }


        [CommonAPISubmoduleInit(Stage = InitStage.Load)]
        internal static void load()
        {
            CommonAPIPlugin.registries.Add($"{CommonAPIPlugin.ID}:StarSystemsRegistry", registry);
            
        }

        private static void ThrowIfNotLoaded()
        {
            if (!Loaded)
            {
                throw new InvalidOperationException(
                    $"{nameof(CustomStarSystem)} is not loaded. Please use [{nameof(CommonAPISubmoduleDependency)}(nameof({nameof(CustomStarSystem)})]");
            }
        }


        public static void InitOnLoad()
        {
            if (Loaded)
            {
                CommonAPIPlugin.logger.LogInfo("Loading star system manager");
                GameData data = GameMain.data;

                systems.Clear();
                systems.Capacity = registry.data.Count + 1;
                systems.Add(null);
                for (int i = 1; i < registry.data.Count; i++)
                {
                    StarSystemStorage storage = new StarSystemStorage();
                    storage.InitOnLoad(i);
                    systems.Add(storage);
                }
            }
        }

        public static void InitNewStar(StarData star)
        {
            for (int i = 1; i < registry.data.Count; i++)
            {
                StarSystemStorage storage = systems[i];
                storage.InitNewStar(star);
            }
        }

        public static T GetSystem<T>(int starId, int systemId) where T : IStarSystem
        {
            ThrowIfNotLoaded();
            StarData star = GameMain.galaxy.StarById(starId);
            return GetSystem<T>(star, systemId);
        }

        public static T GetSystem<T>(StarData star, int systemId) where T : IStarSystem
        {
            ThrowIfNotLoaded();
            if (systemId <= 0 || systemId >= systems.Count) return default;
            return (T) systems[systemId].GetSystem(star);
        }

        public static void DrawUpdate()
        {
            if (GameMain.data.localStar != null && DysonSphere.renderPlace == ERenderPlace.Universe)
            {
                StarData star = GameMain.data.localStar;
                if (star == null) return;

                for (int j = 1; j < systems.Count; j++)
                {
                    StarSystemStorage storage = systems[j];

                    storage.DrawUpdate(star);
                }
            }
        }

        public static void PreUpdate(StarData star)
        {
            if (star == null) return;

            for (int i = 1; i < systems.Count; i++)
            {
                StarSystemStorage storage = systems[i];
                storage.PreUpdate(star);
            }
        }

        public static void Update(StarData star)
        {
            if (star == null) return;

            for (int i = 1; i < systems.Count; i++)
            {
                StarSystemStorage storage = systems[i];
                storage.Update(star);
            }
        }

        public static void PreUpdateOnlySinglethread()
        {
            for (int i = 0; i < GameMain.galaxy.starCount; i++)
            {
                StarData star = GameMain.galaxy.stars[i];
                if (star == null) continue;

                for (int j = 1; j < systems.Count; j++)
                {
                    StarSystemStorage storage = systems[j];
                    if (storage.PreUpdateSupportsMultithread()) return;

                    storage.PreUpdate(star);
                }
            }
        }

        public static void UpdateOnlySinglethread()
        {
            for (int i = 0; i < GameMain.galaxy.starCount; i++)
            {
                StarData star = GameMain.galaxy.stars[i];
                if (star == null) continue;

                for (int j = 1; j < systems.Count; j++)
                {
                    StarSystemStorage storage = systems[j];
                    if (storage.UpdateSupportsMultithread()) return;

                    storage.Update(star);
                }
            }
        }

        public static void PreUpdateMultithread(StarData star, int usedThreadCount, int currentThreadIdx, int minimumCount)
        {
            for (int i = 1; i < systems.Count; i++)
            {
                StarSystemStorage storage = systems[i];
                storage.PreUpdateMultithread(star, usedThreadCount, currentThreadIdx, minimumCount);
            }
        }

        public static void UpdateMultithread(StarData star, int usedThreadCount, int currentThreadIdx, int minimumCount)
        {
            for (int i = 1; i < systems.Count; i++)
            {
                StarSystemStorage storage = systems[i];
                storage.UpdateMultithread(star, usedThreadCount, currentThreadIdx, minimumCount);
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