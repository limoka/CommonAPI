using CommonAPI.Patches;

namespace CommonAPI.Systems
{
    [CommonAPISubmodule]
    public static class CustomDescSystem
    {
        /// <summary>
        /// Return true if the submodule is loaded.
        /// </summary>
        public static bool Loaded {
            get => _loaded;
            internal set => _loaded = value;
        }

        private static bool _loaded;


        [CommonAPISubmoduleInit(Stage = InitStage.SetHooks)]
        internal static void SetHooks()
        {
            CommonAPIPlugin.harmony.PatchAll(typeof(PrefabDescPatch));
        }
    }
}