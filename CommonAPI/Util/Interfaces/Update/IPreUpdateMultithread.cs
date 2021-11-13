namespace CommonAPI
{
    /// <summary>
    /// Allow this factory system to receive update calls
    /// This version also supports multithreading. Note that non single thread version will not get called!
    /// </summary>
    public interface IPreUpdateMultithread : IPreUpdate
    {
        /// <summary>
        /// This call will happen before main factory update 
        /// </summary>
        void PreUpdateMultithread(int usedThreadCount, int currentThreadIdx, int minimumCount);
    }
}