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
    [CommonAPISubmodule]
    public static class CustomKeyBindSystem
    {

        internal static Dictionary<string, PressKeyBind> customKeys = new Dictionary<string, PressKeyBind>();
        internal static Registry keyRegistry;
        
        /// <summary>
        /// Return true if the submodule is loaded.
        /// </summary>
        public static bool Loaded {
            get => _loaded;
            internal set => _loaded = value;
        }

        private static bool _loaded;


        [CommonAPISubmoduleInit(Stage = InitStage.SetHooks)]
        internal static void SetHooks()
        {
            CommonAPIPlugin.harmony.PatchAll(typeof(KeyBindPatches));
            CommonAPIPlugin.harmony.PatchAll(typeof(Chainloader_Patch));
        }
        
        [CommonAPISubmoduleInit(Stage = InitStage.Load)]
        internal static void Load()
        {
            keyRegistry = new Registry(100);
        }
        
        internal static void ThrowIfNotLoaded()
        {
            if (!Loaded)
            {
                throw new InvalidOperationException(
                    $"{nameof(CustomKeyBindSystem)} is not loaded. Please use [{nameof(CommonAPISubmoduleDependency)}(nameof({nameof(CustomKeyBindSystem)})]");
            }
        }

        /// <summary>
        /// Register new KeyBind.
        /// </summary>
        /// <param name="key">Default value for new Keybind</param>
        /// <typeparam name="T">Key press type class. For example PressKeyBind</typeparam>
        public static void RegisterKeyBind<T>(BuiltinKey key) where T : PressKeyBind, new()
        {
            ThrowIfNotLoaded();
            
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
            ThrowIfNotLoaded();
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
            ThrowIfNotLoaded();
            string key = "KEY" + id;
            if (customKeys.ContainsKey(key))
            {
                return customKeys[key];
            }

            return null;
        }
    }
}