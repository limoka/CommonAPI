namespace CommonAPI
{
    /// <summary>
    /// Allow this factory system to receive update calls
    /// This version also supports multithreading. Note that non single thread version will not get called!
    /// </summary>
    public interface IPowerUpdateMultithread : IPowerUpdate
    {
        /// <summary>
        /// This call will happen before power update 
        /// </summary>
        void PowerUpdateMultithread(int usedThreadCount, int currentThreadIdx, int minimumCount);
    }
}