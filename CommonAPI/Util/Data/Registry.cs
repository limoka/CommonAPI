using System;
using System.Collections.Generic;
using System.IO;

namespace CommonAPI
{
    /// <summary>
    /// Data structure that allows to register objects with unique string ID's
    /// </summary>
    public class Registry : ISerializeState
    {
        public Dictionary<string, int> idMap = new Dictionary<string, int>();
        
        public Dictionary<int, int> migrationMap = new Dictionary<int, int>();
        public List<string> removedIds = new List<string>();

        protected int lastId;

        public Registry(int startId = 1)
        {
            lastId = startId - 1;
        }

        /// <summary>
        /// Virtual method for intended to change Registry behavior
        /// </summary>
        /// <param name="key">String ID</param>
        /// <param name="id">Assigned integer ID</param>
        /// <param name="item">Registered object</param>
        protected virtual void OnItemRegistered(string key, int id, object item)
        {
            
        }

        /// <summary>
        /// Clears and prepares internal migration data structure.
        /// </summary>
        public void InitUnitMigrationMap()
        {
            migrationMap.Clear();
            foreach (var kv in idMap)
            {
                migrationMap.Add(kv.Value, kv.Value);
            }
        }
        
        /// <summary>
        /// Register new object
        /// </summary>
        /// <param name="key">Unique string ID</param>
        /// <param name="item">Object</param>
        /// <returns></returns>
        public int Register(string key, object item = null)
        {
            if (!idMap.ContainsKey(key))
            {
                OnItemRegistered(key, lastId + 1, item);
                idMap.Add(key, ++lastId);
                return lastId;
            }

            return GetUniqueId(key);
        }

        /// <summary>
        /// Get assigned integer ID for string ID
        /// </summary>
        /// <param name="typeId">string ID</param>
        /// <returns>Unique integer ID</returns>
        /// <exception cref="ArgumentException">Thrown if requested string ID was never registered</exception>
        public int GetUniqueId(string typeId)
        {
            if (idMap.ContainsKey(typeId))
                return idMap[typeId];
            
            throw new ArgumentException($"Item with id {typeId} is not registered!");
        }

        /// <summary>
        /// Migrate old saved assigned integer ID to new integer ID
        /// </summary>
        /// <param name="oldId"></param>
        /// <returns></returns>
        public int MigrateId(int oldId)
        {
            if (migrationMap.ContainsKey(oldId))
            {
                return migrationMap[oldId];
            }

            return 0;
        }

        public void ManualIdMap(int oldId, int newId)
        {
            migrationMap.Add(oldId, newId);
        }

        public void Free()
        {
        }

        /// <summary>
        /// Save registry data
        /// </summary>
        /// <param name="w">Binary Writer class</param>
        public void Export(BinaryWriter w)
        {
            w.Write((byte)0);
            w.Write(idMap.Count);
            foreach (var kv in idMap)
            {
                w.Write(kv.Key);
                w.Write(kv.Value);
            }
        }

        /// <summary>
        /// Reads saved registry data. Also prepares ID migration data
        /// </summary>
        /// <param name="r"></param>
        public void Import(BinaryReader r)
        {
            migrationMap.Clear();
            r.ReadByte();
            int count = r.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                string key = r.ReadString();
                int oldId = r.ReadInt32();
                try
                {
                    int newId = GetUniqueId(key);
                    migrationMap.Add(oldId, newId);
                }
                catch (ArgumentException)
                {
                    removedIds.Add(key);
                }
            }
        }
    }
}