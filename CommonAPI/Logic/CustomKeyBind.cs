using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace CommonAPI
{

    /// <summary>
    /// Default implementation for KeyBind press type.
    /// Defines what keypresses should be detected.
    /// Reacts only when key is pressed
    /// </summary>
    public class PressKeyBind
    {
        /// <summary>
        /// Is KeyBind activated?
        /// </summary>
        public bool keyValue
        {
            get
            {
                if (!VFInput.override_keys[defaultBind.id].IsNull())
                {
                    return ReadKey(VFInput.override_keys[defaultBind.id]);
                }

                return ReadDefaultKey();
            }
        }

        /// <summary>
        /// Default KeyBind
        /// </summary>
        public BuiltinKey defaultBind;

        public void Init(BuiltinKey defaultBind)
        {
            this.defaultBind = defaultBind;
        }

        /// <summary>
        /// Defines how this type of KeyBind should check default KeyBind
        /// </summary>
        /// <returns>If KeyBind is activated</returns>
        protected virtual bool ReadDefaultKey()
        {
            return ReadKey(defaultBind.key);
        }

        /// <summary>
        /// Defines how this type of KeyBind should check provided KeyBind
        /// </summary>
        /// <param name="key">Key to check</param>
        /// <returns>If KeyBind is activated</returns>
        protected virtual bool ReadKey(CombineKey key)
        {
            return key.GetKeyDown();
        }
    }

    /// <summary>
    /// Alternate implementation of KeyBind. Reacts only when key is held
    /// </summary>
    public class HoldKeyBind : PressKeyBind
    {
        protected override bool ReadKey(CombineKey key)
        {
            return key.GetKey();
        }
    }

    /// <summary>
    /// Alternate implementation of KeyBind. Reacts only when key is released
    /// </summary>
    public class ReleaseKeyBind : PressKeyBind
    {
        protected override bool ReadKey(CombineKey key)
        {
            return key.GetKeyUp();
        }
    }

    /// <summary>
    /// This class allows to define new KeyBinds and use them easily in your code
    /// </summary>
    [HarmonyPatch]
    public static class CustomKeyBind
    {
        // Conflict group bit usages by game's keybindings
        // Each bit defines a group where two keybinds cannot have same keys
        // Bits after 4096 are not used
        public const int MOVEMENT = 1;
        public const int UI = 2;
        public const int BUILD_MODE_1 = 4;
        public const int BUILD_MODE_2 = 8;
        public const int BUILD_MODE_3 = 16;
        public const int INVENTORY = 32;
        public const int CAMERA_1 = 64;
        public const int CAMERA_2 = 128;
        public const int FLYING = 256;
        public const int SAILING = 512;
        public const int EXTRA = 1024;
        
        //Defines whether keybind uses keyboard or mouse
        public const int KEYBOARD_KEYBIND = 2048;
        public const int MOUSE_KEYBIND = 4096;
        
        internal static Dictionary<string, PressKeyBind> customKeys = new Dictionary<string, PressKeyBind>();

        /// <summary>
        /// Register new KeyBind.
        /// </summary>
        /// <param name="key">Default value for new Keybind</param>
        /// <typeparam name="T">Key press type class. For example PressKeyBind</typeparam>
        public static void RegisterKeyBind<T>(BuiltinKey key) where T : PressKeyBind, new()
        {
            T keyBind = new T();
            keyBind.Init(key);
            customKeys.Add("KEY" + key.name, keyBind);
        }

        /// <summary>
        /// Checks if KeyBind with specified ID was registered
        /// </summary>
        /// <param name="id">KeyBind string ID</param>
        /// <returns>Does such KeyBind exist?</returns>
        public static bool HasKeyBind(string id)
        {
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
            string key = "KEY" + id;
            if (customKeys.ContainsKey(key))
            {
                return customKeys[key];
            }

            return null;
        }

        [HarmonyPatch(typeof(UIOptionWindow), "_OnCreate")]
        [HarmonyPrefix]
        private static void AddKeyBind(UIOptionWindow __instance)
        {
            PressKeyBind[] newKeys = customKeys.Values.ToArray();
            if (newKeys.Length == 0) return;

            int index = DSPGame.key.builtinKeys.Length;
            Array.Resize(ref DSPGame.key.builtinKeys, index + customKeys.Count);

            for (int i = 0; i < newKeys.Length; i++)
            {
                DSPGame.key.builtinKeys[index + i] = newKeys[i].defaultBind;
            }
        }
    }
}