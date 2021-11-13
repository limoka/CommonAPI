
namespace CommonAPI.Systems
{
    public static class PlanetFactoryExtensions
    {
        public static T GetSystem<T>(this PlanetFactory factory, int systemId) where T : IPlanetSystem
        {
            CustomPlanetSystem.ThrowIfNotLoaded();
            if (systemId <= 0) return default;
            return (T)CustomPlanetSystem.systems[systemId].GetSystem(factory);
        }
    }
}