using UnityEngine.UI;

namespace CommonAPI
{
    public abstract class CustomMachineWindow : ManualBehaviour
    {
        public abstract bool ShouldOpen(int componentId, int protoId);
        public abstract bool DoClosePlayerInventory();
        public abstract bool DoCloseOtherWindows();
        protected abstract void OnMachineChanged();

        public int customId { get; protected set; }
        
        public int entityId { get; protected set; }

        public PlanetFactory factory;

        public PowerSystem powerSystem;
        
        public FactoryComponent component;
        
        public Text titleText;
        public UIPowerIndicator powerIndicator;

        public override void _OnCreate()
        {
            powerIndicator.Init(this);
        }

        public void Open(int newEntityId)
        {
            entityId = newEntityId;
            if (GameMain.localPlanet != null && GameMain.localPlanet.factory != null)
            {
                factory = GameMain.localPlanet.factory;
                powerSystem = factory.powerSystem;
                GameMain.mainPlayer.SetHandItems(0, 0);

                customId = entityId > 0 ? factory.entityPool[entityId].customId : 0;

                OnIdChange();
            }
            transform.SetAsLastSibling();
            UISystem.openWindow = this;
            _Open();
        }

        public void Close()
        {
            entityId = 0;
            customId = 0;
            _Close();
            
            if (entityId != 0)
            {
                PlayerAction_Inspect actionInspect = GameMain.mainPlayer.controller.actionInspect;
                if (actionInspect.inspectId > 0 && actionInspect.inspectType == EObjectType.Entity &&
                    factory.entityPool[actionInspect.inspectId].id == entityId)
                {
                    actionInspect.InspectNothing();
                }
            }

            factory = null;
            powerSystem = null;
            
            UISystem.customInspectId = 0;
            UISystem.openWindow = null;
        }

        internal void OnIdChange()
        {
            if (entityId == 0 || factory == null)
            {
                Close();
                return;
            }

            EntityData entity = factory.entityPool[entityId];
            
            if (entity.id != entityId || !ShouldOpen(entity.customType, entity.protoId))
            {
                Close();
                return;
            }

            if (customId > 0)
            {
                component = ComponentSystem.GetComponent(factory, entity.customType, customId);
            }

            OnMachineChanged();
        }

        public void OnUpdateUI()
        {
            if (entityId == 0 || factory == null)
            {
                Close();
                return;
            }
            
            EntityData entity = factory.entityPool[entityId];
            
            if (entity.id != entityId || !ShouldOpen(entity.customType, entity.protoId))
            {
                Close();
                return;
            }

            if (customId > 0)
            {
                component = ComponentSystem.GetComponent(factory, entity.customType, customId);
            }

            if (component != null)
            {
                ItemProto itemProto = LDB.items.Select(factory.entityPool[component.entityId].protoId);
                titleText.text = itemProto.name;
                
                powerIndicator.OnUpdate(component.pcId);
            }

            _OnUpdate();
        }
    }
}