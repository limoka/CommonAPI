using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace CommonAPI
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RegisterPatch : Attribute
    {
        public string typeKey;

        public RegisterPatch(string typeKey)
        {
            this.typeKey = typeKey;
        }
    }
    
    public static class HarmonyRegisterExtension
    {
        public static IEnumerable<Type> GetTypesWithAttributeInAssembly<T>(Assembly assembly) where T : Attribute
        {
            return assembly.GetTypes().Where(t => t.GetCustomAttributes(typeof(T), true).Length > 0);
        }
        
        public static void PatchAll(this Harmony harmony, string typeKey)
        {
            Assembly assembly = Assembly.GetCallingAssembly();
            var types = GetTypesWithAttributeInAssembly<RegisterPatch>(assembly);
            foreach (Type type in types)
            {
                if (type.IsClass)
                {
                    RegisterPatch attribute = type.GetCustomAttribute<RegisterPatch>();
                    if (attribute.typeKey.Equals(typeKey))
                    {
                        harmony.PatchAll(type);
                    }
                }
                else
                {
                    CommonAPIPlugin.logger.LogInfo($"Failed to patch: {type.FullName}.");
                }
            }
        }
    }
}