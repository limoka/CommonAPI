using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace CommonAPI
{
    /// <summary>
    /// Data structure that allows to register types of objects with unique string ID's
    /// </summary>
    /// <typeparam name="TItem">Base class of all registered types</typeparam>
    /// <typeparam name="TCont">Type container class. For example look at <see cref="ComponentTypePool"/>></typeparam>
    public class TypeRegistry<TItem, TCont> : InstanceRegistry<Type>
        where TCont : ISerializeState
    {
        /// <summary>
        /// Register new type
        /// </summary>
        /// <param name="key">Unique string ID</param>
        /// <param name="item">Type of new item</param>
        /// <returns>Assigned integer ID</returns>
        /// <exception cref="ArgumentException">Thrown if provided type does not implement TItem</exception>
        public override int Register(string key, Type item)
        {
            if (typeof(TItem).IsAssignableFrom(item))
            {
                return base.Register(key, item);
            }

            throw new ArgumentException($"Trying to register type {item.FullName}, which does not implement {typeof(TItem).FullName}!");
        }

        /// <summary>
        /// Create new instance of registered type
        /// </summary>
        /// <param name="typeId">Unique string ID</param>
        /// <returns>New instance of registered type</returns>
        /// <exception cref="ArgumentException">Thrown if requested string ID was never registered</exception>
        public TItem GetNew(int typeId)
        {
            if (typeId > 0 && typeId < data.Count)
            {
                return (TItem) Activator.CreateInstance(data[typeId]);
            }

            throw new ArgumentException($"Item with id {typeId} is not registered!");
        }

        /// <summary>
        /// Import data to a container list and automatically migrate all used ID's
        /// </summary>
        /// <param name="list">Container list</param>
        /// <param name="r">Binary Reader</param>
        public void ImportAndMigrate(IList<TCont> list, BinaryReader r)
        {
            while (true)
            {
                int oldId = r.ReadInt32();
                if (oldId == 0) break;
                if (oldId == -1) continue;
                
                int newId = MigrateId(oldId);

                if (newId != 0)
                {
                    TCont pool = list[newId];
                    long len = r.ReadInt64();
                    long startPos = r.BaseStream.Position;
                    try
                    {
                        pool.Import(r);
                    }
                    catch (Exception e)
                    {
                        CommonLogger.logger.LogWarning($"Error importing container for type {typeof(TItem).FullName}, message: {e.Message}, Stacktrace:\n{e.StackTrace}");
                        r.BaseStream.Position = startPos + len;
                    }
                }
                else
                {
                    long len = r.ReadInt64();
                    r.ReadBytes((int)len);
                }
            }
        }
        

        /// <summary>
        /// Export container list 
        /// </summary>
        /// <param name="list">Container list</param>
        /// <param name="w">Binary Writer</param>
        public void ExportContainer(IList<TCont> list, BinaryWriter w)
        {
            for (int i = 1; i < list.Count; i++)
            {
                MemoryStream stream = new MemoryStream();
                BinaryWriter tw = new BinaryWriter(stream);
                TCont storage = list[i];
                try
                {
                    storage.Export(tw);
                }
                catch (Exception e)
                {
                    w.Write(-1);
                    CommonLogger.logger.LogWarning($"Error exporting container for type {typeof(TItem).FullName}, message: {e.Message}, Stacktrace:\n{e.StackTrace}");
                    continue;
                }
                
                
                w.Write(i);
                w.Write(stream.Length);
                w.Write(stream.ToArray());
            }
            
            w.Write(0);
        }
    }
}