using System.Collections.Generic;
using System.IO;

namespace CommonAPI
{
    public class StarSystemManager
    {
        public static List<StarSystemStorage> systems = new List<StarSystemStorage>();
        public static TypeRegistry<IStarSystem, StarSystemStorage> registry = new TypeRegistry<IStarSystem, StarSystemStorage>();
        
        public static void InitOnLoad()
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
        
        public static void InitNewStar(StarData star)
        {
            for (int i = 1; i < registry.data.Count; i++)
            {
                StarSystemStorage storage = systems[i];
                storage.InitNewStar(star);
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

            registry.ImportAndMigrate(systems, r);
        }

        public static void Export(BinaryWriter w)
        {
            w.Write(0);
            
            registry.ExportContainer(systems, w);
        }
    }
}