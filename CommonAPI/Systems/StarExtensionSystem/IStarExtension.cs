namespace CommonAPI.Systems
{
    /// <summary>
    /// Defines a extension, which has one instance per star.
    /// </summary>
    public interface IStarExtension : ISerializeState
    {
        void Init(StarData star);
    }
}