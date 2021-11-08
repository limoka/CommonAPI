using System;
using System.Collections.Generic;

namespace CommonAPI
{
    /// <summary>
    /// Data structure that allows to register instances of objects with unique string ID's
    /// </summary>
    /// <typeparam name="T">Base class for all instances</typeparam>
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

        /// <summary>
        /// Register new instance
        /// </summary>
        /// <param name="key">Unique string ID</param>
        /// <param name="item">instance of object</param>
        /// <returns>Assigned integer ID</returns>
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