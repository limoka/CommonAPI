using System;
using System.Collections.Generic;
using CommonAPI.Systems.Patches;

namespace CommonAPI.Systems
{
    [CommonAPISubmodule]
    public static class UtilSystem
    {
        /// <summary>
        /// Return true if the submodule is loaded.
        /// </summary>
        public static bool Loaded {
            get => _loaded;
            internal set => _loaded = value;
        }

        private static bool _loaded;

        internal static List<Func<string>> messageHandlers = new List<Func<string>>();
        

        [CommonAPISubmoduleInit(Stage = InitStage.SetHooks)]
        internal static void SetHooks()
        {
            CommonAPIPlugin.harmony.PatchAll(typeof(GameLoaderPatch));
        }
        
        internal static void ThrowIfNotLoaded()
        {
            if (!Loaded)
            {
                throw new InvalidOperationException(
                    $"{nameof(UtilSystem)} is not loaded. Please use [{nameof(CommonAPISubmoduleDependency)}(nameof({nameof(UtilSystem)})]");
            }
        }


        public static void AddLoadMessageHandler(Func<string> handler)
        {
            ThrowIfNotLoaded();
            messageHandlers.Add(handler);
        }
        
    }
}