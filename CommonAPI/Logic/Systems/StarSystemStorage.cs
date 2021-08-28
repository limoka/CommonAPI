using System.Collections.Generic;
using System.IO;

namespace CommonAPI
{
    public class StarSystemStorage : ISerializeState
    {
        public List<IStarSystem> systems = new List<IStarSystem>();
        public int starIndex;
        
        public void InitOnLoad(int index)
        {
            systems.Capacity = GameMain.galaxy.starCount;
            starIndex = index;
            
            for (int i = 0; i < GameMain.galaxy.starCount; i++)
            {
                systems.Add(StarSystemManager.registry.GetNew(index));
                systems[i].Init(GameMain.galaxy.stars[i]);
            }
        }

        public void InitNewStar(StarData star)
        {
            if (GetSystem(star) != null) return;
            
            systems.Capacity += 1;
            systems.Add(StarSystemManager.registry.GetNew(starIndex));
            systems[star.index].Init(GameMain.galaxy.stars[star.index]);
        }
        
        
        public IStarSystem GetSystem(StarData star)
        {
            if (star.index >= 0 && star.index < systems.Count)
            {
                return systems[star.index];
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
            foreach (IStarSystem system in systems)
            {
                system.Free();
            }
            systems.Clear();
            systems = null;
        }

        public void Export(BinaryWriter w)
        {
            w.Write(0);
            
            CommonAPIPlugin.logger.LogInfo("Start star System storage Export");

            for (int i = 0; i < GameMain.galaxy.starCount; i++)
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
            systems.Capacity = GameMain.galaxy.starCount;

            for (int i = 0; i < GameMain.galaxy.starCount; i++)
            {
                systems.Add(StarSystemManager.registry.GetNew(starIndex));
                systems[i].Init(GameMain.galaxy.stars[i]);
                systems[i].Import(r);
            }
        }
    
    }
}