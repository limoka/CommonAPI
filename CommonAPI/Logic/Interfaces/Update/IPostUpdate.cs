namespace CommonAPI
{
    /// <summary>
    /// Allow this factory system to receive update calls
    /// </summary>
    public interface IPostUpdate
    {
        /// <summary>
        /// This call will happen after everything else had already updated
        /// </summary>
        void PostUpdate();
    }
}