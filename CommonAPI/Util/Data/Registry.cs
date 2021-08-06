using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace CommonAPI
{
    
    public class Registry<TItem, TCont> : SimpleRegistry
        where TCont : ISerializeState
    {
        public List<Type> data = new List<Type>();

        public Registry()
        {
            data.Add(null);
        }


        public override int Register(string key, [NotNull] Type system)
        {
            if (system == null) throw new ArgumentNullException(nameof(system));
            if (!idMap.ContainsKey(key))
            {
                data.Add(system);
                idMap.Add(key, data.Count - 1);
                return data.Count - 1;
            }

            return GetUniqueId(key);
        }
        
        public int Register(Type system)
        {
            if (!idMap.ContainsKey(system.FullName))
            {
                data.Add(system);
                idMap.Add(system.FullName, data.Count - 1);
                return data.Count - 1;
            }
            
            return GetUniqueId(system.FullName);
        }

        public TItem GetNew(int typeId)
        {
            if (typeId > 0 && typeId < data.Count)
            {
                return (TItem) Activator.CreateInstance(data[typeId]);
            }

            return default;
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