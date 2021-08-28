using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace CommonAPI
{
    
    public class TypeRegistry<TItem, TCont> : InstanceRegistry<Type>
        where TCont : ISerializeState
    {
        public override int Register(string key, Type item)
        {
            if (typeof(TItem).IsAssignableFrom(item))
            {
                return base.Register(key, item);
            }

            throw new ArgumentException($"Trying to register type {item.FullName}, which does not implement {typeof(TItem).FullName}!");
        }

        public TItem GetNew(int typeId)
        {
            if (typeId > 0 && typeId < data.Count)
            {
                return (TItem) Activator.CreateInstance(data[typeId]);
            }

            throw new ArgumentException($"Item with id {typeId} is not registered!");
        }

        public void ImportAndMigrate(IList<TCont> list, BinaryReader r)
        {
            while (true)
            {
                int oldId = r.ReadInt32();
                if (oldId == 0) break;
                int newId = MigrateId(oldId);

                if (newId != 0)
                {
                    TCont pool = list[newId];
                    r.ReadInt64();
                    pool.Import(r);
                }
                else
                {
                    long len = r.ReadInt64();
                    r.ReadBytes((int)len);
                }
            }
        }
        

        public void ExportContainer(IList<TCont> list, BinaryWriter w)
        {
            for (int i = 1; i < list.Count; i++)
            {
                w.Write(i);
                MemoryStream stream = new MemoryStream();
                BinaryWriter tw = new BinaryWriter(stream);
                TCont storage = list[i];
                storage.Export(tw);
                w.Write(stream.Length);
                w.Write(stream.ToArray());
            }
            
            w.Write(0);
        }
    }
}