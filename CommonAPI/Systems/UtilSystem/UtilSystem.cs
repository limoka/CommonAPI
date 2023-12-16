using System;
using System.Collections.Generic;
using CommonAPI.Systems.Patches;

namespace CommonAPI.Systems
{
    public class UtilSystem : BaseSubmodule
    {
        internal static List<Func<string>> messageHandlers = new List<Func<string>>();
        
        internal static UtilSystem Instance => CommonAPIPlugin.GetModuleInstance<UtilSystem>();
        
        internal override void SetHooks()
        {
            CommonAPIPlugin.harmony.PatchAll(typeof(GameLoaderPatch));
        }


        public static void AddLoadMessageHandler(Func<string> handler)
        {
            Instance.ThrowIfNotLoaded();
            messageHandlers.Add(handler);
        }
        
    }
}