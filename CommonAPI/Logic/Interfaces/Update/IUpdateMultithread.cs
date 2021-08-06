namespace CommonAPI
{
    /// <summary>
    /// Allow this factory system to receive update calls
    /// This version also supports multithreading. Note that non single thread version will not get called!
    /// </summary>
    public interface IUpdateMultithread : IUpdate
    {
        /// <summary>
        /// This call will happen after main factory update 
        /// </summary>
        void UpdateMultithread(int usedThreadCount, int currentThreadIdx, int minimumCount);
    }
}