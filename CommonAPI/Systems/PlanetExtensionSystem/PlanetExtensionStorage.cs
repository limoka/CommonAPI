using System;
using System.Collections.Generic;
using System.IO;

namespace CommonAPI.Systems
{
    public class PlanetExtensionStorage : ISerializeState
    {
        public List<IPlanetExtension> extensions = new List<IPlanetExtension>();
        public int planetIndex;
        
        public void InitOnLoad(GameData data, int index)
        {
            extensions.Capacity = data.factoryCount;
            planetIndex = index;
            
            for (int i = 0; i < data.factoryCount; i++)
            {
                extensions.Add(PlanetExtensionSystem.registry.GetNew(index));
                extensions[i].Init(data.factories[i]);
            }
        }

        public void InitNewPlanet(PlanetData planet)
        {
            if (GetExtension(planet.factory) != null) return;
            
            extensions.Capacity += 1;
            extensions.Add(PlanetExtensionSystem.registry.GetNew(planetIndex));
            extensions[planet.factory.index].Init(planet.factory);
        }
        
        
        public IPlanetExtension GetExtension(PlanetFactory factory)
        {
            if (factory.index >= 0 && factory.index < extensions.Count)
            {
                return extensions[factory.index];
            }

            return null;
        }
        
        public void DrawUpdate(PlanetFactory factory)
        {
            if (GetExtension(factory) is IDrawUpdate draw)
            {
                draw.Draw();
            }
        }

        public void PreUpdate(PlanetFactory factory)
        {
            if (GetExtension(factory) is IPreUpdate pre)
            {
                pre.PreUpdate();
            }
        }
        
        public void Update(PlanetFactory factory)
        {
            if (GetExtension(factory) is IUpdate update)
            {
                update.Update();
            }
        }
        
        public void PostUpdate(PlanetFactory factory)
        {
            if (GetExtension(factory) is IPostUpdate update)
            {
                update.PostUpdate();
            }
        }
        
        public void PowerUpdate(PlanetFactory factory)
        {
            if (GetExtension(factory) is IPowerUpdate update)
            {
                update.PowerUpdate();
            }
        }

        public bool PreUpdateSupportsMultithread()
        {
            return extensions[0] is IPreUpdateMultithread;
        }
        
        public bool UpdateSupportsMultithread()
        {
            return extensions[0] is IUpdateMultithread;
        }
        
        public bool PostUpdateSupportsMultithread()
        {
            return extensions[0] is IPostUpdateMultithread;
        }
        
        public bool PowerUpdateSupportsMultithread()
        {
            return extensions[0] is IPowerUpdateMultithread;
        }
        
        public void PreUpdateMultithread(PlanetFactory factory, int usedThreadCount, int currentThreadIdx, int minimumCount)
        {
            if (GetExtension(factory) is IPreUpdateMultithread pre)
            {
                pre.PreUpdateMultithread(usedThreadCount, currentThreadIdx, minimumCount);
            }
        }

        public void UpdateMultithread(PlanetFactory factory, int usedThreadCount, int currentThreadIdx, int minimumCount)
        {
            if (GetExtension(factory) is IUpdateMultithread update)
            {
                update.UpdateMultithread(usedThreadCount, currentThreadIdx, minimumCount);
            }
        }
        
        public void PostUpdateMultithread(PlanetFactory factory, int usedThreadCount, int currentThreadIdx, int minimumCount)
        {
            if (GetExtension(factory) is IPostUpdateMultithread update)
            {
                update.PostUpdateMultithread(usedThreadCount, currentThreadIdx, minimumCount);
            }
        }
        
        public void PowerUpdateMultithread(PlanetFactory factory, int usedThreadCount, int currentThreadIdx, int minimumCount)
        {
            if (GetExtension(factory) is IPowerUpdateMultithread update)
            {
                update.PowerUpdateMultithread(usedThreadCount, currentThreadIdx, minimumCount);
            }
        }
        
        public void Free()
        {
            foreach (IPlanetExtension system in extensions)
            {
                system.Free();
            }
            extensions.Clear();
            extensions = null;
        }

        public void Export(BinaryWriter w)
        {
            GameData data = GameMain.data;

            w.Write(0);

            for (int i = 0; i < data.factoryCount; i++)
            {
                extensions[i].Export(w);
            }
        }

        public void Import(BinaryReader r)
        {
            GameData data = GameMain.data;

            int ver = r.ReadInt32();
            
            extensions.Clear();
            extensions.Capacity = data.factoryCount;

            for (int i = 0; i < data.factoryCount; i++)
            {
                extensions.Add(PlanetExtensionSystem.registry.GetNew(planetIndex));
                extensions[i].Init(data.factories[i]);
                extensions[i].Import(r);
            }
        }
    }
}