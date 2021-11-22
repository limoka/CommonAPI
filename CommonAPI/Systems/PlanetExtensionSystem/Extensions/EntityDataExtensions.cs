using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace CommonAPI
{
    public class IntPropertySerializer : IPropertySerializer
    {
        public static readonly IntPropertySerializer instance = new IntPropertySerializer();
        public void Export(object obj, BinaryWriter w) => w.Write((int)obj);

        public object Import(BinaryReader r) => r.ReadInt32();
        public Type GetTargetType() => typeof(int);
    }
    
    public class IntArrayPropertySerializer : IPropertySerializer
    {
        public static readonly IntArrayPropertySerializer instance = new IntArrayPropertySerializer();
        public void Export(object obj, BinaryWriter w)
        {
            int[] array = (int[])obj;
            w.Write(array.Length);
            foreach (int item in array)
            {
                w.Write(item);
            }
        }

        public object Import(BinaryReader r)
        {
            int len = r.ReadInt32();
            int[] array = new int[len];
            for (int i = 0; i < len; i++)
            {
                array[i] = r.ReadInt32();
            }

            return array;
        }
        public Type GetTargetType() => typeof(int[]);
    }
    
    public static class EntityDataExtensions
    {
        public static Dictionary<string, IPropertySerializer> propertySerializers = new Dictionary<string, IPropertySerializer>();

        public static void DefineProperty(string key, IPropertySerializer serializer)
        {
            if (propertySerializers.ContainsKey(key))
            {
                propertySerializers[key] = serializer;
            }
            else
            {
                propertySerializers.Add(key, serializer);
            }
        }
        
        public static bool HasProperty(this ref EntityData desc, string name)
        {
            if (desc.customData == null)
            {
                desc.customData = new Dictionary<string, object>();
            }

            return desc.customData.ContainsKey(name);
        }

        public static void SetProperty<T>(this ref EntityData desc, string name, T value)
        {
            if (!propertySerializers.ContainsKey(name) || propertySerializers[name] == null)
            {
                throw new ArgumentException($"Can't set property {name} of type {value.GetType().FullName} because serializer for it was not defined!");
            }

            Type targetType = propertySerializers[name].GetTargetType();
            if (targetType != value.GetType())
            {
                throw new ArgumentException($"Can't set property {name} of type {value.GetType().FullName} because serializer defined for {name} is for type {targetType.FullName}!");
            }
            
            if (desc.customData == null)
            {
                desc.customData = new Dictionary<string, object>();
            }

            if (desc.customData.ContainsKey(name))
            {
                desc.customData[name] = value;
            }
            else
            {
                desc.customData.Add(name, value);
            }
        }

        public static T GetProperty<T>(this ref EntityData desc, string name)
        {
            if (desc.customData == null)
            {
                desc.customData = new Dictionary<string, object>();
            }

            if (desc.customData.ContainsKey(name))
            {
                return (T) desc.customData[name];
            }

            return default;
        }

        public static T GetOrAddProperty<T>(this ref EntityData desc, string name) where T : new()
        {
            if (desc.customData == null)
            {
                desc.customData = new Dictionary<string, object>();
            }

            if (desc.customData.ContainsKey(name))
            {
                return (T) desc.customData[name];
            }

            T result = new T();
            desc.customData.Add(name, result);

            return result;
        }

        public static void ExportData(ref EntityData data, BinaryWriter w)
        {
            if (data.customData != null)
            {
                w.Write((byte)data.customData.Count);
                foreach (var kv in data.customData)
                {
                    w.Write(kv.Key);
                    IPropertySerializer serializer = propertySerializers[kv.Key];
                    serializer.Export(kv.Value, w);
                }
            }
            else
            {
                w.Write((byte)0);
            }
        }
        
        public static void ImportData(ref EntityData data, BinaryReader r)
        {
            int count = r.ReadByte();
            for (int i = 0; i < count; i++)
            {
                string key = r.ReadString();
                IPropertySerializer serializer = propertySerializers[key];
                object property = serializer.Import(r);
                data.SetProperty(key, property);
            }
        }
    }
}