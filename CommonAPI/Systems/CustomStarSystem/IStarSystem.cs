namespace CommonAPI.Systems
{
    /// <summary>
    /// Defines a system, which has one instance per star.
    /// </summary>
    public interface IStarSystem : ISerializeState
    {
        void Init(StarData star);
    }
}