namespace CommonAPI
{
    public static class PlanetFactoryExtensions
    {
        public static T GetSystem<T>(this PlanetFactory factory, int systemId) where T : IPlanetSystem
        {
            if (systemId <= 0) return default;
            return (T)PlanetSystemManager.systems[systemId].GetSystem(factory);
        }
    }
}