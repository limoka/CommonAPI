namespace CommonAPI
{
    /// <summary>
    /// Extend Picker behavior
    /// </summary>
    /// <typeparam name="T">Target Picker</typeparam>
    public interface IPickerExtension<in T>
    {
        /// <summary>
        /// Called when picker with extension is open
        /// </summary>
        void Open(T picker);
        /// <summary>
        /// Called when picker with extension is closed
        /// </summary>
        void Close(T picker);
        /// <summary>
        /// Called before picker is open
        /// </summary>
        void OnPopup(T picker);
        /// <summary>
        /// Called after picker is open
        /// </summary>
        void PostPopup(T picker);
    }
    
    /// <summary>
    /// Extend Picker behavior
    /// </summary>
    /// <typeparam name="T">Target Picker</typeparam>
    public interface IMouseHandlerExtension<in T> : IPickerExtension<T>
    {
        /// <summary>
        /// Called when player click left mouse button
        /// </summary>
        /// <returns>Should picker close</returns>
        bool OnBoxMouseDown(T picker);
        
        /// <summary>
        /// Called when mouse position being checked for intersection
        /// </summary>
        void TestMouseIndex(T picker);
    }

    public interface IUpdatePickerExtension<in T> : IPickerExtension<T>
    {
        void OnUpdate(T picker);
    }

    /// <summary>
    /// If implemented all items will be shown, even if locked
    /// </summary>
    public interface ShowLocked
    {
        
    }
}