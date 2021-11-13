using System;
using System.Reflection;
using BepInEx.Configuration;
using HarmonyLib;

namespace CommonAPI.Systems
{
    [AttributeUsage(AttributeTargets.Field)]
    public class UseConfigFile : Attribute
    {
        public string description;
        
        public UseConfigFile(){}

        public UseConfigFile(string description)
        {
            this.description = description;
        }
    }
    
    public abstract class ConfigurableDesc : CustomDesc
    {
        public int tier;
        
        public abstract string configCategory { get; }
        public abstract ConfigFile modConfig { get; }

        internal static MethodInfo bindMethod;

        static ConfigurableDesc()
        {
            MethodInfo[] methods = typeof(ConfigFile).GetMethods(BindingFlags.Instance | BindingFlags.Public);
            foreach (MethodInfo method in methods)
            {
                ParameterInfo[] parameters = method.GetParameters();
                if (method.Name != nameof(ConfigFile.Bind) || parameters.Length != 4) continue;
                
                if (parameters[0].ParameterType == typeof(string) && parameters[1].ParameterType == typeof(string)
                                                                  && parameters[3].ParameterType == typeof(string))
                {
                    bindMethod = method;
                    return;
                }
            }
            
            CommonAPIPlugin.logger.LogDebug("Failed to find MethodInfo for ConfigFile.Bind!");
            
        }
        
        public override void ApplyProperties(PrefabDesc desc)
        {
            FieldInfo[] fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (FieldInfo field in fields)
            {
                if (!Attribute.IsDefined(field, typeof(UseConfigFile))) continue;
                
                UseConfigFile[] attributes = (UseConfigFile[])field.GetCustomAttributes<UseConfigFile>(false);
                if (attributes.Length <= 0) continue;
                
                MethodInfo method = bindMethod.MakeGenericMethod(field.FieldType);
                object entry = method.Invoke(modConfig,
                    new[] {configCategory, $"Tier-{tier}_{field.Name}", field.GetValue(this), attributes[0].description});

                Type entryGenericType = typeof(ConfigEntry<>);
                Type entryType = entryGenericType.MakeGenericType(field.FieldType);
                        
                PropertyInfo valueProperty = entryType.GetProperty(nameof(ConfigEntry<int>.BoxedValue));

                object value = valueProperty.GetValue(entry);
                field.SetValue(this, value);
            }
        }
    }
}