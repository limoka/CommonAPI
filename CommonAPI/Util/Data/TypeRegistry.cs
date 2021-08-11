using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace CommonAPI
{
    
    public class TypeRegistry<TItem, TCont> : InstanceRegistry<Type>
        where TCont : ISerializeState
    {
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