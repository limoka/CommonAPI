using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Configuration;
using HarmonyLib;

namespace CommonAPI
{
    public static class ConfigFileExtension
    {
        public static Type configFile;
        public static PropertyInfo OrphanedEntriesProp;
        
        static ConfigFileExtension()
        {
            configFile = AccessTools.TypeByName("BepInEx.Configuration.ConfigFile");
            OrphanedEntriesProp = configFile.GetProperty("OrphanedEntries", AccessTools.all);
        }

        /// <summary>
        /// Migrate BepInEx config file entries from one category to another.
        /// </summary>
        /// <param name="file">target BepInEx config file</param>
        /// <param name="oldSection">Section to migrate from</param>
        /// <param name="newSection">Section to migrate to</param>
        /// <param name="keyFilter">Array of keys to migrate</param>
        /// <typeparam name="T">Type of config entries</typeparam>
        public static void MigrateConfig<T>(this ConfigFile file, string oldSection, string newSection, string[] keyFilter)
        {
            Dictionary<ConfigDefinition, string> oldEntries = (Dictionary<ConfigDefinition, string>)OrphanedEntriesProp.GetValue(file);
            List<ConfigDefinition> keysToRemove = new List<ConfigDefinition>();

            foreach (var kv in oldEntries)
            {
                string key = kv.Key.Key;
                if (kv.Key.Section.Equals(oldSection) && ((IList) keyFilter).Contains(key))
                {
                    if (!file.TryGetEntry(newSection, key, out ConfigEntry<T> entry)) continue;

                    entry.SetSerializedValue(kv.Value);
                    keysToRemove.Add(kv.Key);
                    CommonAPIPlugin.logger.LogInfo($"Migrating config from {oldSection}:{key} to {newSection}:{key}");
                    
                }
            }

            foreach (var key in keysToRemove)
            {
                oldEntries.Remove(key);
            }
        }

        /// <summary>
        /// Migrate BepInEx config file entry.
        /// </summary>
        /// <param name="file">target BepInEx config file</param>
        /// <param name="oldSection">Section to migrate from</param>
        /// <param name="oldName">Old entry key</param>
        /// <param name="newSection">Section to migrate to</param>
        /// <param name="newName">New entry key</param>
        /// <typeparam name="T">Type of config entry</typeparam>
        public static void MigrateConfig<T>(this ConfigFile file, string oldSection, string oldName, string newSection, string newName)
        {
            Dictionary<ConfigDefinition, string> oldEntries = (Dictionary<ConfigDefinition, string>)OrphanedEntriesProp.GetValue(file);
            List<ConfigDefinition> keysToRemove = new List<ConfigDefinition>();

            foreach (var kv in oldEntries)
            {
                ConfigDefinition config = kv.Key;
                if (config.Section.Equals(oldSection) && config.Key.Equals(oldName))
                {
                    if (!file.TryGetEntry(newSection, newName, out ConfigEntry<T> entry)) continue;

                    entry.SetSerializedValue(kv.Value);
                    keysToRemove.Add(config);
                    CommonAPIPlugin.logger.LogInfo($"Migrating config from {oldSection}:{oldName} to {newSection}:{newName}");
                    
                }
            }

            foreach (var key in keysToRemove)
            {
                oldEntries.Remove(key);
            }
        }
    }
}