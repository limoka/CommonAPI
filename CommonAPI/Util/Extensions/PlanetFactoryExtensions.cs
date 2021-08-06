namespace CommonAPI
{
    public static class PlanetFactoryExtensions
    {
        public static T GetSystem<T>(this PlanetFactory factory, int systemId) where T : IFactorySystem
        {
            if (systemId <= 0) return default;
            return (T)CustomFactory.systems[systemId].GetSystem(factory);
        }
    }
}