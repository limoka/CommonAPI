using System;
using System.Collections.Generic;
using System.IO;

namespace CommonAPI.Systems
{
    public class PlanetSystemStorage : ISerializeState
    {
        public List<IPlanetSystem> systems = new List<IPlanetSystem>();
        public int planetIndex;
        
        public void InitOnLoad(GameData data, int index)
        {
            systems.Capacity = data.factoryCount;
            planetIndex = index;
            
            for (int i = 0; i < data.factoryCount; i++)
            {
                systems.Add(CustomPlanetSystem.registry.GetNew(index));
                systems[i].Init(data.factories[i]);
            }
        }

        public void InitNewPlanet(PlanetData planet)
        {
            if (GetSystem(planet.factory) != null) return;
            
            systems.Capacity += 1;
            systems.Add(CustomPlanetSystem.registry.GetNew(planetIndex));
            systems[planet.factory.index].Init(planet.factory);
        }
        
        
        public IPlanetSystem GetSystem(PlanetFactory factory)
        {
            if (factory.index >= 0 && factory.index < systems.Count)
            {
                return systems[factory.index];
            }

            return null;
        }
        
        public void DrawUpdate(PlanetFactory factory)
        {
            if (GetSystem(factory) is IDrawUpdate draw)
            {
                draw.Draw();
            }
        }

        public void PreUpdate(PlanetFactory factory)
        {
            if (GetSystem(factory) is IPreUpdate pre)
            {
                pre.PreUpdate();
            }
        }
        
        public void Update(PlanetFactory factory)
        {
            if (GetSystem(factory) is IUpdate update)
            {
                update.Update();
            }
        }
        
        public void PostUpdate(PlanetFactory factory)
        {
            if (GetSystem(factory) is IPostUpdate update)
            {
                update.PostUpdate();
            }
        }
        
        public void PowerUpdate(PlanetFactory factory)
        {
            if (GetSystem(factory) is IPowerUpdate update)
            {
                update.PowerUpdate();
            }
        }

        public bool PreUpdateSupportsMultithread()
        {
            return systems[0] is IPreUpdateMultithread;
        }
        
        public bool UpdateSupportsMultithread()
        {
            return systems[0] is IUpdateMultithread;
        }
        
        public bool PostUpdateSupportsMultithread()
        {
            return systems[0] is IPostUpdateMultithread;
        }
        
        public bool PowerUpdateSupportsMultithread()
        {
            return systems[0] is IPowerUpdateMultithread;
        }
        
        public void PreUpdateMultithread(PlanetFactory factory, int usedThreadCount, int currentThreadIdx, int minimumCount)
        {
            if (GetSystem(factory) is IPreUpdateMultithread pre)
            {
                pre.PreUpdateMultithread(usedThreadCount, currentThreadIdx, minimumCount);
            }
        }

        public void UpdateMultithread(PlanetFactory factory, int usedThreadCount, int currentThreadIdx, int minimumCount)
        {
            if (GetSystem(factory) is IUpdateMultithread update)
            {
                update.UpdateMultithread(usedThreadCount, currentThreadIdx, minimumCount);
            }
        }
        
        public void PostUpdateMultithread(PlanetFactory factory, int usedThreadCount, int currentThreadIdx, int minimumCount)
        {
            if (GetSystem(factory) is IPostUpdateMultithread update)
            {
                update.PostUpdateMultithread(usedThreadCount, currentThreadIdx, minimumCount);
            }
        }
        
        public void PowerUpdateMultithread(PlanetFactory factory, int usedThreadCount, int currentThreadIdx, int minimumCount)
        {
            if (GetSystem(factory) is IPowerUpdateMultithread update)
            {
                update.PowerUpdateMultithread(usedThreadCount, currentThreadIdx, minimumCount);
            }
        }
        
        public void Free()
        {
            foreach (IPlanetSystem system in systems)
            {
                system.Free();
            }
            systems.Clear();
            systems = null;
        }

        public void Export(BinaryWriter w)
        {
            GameData data = GameMain.data;

            w.Write(0);
            
            CommonAPIPlugin.logger.LogInfo("Start System storage Export");

            for (int i = 0; i < data.factoryCount; i++)
            {
                systems[i].Export(w);
            }
        }

        public void Import(BinaryReader r)
        {
            GameData data = GameMain.data;
            
            CommonAPIPlugin.logger.LogInfo("Start System storage Import");

            int ver = r.ReadInt32();
            
            systems.Clear();
            systems.Capacity = data.factoryCount;

            for (int i = 0; i < data.factoryCount; i++)
            {
                systems.Add(CustomPlanetSystem.registry.GetNew(planetIndex));
                systems[i].Init(data.factories[i]);
                systems[i].Import(r);
            }
        }
    }
}