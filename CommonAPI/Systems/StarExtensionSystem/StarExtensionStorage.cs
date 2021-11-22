using System.Collections.Generic;
using System.IO;

namespace CommonAPI.Systems
{
    public class StarExtensionStorage : ISerializeState
    {
        public List<IStarExtension> extensions = new List<IStarExtension>();
        public int starIndex;
        
        public void InitOnLoad(int index)
        {
            extensions.Capacity = GameMain.galaxy.starCount;
            starIndex = index;
            
            for (int i = 0; i < GameMain.galaxy.starCount; i++)
            {
                extensions.Add(StarExtensionSystem.registry.GetNew(index));
                extensions[i].Init(GameMain.galaxy.stars[i]);
            }
        }

        public void InitNewStar(StarData star)
        {
            if (GetSystem(star) != null) return;
            
            extensions.Capacity += 1;
            extensions.Add(StarExtensionSystem.registry.GetNew(starIndex));
            extensions[star.index].Init(GameMain.galaxy.stars[star.index]);
        }
        
        
        public IStarExtension GetSystem(StarData star)
        {
            if (star.index >= 0 && star.index < extensions.Count)
            {
                return extensions[star.index];
            }

            return null;
        }
        
        public void DrawUpdate(StarData star)
        {
            if (GetSystem(star) is IDrawUpdate draw)
            {
                draw.Draw();
            }
        }

        public void PreUpdate(StarData star)
        {
            if (GetSystem(star) is IPreUpdate pre)
            {
                pre.PreUpdate();
            }
        }
        
        public void Update(StarData star)
        {
            if (GetSystem(star) is IUpdate update)
            {
                update.Update();
            }
        }
        
        public void PostUpdate(StarData star)
        {
            if (GetSystem(star) is IPostUpdate update)
            {
                update.PostUpdate();
            }
        }
        
        public void PowerUpdate(StarData star)
        {
            if (GetSystem(star) is IPowerUpdate update)
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
        
        public void PreUpdateMultithread(StarData star, int usedThreadCount, int currentThreadIdx, int minimumCount)
        {
            if (GetSystem(star) is IPreUpdateMultithread pre)
            {
                pre.PreUpdateMultithread(usedThreadCount, currentThreadIdx, minimumCount);
            }
        }

        public void UpdateMultithread(StarData star, int usedThreadCount, int currentThreadIdx, int minimumCount)
        {
            if (GetSystem(star) is IUpdateMultithread update)
            {
                update.UpdateMultithread(usedThreadCount, currentThreadIdx, minimumCount);
            }
        }
        
        public void PostUpdateMultithread(StarData star, int usedThreadCount, int currentThreadIdx, int minimumCount)
        {
            if (GetSystem(star) is IPostUpdateMultithread update)
            {
                update.PostUpdateMultithread(usedThreadCount, currentThreadIdx, minimumCount);
            }
        }
        
        public void PowerUpdateMultithread(StarData star, int usedThreadCount, int currentThreadIdx, int minimumCount)
        {
            if (GetSystem(star) is IPowerUpdateMultithread update)
            {
                update.PowerUpdateMultithread(usedThreadCount, currentThreadIdx, minimumCount);
            }
        }
        
        public void Free()
        {
            foreach (IStarExtension system in extensions)
            {
                system.Free();
            }
            extensions.Clear();
            extensions = null;
        }

        public void Export(BinaryWriter w)
        {
            w.Write(0);
            
            CommonAPIPlugin.logger.LogInfo("Start star System storage Export");

            for (int i = 0; i < GameMain.galaxy.starCount; i++)
            {
                extensions[i].Export(w);
            }
        }

        public void Import(BinaryReader r)
        {
            GameData data = GameMain.data;
            
            CommonAPIPlugin.logger.LogInfo("Start System storage Import");

            int ver = r.ReadInt32();
            
            extensions.Clear();
            extensions.Capacity = GameMain.galaxy.starCount;

            for (int i = 0; i < GameMain.galaxy.starCount; i++)
            {
                extensions.Add(StarExtensionSystem.registry.GetNew(starIndex));
                extensions[i].Init(GameMain.galaxy.stars[i]);
                extensions[i].Import(r);
            }
        }
    
    }
}