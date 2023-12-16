using CommonAPI.Patches;

namespace CommonAPI.Systems
{
    public class CustomDescSystem : BaseSubmodule
    {
        internal override void SetHooks()
        {
            CommonAPIPlugin.harmony.PatchAll(typeof(PrefabDescPatch));
        }
    }
}