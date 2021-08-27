using HarmonyLib;

namespace CommonAPI
{
    [HarmonyPatch]
    public static class StarSystemHooks
    {
        //Single thread update calls
        [HarmonyPatch(typeof(GameData), "GameTick")]
        [HarmonyPrefix]
        public static void PreUpdateST(GameData __instance, long time)
        {
            if (GameMain.multithreadSystem.multithreadSystemEnable)
            {
                PerformanceMonitor.BeginSample(ECpuWorkEntry.DysonSphere);
                StarSystemManager.PreUpdateOnlySinglethread();
                PerformanceMonitor.EndSample(ECpuWorkEntry.DysonSphere);
                return;
            }
            
            PerformanceMonitor.BeginSample(ECpuWorkEntry.DysonSphere);
            for (int i = 0; i < GameMain.galaxy.starCount; i++)
            {
                StarData star = GameMain.galaxy.stars[i];
                if (star == null) continue;

                StarSystemManager.PreUpdate(star);
            }
            PerformanceMonitor.EndSample(ECpuWorkEntry.DysonSphere);
        }
        
        [HarmonyPatch(typeof(TrashSystem), "GameTick")]
        [HarmonyPostfix]
        public static void UpdateST(TrashSystem __instance, long time)
        {
            if (GameMain.multithreadSystem.multithreadSystemEnable)
            {
                PerformanceMonitor.EndSample(ECpuWorkEntry.Trash);
                PerformanceMonitor.BeginSample(ECpuWorkEntry.DysonSphere);
                StarSystemManager.UpdateOnlySinglethread();
                PerformanceMonitor.EndSample(ECpuWorkEntry.DysonSphere);
                PerformanceMonitor.BeginSample(ECpuWorkEntry.Trash);
                return;
            }
            
            PerformanceMonitor.EndSample(ECpuWorkEntry.Trash);
            PerformanceMonitor.BeginSample(ECpuWorkEntry.DysonSphere);
            for (int i = 0; i < GameMain.galaxy.starCount; i++)
            {
                StarData star = GameMain.galaxy.stars[i];
                if (star == null) continue;

                StarSystemManager.Update(star);
            }
            PerformanceMonitor.EndSample(ECpuWorkEntry.DysonSphere);
            PerformanceMonitor.BeginSample(ECpuWorkEntry.Trash);
        }
        
        //Multi-thread update calls, used only if player system support multithreading
        
        //TODO improve multi-thread calls
        
        [HarmonyPatch(typeof(DysonSphere), "RocketGameTick")]
        [HarmonyPostfix]
        public static void PowerTickMultithread(DysonSphere __instance, int _usedThreadCnt, int _curThreadIdx)
        {
            StarSystemManager.UpdateMultithread(__instance.starData, _usedThreadCnt, _curThreadIdx, 12);
        }
    }
}