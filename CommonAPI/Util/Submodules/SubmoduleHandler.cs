#nullable enable
using BepInEx.Logging;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;

#pragma warning disable 649

namespace CommonAPI {
    
    internal class SubmoduleHandler {
        private readonly Version currentBuild;
        private readonly ManualLogSource logger;
        
        internal readonly Dictionary<Type, BaseSubmodule> allModules;
        private readonly HashSet<string> loadedModules;

        internal SubmoduleHandler(Version currentBuild, ManualLogSource logger) {
            this.currentBuild = currentBuild;
            this.logger = logger;
            loadedModules = new HashSet<string>();
            
            allModules = GetSubmodules();
        }
        
        internal T? GetModuleInstance<T>()
            where T : BaseSubmodule
        {
            if (allModules.TryGetValue(typeof(T), out BaseSubmodule submodule))
            {
                return (T)submodule;
            }
            
            return null;
        }

        /// <summary>
        /// Return true if the specified submodule is loaded.
        /// </summary>
        /// <param name="submodule">nameof the submodule</param>
        public bool IsLoaded(string submodule) => loadedModules.Contains(submodule);

        /// <summary>
        /// Load submodule
        /// </summary>
        /// <param name="moduleType">Module type</param>
        /// <returns>Is loading successful?</returns>
        public bool RequestModuleLoad(Type moduleType)
        {
            return RequestModuleLoad(moduleType, false);
        }

        private bool RequestModuleLoad(Type moduleType, bool ignoreDependencies)
        {
            if (moduleType == null)
                throw new ArgumentNullException(nameof(moduleType));
            
            if (!allModules.ContainsKey(moduleType))
                throw new InvalidOperationException($"Tried to load unknown submodule: '{moduleType.FullName}'!");

            string moduleName = moduleType.Name;
            if (IsLoaded(moduleName)) return true;
            
            CommonAPIPlugin.logger.LogInfo($"Enabling CommonAPI Submodule: {moduleName}");

            try
            {
                if (!ignoreDependencies)
                {
                    var dependencies = GetModuleDependencies(moduleType);
                    foreach (Type dependency in dependencies)
                    {
                        if (dependency == moduleType) continue;
                        if (!RequestModuleLoad(dependency, true))
                        {
                            logger.LogError($"{moduleName} could not be initialized because one of it's dependencies failed to load.");
                        }
                    }
                }

                BaseSubmodule submodule = allModules[moduleType];

                if (!submodule.Build.Equals(new Version()) &&
                    !submodule.Build.CompatibleWith(currentBuild))
                {
                    logger.LogWarning($"Submodule {moduleName} was built for {submodule.Build}, but current build is {currentBuild}.");
                }

                submodule.SetHooks();
                submodule.Load();

                submodule.Loaded = true;
                loadedModules.Add(moduleName);

                submodule.PostLoad();
                return true;
            }
            catch (Exception e)
            {
                logger.LogError($"{moduleName} could not be initialized and has been disabled:\n{e}");
            }

            return false;
        }
        
        private IEnumerable<Type> GetModuleDependencies(Type moduleType)
        {
            IEnumerable<Type> modulesToAdd = moduleType.GetDependants(type =>
                {
                    BaseSubmodule submodule = allModules[type];
                    return submodule.Dependencies.AddRangeToArray(submodule.GetOptionalDependencies());
                },
                (start, end) =>
                {
                    logger.LogWarning($"Found Submodule circular dependency! Submodule {start.FullName} depends on {end.FullName}, which depends on {start.FullName}! Submodule {start.FullName} and all of its dependencies will not be loaded.");
                });
            return modulesToAdd;
        }
        
        internal HashSet<string> LoadRequested(PluginScanner pluginScanner)
        {
            List<Type> modulesToLoad = new List<Type>();

            void AddModuleToSet(IEnumerable<CustomAttributeArgument> arguments) {
                foreach (var arg in arguments) {
                    foreach (var stringElement in (CustomAttributeArgument[])arg.Value)
                    {
                        string moduleName = (string) stringElement.Value;
                        Type moduleType = allModules.First(pair => pair.Key.Name.Equals(moduleName)).Key;
                        
                        IEnumerable<Type> modulesToAdd = moduleType.GetDependants(type =>
                            {
                                BaseSubmodule submodule = allModules[type];
                                return submodule.Dependencies.AddRangeToArray(submodule.GetOptionalDependencies());
                            }, (start, end) => 
                            {
                                CommonAPIPlugin.logger.LogWarning($"Found Submodule circular dependency! Submodule {start.FullName} depends on {end.FullName}, which depends on {start.FullName}! Submodule {start.FullName} and all of its dependencies will not be loaded.");
                            });

                        foreach (Type module in modulesToAdd)
                        {
                            modulesToLoad.Add(module);
                        }
                    }
                }
            }

            void CallWhenAssembliesAreScanned()
            {
                foreach (var moduleType in modulesToLoad)
                {
                    RequestModuleLoad(moduleType, true);
                }
            }

            var scanRequest = new PluginScanner.AttributeScanRequest(typeof(CommonAPISubmoduleDependency).FullName,
                AttributeTargets.Assembly | AttributeTargets.Class,
                CallWhenAssembliesAreScanned, false,
                (assembly, arguments) =>
                    AddModuleToSet(arguments),
                (type, arguments) =>
                    AddModuleToSet(arguments)
                );

            pluginScanner.AddScanRequest(scanRequest);

            return loadedModules;
        }

        private Dictionary<Type, BaseSubmodule> GetSubmodules()
        {
            Type[] types;
            try
            {
                types = Assembly.GetExecutingAssembly().GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types;
            }

            Dictionary<Type, BaseSubmodule> modules = new Dictionary<Type, BaseSubmodule>();

            var moduleTypes = types.Where(IsSubmodule);
            foreach (Type moduleType in moduleTypes)
            {
                modules.Add(moduleType, (BaseSubmodule)Activator.CreateInstance(moduleType));
            }
            
            return modules;
        }
        
        private static bool IsSubmodule(Type type)
        {
            return typeof(BaseSubmodule).IsAssignableFrom(type) &&
                   type != typeof(BaseSubmodule);
        }
    }
}
