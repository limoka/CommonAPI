using System;
using System.Collections.Generic;
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
            keyRegistry = new Registry(100);
        }

        /// <summary>
        /// Register new KeyBind.
        /// </summary>
        /// <param name="key">Default value for new Keybind</param>
        /// <typeparam name="T">Key press type class. For example PressKeyBind</typeparam>
        public static void RegisterKeyBind<T>(BuiltinKey key) where T : PressKeyBind, new()
        {
            Instance.ThrowIfNotLoaded();
            
            string id = "KEY" + key.name;
            int index = keyRegistry.Register(id);
            key.id = index;
            
            T keyBind = new T();
            keyBind.Init(key);

            customKeys.Add(id, keyBind);
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
    }
}