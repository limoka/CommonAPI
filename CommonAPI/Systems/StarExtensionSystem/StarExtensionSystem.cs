using System;
using System.Collections.Generic;
using System.IO;
using CommonAPI.Patches;

namespace CommonAPI.Systems
{
    public class StarExtensionSystem : BaseSubmodule
    {
        public static List<StarExtensionStorage> extensions = new List<StarExtensionStorage>();
        public static TypeRegistry<IStarExtension, StarExtensionStorage> registry = new TypeRegistry<IStarExtension, StarExtensionStorage>();

        internal static StarExtensionSystem Instance => CommonAPIPlugin.GetModuleInstance<StarExtensionSystem>();
        
        internal override void SetHooks()
        {
            CommonAPIPlugin.harmony.PatchAll(typeof(StarExtensionHooks));
        }

        
        internal override void Load()
        {
            CommonAPIPlugin.registries.Add($"{CommonAPIPlugin.ID}:StarSystemsRegistry", registry);
            
        }

        public static void InitOnLoad()
        {
            if (Instance.Loaded)
            {
                CommonAPIPlugin.logger.LogInfo("Loading star extension system");
                GameData data = GameMain.data;

                extensions.Clear();
                extensions.Capacity = registry.data.Count + 1;
                extensions.Add(null);
                CommonAPIPlugin.logger.LogInfo($"Got {registry.data.Count} entries!");
                for (int i = 1; i < registry.data.Count; i++)
                {
                    
                    StarExtensionStorage storage = new StarExtensionStorage();
                    storage.InitOnLoad(i);
                    extensions.Add(storage);
                }
            }
        }

        public static void InitNewStar(StarData star)
        {
            for (int i = 1; i < registry.data.Count; i++)
            {
                StarExtensionStorage storage = extensions[i];
                storage.InitNewStar(star);
            }
        }

        public static T GetExtension<T>(int starId, int systemId) where T : IStarExtension
        {
            Instance.ThrowIfNotLoaded();
            StarData star = GameMain.galaxy.StarById(starId);
            return GetExtension<T>(star, systemId);
        }

        public static T GetExtension<T>(StarData star, int systemId) where T : IStarExtension
        {
            Instance.ThrowIfNotLoaded();
            if (systemId <= 0 || systemId >= extensions.Count) return default;
            return (T) extensions[systemId].GetSystem(star);
        }

        public static void DrawUpdate()
        {
            if (GameMain.data.localStar != null && DysonSphere.renderPlace == ERenderPlace.Universe)
            {
                StarData star = GameMain.data.localStar;
                if (star == null) return;

                for (int j = 1; j < extensions.Count; j++)
                {
                    StarExtensionStorage storage = extensions[j];

                    storage.DrawUpdate(star);
                }
            }
        }

        public static void PreUpdate(StarData star)
        {
            if (star == null) return;

            for (int i = 1; i < extensions.Count; i++)
            {
                StarExtensionStorage storage = extensions[i];
                storage.PreUpdate(star);
            }
        }

        public static void Update(StarData star)
        {
            if (star == null) return;

            for (int i = 1; i < extensions.Count; i++)
            {
                StarExtensionStorage storage = extensions[i];
                storage.Update(star);
            }
        }

        public static void PreUpdateOnlySinglethread()
        {
            for (int i = 0; i < GameMain.galaxy.starCount; i++)
            {
                StarData star = GameMain.galaxy.stars[i];
                if (star == null) continue;

                for (int j = 1; j < extensions.Count; j++)
                {
                    StarExtensionStorage storage = extensions[j];
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

                for (int j = 1; j < extensions.Count; j++)
                {
                    StarExtensionStorage storage = extensions[j];
                    if (storage.UpdateSupportsMultithread()) return;

                    storage.Update(star);
                }
            }
        }

        public static void PreUpdateMultithread(StarData star, int usedThreadCount, int currentThreadIdx, int minimumCount)
        {
            for (int i = 1; i < extensions.Count; i++)
            {
                StarExtensionStorage storage = extensions[i];
                storage.PreUpdateMultithread(star, usedThreadCount, currentThreadIdx, minimumCount);
            }
        }

        public static void UpdateMultithread(StarData star, int usedThreadCount, int currentThreadIdx, int minimumCount)
        {
            for (int i = 1; i < extensions.Count; i++)
            {
                StarExtensionStorage storage = extensions[i];
                storage.UpdateMultithread(star, usedThreadCount, currentThreadIdx, minimumCount);
            }
        }


        public static void Import(BinaryReader r)
        {
            int ver = r.ReadInt32();
            bool wasLoaded = r.ReadBoolean();

            if (wasLoaded)
            {
                registry.ImportAndMigrate(extensions, r);
            }
        }

        public static void Export(BinaryWriter w)
        {
            w.Write(0);
            w.Write(Instance.Loaded);

            if (Instance.Loaded)
            {
                registry.ExportContainer(extensions, w);
            }
        }
    }
}