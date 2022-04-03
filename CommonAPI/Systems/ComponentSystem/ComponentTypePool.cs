using System;
using System.Collections.Generic;

namespace CommonAPI.Systems
{
    public class ComponentTypePool : Pool<FactoryComponent>, IPoolable
    {
        public int id;
        
        public const float DT = 0.016666668f;
        
        private PlanetFactory factory;
        private int PoolTypeId;

        private FactoryComponent lastAddedComponent;
        private PrebuildData lastAddedData;
        


        public ComponentTypePool(PlanetFactory factory, int type)
        {
            this.factory = factory;
            PoolTypeId = type;
            this._cachedInitUpdate = _internalInitUpdate;
            this._cachedInitPowerUpdate = _internalInitPowerUpdate;
        }

        public ComponentTypePool()
        {
        }

        public int AddComponent(int entityId, PrebuildData data = default)
        {
            int num = AddPoolItem(new object[] {entityId, data});
            return num;
        }

        protected override FactoryComponent GetNewInstance()
        {
            return ComponentExtension.componentRegistry.GetNew(PoolTypeId);
        }

        public void OnPostComponentAdded()
        {
            if (lastAddedComponent == null) return;
            
            lastAddedComponent.OnAdded(lastAddedData, factory);
            lastAddedComponent = null;
        }

        protected override void InitPoolItem(FactoryComponent item, object[] data)
        {
            int entityId = (int) data[0];
            
            int pcId = factory.entityPool[entityId].powerConId;

            item.entityId = entityId;
            item.pcId = pcId;
            factory.entityPool[entityId].customId = item.id;
            factory.entityPool[entityId].customType = PoolTypeId;
            lastAddedComponent = item;
            lastAddedData = (PrebuildData)data[1];
        }

        protected override void RemovePoolItem(FactoryComponent item)
        {
            item.OnRemoved(factory);
            
            base.RemovePoolItem(item);
        }

        private Action<FactoryComponent> _cachedInitUpdate;
        protected override Action<FactoryComponent> InitUpdate() => _cachedInitUpdate;
        private void _internalInitUpdate(FactoryComponent item)
        {
            PowerSystem powerSystem = factory.powerSystem;
            PowerConsumerComponent[] consumerPool = powerSystem.consumerPool;
            float[] networkServes = powerSystem.networkServes;
            AnimData[] entityAnimPool = factory.entityAnimPool;
            SignData[] entitySignPool = factory.entitySignPool;
            int[][] entityNeeds = factory.entityNeeds;

            int entityId = item.entityId;
            float power = networkServes[consumerPool[item.pcId].networkId];

            int animationDelta = item.InternalUpdate(power, factory);
            item.UpdateAnimation(ref entityAnimPool[entityId], animationDelta, power);
            entityAnimPool[entityId].power = power;
            item.UpdateSigns(ref entitySignPool[entityId], animationDelta, power, factory);
            entityNeeds[entityId] = item.UpdateNeeds();
        }

        private Action<FactoryComponent> _cachedInitPowerUpdate;
        public Action<FactoryComponent> InitPowerUpdate() => _cachedInitPowerUpdate;
        private void _internalInitPowerUpdate(FactoryComponent item)
        {
            PowerConsumerComponent[] consumerPool = factory.powerSystem.consumerPool;
            item.UpdatePowerState(ref consumerPool[item.pcId]);
        }

        public int GetId()
        {
            return id;
        }

        public void SetId(int id)
        {
            this.id = id;
        }
    }
}