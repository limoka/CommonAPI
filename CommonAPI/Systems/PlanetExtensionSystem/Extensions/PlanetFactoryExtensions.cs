
namespace CommonAPI.Systems
{
    public static class PlanetFactoryExtensions
    {
        public static T GetSystem<T>(this PlanetFactory factory, int systemId) where T : IPlanetExtension
        {
            PlanetExtensionSystem.ThrowIfNotLoaded();
            if (systemId <= 0) return default;
            return (T)PlanetExtensionSystem.extensions[systemId].GetExtension(factory);
        }
    }
}