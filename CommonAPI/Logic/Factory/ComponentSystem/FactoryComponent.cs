using System.IO;

namespace CommonAPI
{
    public class FactoryComponent : IPoolable
    {
        public int id;
        public int entityId;
        public int pcId;

        public virtual void Free()
        {
            id = 0;
            entityId = 0;
            pcId = 0;
        }

        public int GetId()
        {
            return id;
        }

        public void SetId(int _id)
        {
            id = _id;
        }

        public virtual void Import(BinaryReader r)
        {
            id = r.ReadInt32();
            entityId = r.ReadInt32();
            pcId = r.ReadInt32();
        }

        public virtual void Export(BinaryWriter w)
        {
            w.Write(id);
            w.Write(entityId);
            w.Write(pcId);
        }

        public virtual void OnAdded(PrebuildData data, PlanetFactory factory) { }

        public virtual void OnRemoved(PlanetFactory factory) { }

        public virtual int InternalUpdate(float power, PlanetFactory factory)
        {
            return 0;
        }
        
        public virtual void UpdateAnimation(ref AnimData data, int updateResult, float power)
        {
        }

        public virtual void UpdateSigns(ref SignData data, int updateResult, float power, PlanetFactory factory)
        {
        }

        public virtual void UpdatePowerState(ref PowerConsumerComponent component) { }
    }
}