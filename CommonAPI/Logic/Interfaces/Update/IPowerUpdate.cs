namespace CommonAPI
{
    /// <summary>
    /// Allow this factory system to receive update calls
    /// </summary>
    public interface IPowerUpdate : IFactorySystem
    {
        /// <summary>
        /// This call will happen before power update 
        /// </summary>
        void PowerUpdate();
    }
}