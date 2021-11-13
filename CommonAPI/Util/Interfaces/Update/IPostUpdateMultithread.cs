namespace CommonAPI
{
    /// <summary>
    /// Allow this factory system to receive update calls
    /// This version also supports multithreading. Note that non single thread version will not get called!
    /// </summary>
    public interface IPostUpdateMultithread : IPostUpdate
    {

        /// <summary>
        /// This call will happen after everything else had already updated
        /// </summary>
        /// <param name="usedThreadCount"></param>
        /// <param name="currentThreadIdx"></param>
        /// <param name="minimumCount"></param>
        void PostUpdateMultithread(int usedThreadCount, int currentThreadIdx, int minimumCount);
    }
}