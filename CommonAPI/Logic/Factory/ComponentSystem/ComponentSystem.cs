using System.Collections.Generic;
using System.IO;

namespace CommonAPI
{
    public class ComponentSystem : IUpdateMultithread, IPowerUpdateMultithread
    {
        public static readonly string systemID = $"{CommonAPIPlugin.ID}:ComponentSystem";
        
        private static int _cachedId;
        public static int cachedId
        {
            get
            {
                if (_cachedId == 0)
                    _cachedId = CustomFactory.systemRegistry.GetUniqueId(systemID);
                
                return _cachedId;
            }
        }
        
        public static Registry<FactoryComponent, ComponentTypePool> componentRegistry = new Registry<FactoryComponent, ComponentTypePool>();
        
        private PlanetFactory factory;

        public List<ComponentTypePool> pools = new List<ComponentTypePool>();
        
        
        
        public void Init(PlanetFactory factory)
        {
            this.factory = factory;
            pools.Capacity = componentRegistry.data.Count + 1;
            pools.Add(null);
            for (int i = 1; i < componentRegistry.data.Count; i++)
            {
                ComponentTypePool pool = new ComponentTypePool(factory, i);
                pools.Add(pool);
                
                pool.Init(256);
            }
        }

        public void OnLogicComponentsAdd(int entityId, PrefabDesc desc, int prebuildId)
        {
            int typeId = desc.GetProperty<int>(ComponentDesc.FIELD_NAME);
            if (typeId != 0)
            {
                PrebuildData data = default;
                if (prebuildId > 0 && factory.prebuildPool[prebuildId].id == prebuildId)
                {
                    data = factory.prebuildPool[prebuildId];
                }

                GetPool(typeId).AddComponent(entityId, data);
            }
        }

        public void OnPostlogicComponentsAdd(int entityId, PrefabDesc desc, int prebuildId)
        {
            int typeId = desc.GetProperty<int>(ComponentDesc.FIELD_NAME);
            if (typeId != 0)
            {
                GetPool(typeId).OnPostComponentAdded();
            }
        }

        public void OnLogicComponentsRemove(int id)
        {
            if (id == 0 || factory.entityPool[id].id == 0) return;
            
            int cType = factory.entityPool[id].customType;
            int cId = factory.entityPool[id].customId;
            if (cId != 0)
            {
                GetPool(cType).RemovePoolItem(cId);
            }
        }

        public static FactoryComponent GetComponent(PlanetFactory factory, int typeId, int customId)
        {
            if (customId == 0 || typeId == 0) return null;
            
            ComponentSystem system = factory.GetSystem<ComponentSystem>(cachedId);

            return system.GetPool(typeId).pool[customId];
        }

        public void Import(BinaryReader r)
        {
            int ver = r.ReadInt32();

            CommonAPIPlugin.logger.LogInfo("Start Component System Import");
            
            componentRegistry.ImportAndMigrate(pools, r);
            
            for (int j = 1; j < factory.entityCursor; j++)
            {
                bool customId = r.ReadBoolean();
                if (customId)
                {
                    factory.entityPool[j].customId = r.ReadInt32();
                    int oldId = r.ReadInt32();

                    if (componentRegistry.migrationMap.ContainsKey(oldId))
                    {
                        int newId = componentRegistry.migrationMap[oldId];
                        factory.entityPool[j].customType = newId;
                    }
                    else
                    {
                        factory.entityPool[j].customId = 0;
                    }
                }
                EntityDataExtensions.ImportData(ref factory.entityPool[j], r);
            }
        }

        public void Free()
        {
            for (int i = 1; i < componentRegistry.data.Count; i++)
            {
                ComponentTypePool pool = pools[i];
                pool.Free();
            }
            pools.Clear();
        }

        public void Export(BinaryWriter w)
        {
            w.Write(0);
            
            CommonAPIPlugin.logger.LogInfo("Start Component System Export");
            
            componentRegistry.ExportContainer(pools, w);

            for (int j = 1; j < factory.entityCursor; j++)
            {
                int customID = factory.entityPool[j].customId;
                w.Write(customID != 0);
                if (customID != 0)
                {
                    w.Write(customID);
                    w.Write(factory.entityPool[j].customType);
                }
                EntityDataExtensions.ExportData(ref factory.entityPool[j], w);
            }
        }

        public ComponentTypePool GetPool(int typeId)
        {
            if (typeId > 0 && typeId < pools.Count)
                return pools[typeId];

            return null;
        }

        public void Update()
        {
            for (int i = 1; i < componentRegistry.data.Count; i++)
            {
                ComponentTypePool pool = pools[i];
                pool.UpdatePool();
            }
        }

        public void UpdateMultithread(int usedThreadCount, int currentThreadIdx, int minimumCount)
        {
            for (int i = 1; i < componentRegistry.data.Count; i++)
            {
                ComponentTypePool pool = pools[i];
                pool.UpdatePoolMultithread(usedThreadCount, currentThreadIdx, minimumCount);
            }
        }

        public void PowerUpdate()
        {
            for (int i = 1; i < componentRegistry.data.Count; i++)
            {
                ComponentTypePool pool = pools[i];
                pool.UpdatePool(pool.InitPowerUpdate);
            }
        }

        public void PowerUpdateMultithread(int usedThreadCount, int currentThreadIdx, int minimumCount)
        {
            for (int i = 1; i < componentRegistry.data.Count; i++)
            {
                ComponentTypePool pool = pools[i];
                pool.UpdatePoolMultithread(usedThreadCount, currentThreadIdx, minimumCount, pool.InitPowerUpdate);
            }
        }
    }
}