using System;
using System.Collections.Generic;

namespace CommonAPI
{
    public class InstanceRegistry<T> : Registry
    {
        public List<T> data = new List<T>();

        public InstanceRegistry()
        {
            data.Add(default);
        }
        
        public InstanceRegistry(int startId) : base(startId)
        {
            for (int i = 0; i < startId; i++)
            {
                data.Add(default);
            }
        }

        public virtual int Register(string key, T item)
        {
            return Register(key, (object)item);
        }

        protected override void OnItemRegistered(string key, int id, object item)
        {
            if (item is T o)
            {
                data.Add(o);
                return;
            }
            throw new ArgumentException($"Tried to register invalid type {item.GetType().FullName}, expected {typeof(T).FullName}");
        }
    }
}