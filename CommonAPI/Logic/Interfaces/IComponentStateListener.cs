namespace CommonAPI
{
    /// <summary>
    /// Allows to listen to factory component add/remove events
    /// </summary>
    public interface IComponentStateListener
    {
        void OnLogicComponentsAdd(int entityId, PrefabDesc desc, int prebuildId);
        void OnPostlogicComponentsAdd(int entityId, PrefabDesc desc, int prebuildId);
        void OnLogicComponentsRemove(int entityId);
    }
}