using System;
using System.Collections.Generic;
using System.IO;

namespace CommonAPI
{
    public static class CustomFactory
    {
        public static List<FactorySystemStorage> systems = new List<FactorySystemStorage>();
        public static TypeRegistry<IFactorySystem, FactorySystemStorage> systemRegistry = new TypeRegistry<IFactorySystem, FactorySystemStorage>();

        public static void InitOnLoad()
        {
            CommonAPIPlugin.logger.LogInfo("Loading custom factory!");
            GameData data = GameMain.data;
            
            systems.Clear();
            systems.Capacity = systemRegistry.data.Count + 1;
            systems.Add(null);
            for (int i = 1; i < systemRegistry.data.Count; i++)
            {
                FactorySystemStorage storage = new FactorySystemStorage();
                storage.InitOnLoad(data, i);
                systems.Add(storage);
            }
        }

        public static void InitNewPlanet(PlanetData planet)
        {
            for (int i = 1; i < systemRegistry.data.Count; i++)
            {
                FactorySystemStorage storage = systems[i];
                storage.InitNewPlanet(planet);
            }
        }
        

        public static void CreateEntityComponents(PlanetFactory factory, int entityId, PrefabDesc desc, int prebuildId)
        {
            for (int i = 1; i < systems.Count; i++)
            {
                FactorySystemStorage storage = systems[i];
                storage.GetSystem(factory).OnLogicComponentsAdd(entityId, desc, prebuildId);
            }
            
            for (int i = 1; i < systems.Count; i++)
            {
                FactorySystemStorage storage = systems[i];
                storage.GetSystem(factory).OnPostlogicComponentsAdd(entityId, desc, prebuildId);
            }
        }

        public static void RemoveEntityComponents(PlanetFactory factory, int entityId)
        {
            for (int i = 1; i < systems.Count; i++)
            {
                FactorySystemStorage storage = systems[i];
                storage.GetSystem(factory).OnLogicComponentsRemove(entityId);
            }
        }
        
        public static void PowerUpdate(PlanetFactory factory)
        {
            if (factory == null) return;
            
            for (int i = 1; i < systems.Count; i++)
            {
                FactorySystemStorage storage = systems[i];
                
                storage.PowerUpdate(factory);
            }
        }

        public static void PreUpdate(PlanetFactory factory)
        {
            if (factory == null) return;

            for (int i = 1; i < systems.Count; i++)
            {
                FactorySystemStorage storage = systems[i];
                storage.PreUpdate(factory);
            }
        }
        
        public static void Update(PlanetFactory factory)
        {
            if (factory == null) return;

            for (int i = 1; i < systems.Count; i++)
            {
                FactorySystemStorage storage = systems[i];
                storage.Update(factory);
            }
        }
        
        public static void PostUpdate(PlanetFactory factory)
        {
            if (factory == null) return;

            for (int i = 1; i < systems.Count; i++)
            {
                FactorySystemStorage storage = systems[i];
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
                    FactorySystemStorage storage = systems[j];
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
                    FactorySystemStorage storage = systems[j];
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
                    FactorySystemStorage storage = systems[j];
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
                    FactorySystemStorage storage = systems[j];
                    if (storage.PostUpdateSupportsMultithread()) return;

                    storage.PostUpdate(factory);
                }
            }
        }
        
        public static void PowerUpdateMultithread(PlanetFactory factory, int usedThreadCount, int currentThreadIdx, int minimumCount)
        {
            for (int i = 1; i < systems.Count; i++)
            {
                FactorySystemStorage storage = systems[i];
                storage.PowerUpdateMultithread(factory, usedThreadCount, currentThreadIdx, minimumCount);
            }
        }

        public static void PreUpdateMultithread(PlanetFactory factory, int usedThreadCount, int currentThreadIdx, int minimumCount)
        {
            for (int i = 1; i < systems.Count; i++)
            {
                FactorySystemStorage storage = systems[i];
                storage.PreUpdateMultithread(factory, usedThreadCount, currentThreadIdx, minimumCount);
            }
        }
        
        public static void UpdateMultithread(PlanetFactory factory, int usedThreadCount, int currentThreadIdx, int minimumCount)
        {
            for (int i = 1; i < systems.Count; i++)
            {
                FactorySystemStorage storage = systems[i];
                storage.UpdateMultithread(factory, usedThreadCount, currentThreadIdx, minimumCount);
            }
        }
        
        public static void PostUpdateMultithread(PlanetFactory factory, int usedThreadCount, int currentThreadIdx, int minimumCount)
        {
            for (int i = 1; i < systems.Count; i++)
            {
                FactorySystemStorage storage = systems[i];
                storage.PostUpdateMultithread(factory, usedThreadCount, currentThreadIdx, minimumCount);
            }
        }
        

        public static void Import(BinaryReader r)
        {
            int ver = r.ReadInt32();

            systemRegistry.ImportAndMigrate(systems, r);
        }

        public static void Export(BinaryWriter w)
        {
            w.Write(0);
            
            systemRegistry.ExportContainer(systems, w);
        }
    }
}