using System;
using CommonAPI.Patches;

namespace CommonAPI.Systems
{
    public class ComponentExtensionModule : BaseSubmodule
    {
        internal override Type[] Dependencies => new[] { typeof(CustomDescSystem) };

        internal override void SetHooks()
        {
            CommonAPIPlugin.harmony.PatchAll(typeof(CopyPastePatch));
            CommonAPIPlugin.harmony.PatchAll(typeof(EntityDataSetNullPatch));
            CommonAPIPlugin.harmony.PatchAll(typeof(UIGamePatch));
        }
        
        internal override void Load()
        {
            CommonAPIPlugin.registries.Add($"{CommonAPIPlugin.ID}:ComponentRegistry", ComponentExtension.componentRegistry);
            PlanetExtensionSystem.registry.Register(ComponentExtension.systemID, typeof(ComponentExtension));
        }
    }
}