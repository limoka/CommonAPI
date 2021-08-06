namespace CommonAPI
{
    /// <summary>
    /// Allow this factory system to receive update calls
    /// </summary>
    public interface IUpdate : IFactorySystem
    {
        /// <summary>
        /// This call will happen after main factory update 
        /// </summary>
        void Update();
    }
}