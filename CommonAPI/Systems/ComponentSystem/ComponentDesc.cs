
namespace CommonAPI.Systems
{
    public class ComponentDesc : CustomDesc
    {
        public const string FIELD_NAME = CommonAPIPlugin.ID + ":componentId";
        
        public string componentId;
        public override void ApplyProperties(PrefabDesc desc)
        {
            int id = ComponentSystem.componentRegistry.GetUniqueId(componentId);
            desc.SetProperty(FIELD_NAME, id);
        }
    }
}