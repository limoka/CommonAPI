using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx.Bootstrap;
using CommonAPI.Patches;
using HarmonyLib;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace CommonAPI.Systems
{
    /// <summary>
    /// This class allows to define new KeyBinds and use them easily in your code
    /// </summary>
    public class CustomKeyBindSystem : BaseSubmodule
    {

        internal static Dictionary<string, PressKeyBind> customKeys = new Dictionary<string, PressKeyBind>();
        internal static Registry keyRegistry;
        
        internal static CustomKeyBindSystem Instance => CommonAPIPlugin.GetModuleInstance<CustomKeyBindSystem>();
        
        internal override void SetHooks()
        {
            CommonAPIPlugin.harmony.PatchAll(typeof(KeyBindPatches));
            CommonAPIPlugin.harmony.PatchAll(typeof(Chainloader_Patch));
        }
        
        internal override void Load()
        {
            // Built-in array length is 256, we will resize it to 512 later, and use 256 as startId to avoid possible conflicts in future
            keyRegistry = new Registry(256);
        }

        /// <summary>
        /// Register new KeyBind.
        /// </summary>
        /// <param name="key">Default value for new Keybind</param>
        /// <typeparam name="T">Key press type class. For example PressKeyBind</typeparam>
        public static void RegisterKeyBind<T>(BuiltinKey key) where T : PressKeyBind, new()
        {
            RegisterKeyBindWithReturn<T>(key);
        }
        
        /// <summary>
        /// Register new KeyBind.
        /// </summary>
        /// <param name="key">Default value for new Keybind</param>
        /// <typeparam name="T">Key press type class. For example PressKeyBind</typeparam>
        public static T RegisterKeyBindWithReturn<T>(BuiltinKey key) where T : PressKeyBind, new()
        {
            Instance.ThrowIfNotLoaded();
            
            string id = "KEY" + key.name;
            int index = keyRegistry.Register(id);
            key.id = index;
            
            T keyBind = new T();
            keyBind.Init(key);

            customKeys.Add(id, keyBind);
            return keyBind;
        }

        public static ICollection<PressKeyBind> GetRegisteredKeybinds()
        {
            return customKeys.Values;
        }

        /// <summary>
        /// Checks if KeyBind with specified ID was registered
        /// </summary>
        /// <param name="id">KeyBind string ID</param>
        /// <returns>Does such KeyBind exist?</returns>
        public static bool HasKeyBind(string id)
        {
            Instance.ThrowIfNotLoaded();
            string key = "KEY" + id;
            return customKeys.ContainsKey(key);
        }

        /// <summary>
        /// Get KeyBind with specified ID
        /// </summary>
        /// <param name="id">KeyBind string ID</param>
        /// <returns>registered KeyBind. if it KeyBind for ID was not found returns null</returns>
        public static PressKeyBind GetKeyBind(string id)
        {
            Instance.ThrowIfNotLoaded();
            string key = "KEY" + id;
            if (customKeys.ContainsKey(key))
            {
                return customKeys[key];
            }

            return null;
        }

        /// <summary>
        /// Save KeyBind data
        /// </summary>
        /// <param name="w">Binary Writer class</param>
        public static void Export(BinaryWriter w)
        {
            w.Write((byte)0);
            // Save begin position
            long beginPos = w.Seek(0, SeekOrigin.Current);
            // Write zero here, we will replace it later
            w.Write(0);

            int count = 0;
            foreach (var kv in customKeys)
            {
                ref CombineKey key = ref DSPGame.globalOption.overrideKeys[kv.Value.defaultBind.id];
                if (key.IsNull())
                    continue;
                count++;
                w.Write(kv.Key);
                w.Write(key.keyCode);
                w.Write(key.modifier);
                w.Write((byte)key.action);
                w.Write(key.noneKey);
            }

            // Save end position
            long endPos = w.Seek(0, SeekOrigin.Current);

            // Write count at the beginning
            w.Seek((int)beginPos, SeekOrigin.Begin);
            w.Write(count);

            // Seek back to end position
            w.Seek((int)endPos, SeekOrigin.Begin);
        }

        /// <summary>
        /// Load KeyBind data
        /// </summary>
        /// <param name="r">Binary Reader class</param>
        public static void Import(BinaryReader r)
        {
            r.ReadByte();
            int count = r.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                string keyId = r.ReadString();
                int keyCode = r.ReadInt32();
                byte modifier = r.ReadByte();
                ECombineKeyAction action = (ECombineKeyAction)r.ReadByte();
                bool noneKey = r.ReadBoolean();

                if (customKeys.TryGetValue(keyId, out var customKey))
                {
                    DSPGame.globalOption.overrideKeys[customKey.defaultBind.id] = new CombineKey(keyCode, modifier, action, noneKey);
                }
            }
        }
    }
}