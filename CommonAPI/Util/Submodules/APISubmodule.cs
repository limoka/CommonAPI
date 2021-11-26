#nullable enable
using BepInEx.Logging;
using JetBrains.Annotations;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#pragma warning disable 649

// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace CommonAPI {
    
// Source code is taken from R2API: https://github.com/risk-of-thunder/R2API/tree/master

    
    [Flags]
    internal enum InitStage {
        SetHooks = 1 << 0,
        Load = 1 << 1,
        Unload = 1 << 2,
        UnsetHooks = 1 << 3,
        LoadCheck = 1 << 4,
    }

    // ReSharper disable once InconsistentNaming
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Class)]
    internal class CommonAPISubmodule : Attribute {
        public Version Build;
    }

    // ReSharper disable once InconsistentNaming
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method)]
    internal class CommonAPISubmoduleInit : Attribute {
        public InitStage Stage;
    }

    /// <summary>
    /// Attribute to have at the top of your BaseUnityPlugin class if you want to load a specific R2API Submodule.
    /// Parameter(s) are the nameof the submodules.
    /// e.g: [CommonAPISubmoduleDependency("", "")]
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class CommonAPISubmoduleDependency : Attribute {
        public string?[]? SubmoduleNames { get; }

        public CommonAPISubmoduleDependency(params string[] submoduleName) {
            SubmoduleNames = submoduleName;
        }
    }

    // ReSharper disable once InconsistentNaming
    /// <summary>
    ///
    /// </summary>
    public class APISubmoduleHandler {
        private readonly Version _build;
        private readonly ManualLogSource logger;
        private readonly HashSet<string> moduleSet;
        private static readonly HashSet<string> loadedModules;

        static APISubmoduleHandler()
        {
            loadedModules = new HashSet<string>();
        }

        internal APISubmoduleHandler(Version build, ManualLogSource logger) {
            _build = build;
            this.logger = logger;
            moduleSet = new HashSet<string>();
        }

        /// <summary>
        /// Return true if the specified submodule is loaded.
        /// </summary>
        /// <param name="submodule">nameof the submodule</param>
        public static bool IsLoaded(string submodule) => loadedModules.Contains(submodule);

        internal HashSet<string> LoadRequested(PluginScanner pluginScanner) {

            void AddModuleToSet(IEnumerable<CustomAttributeArgument> arguments) {
                foreach (var arg in arguments) {
                    foreach (var stringElement in (CustomAttributeArgument[])arg.Value) {
                        moduleSet.Add((string)stringElement.Value);
                    }
                }
            }

            void CallWhenAssembliesAreScanned()
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


                var moduleTypes = types.Where(APISubmoduleFilter).ToList();

                foreach (var moduleType in moduleTypes) {
                    CommonAPIPlugin.logger.LogInfo($"Enabling CommonAPI Submodule: {moduleType.Name}");
                }

                var faults = new Dictionary<Type, Exception>();

                moduleTypes
                    .ForEachTry<Type>(t => InvokeStage(t, InitStage.SetHooks, null), faults);
                moduleTypes.Where(t => !faults.ContainsKey(t))
                    .ForEachTry(t => InvokeStage(t, InitStage.Load, null), faults);

                faults.Keys.ForEachTry(t => {
                    logger.Log(LogLevel.Error, $"{t.Name} could not be initialized and has been disabled:\n\n{faults[t]}");
                    InvokeStage(t, InitStage.UnsetHooks, null);
                });

                moduleTypes.Where(t => !faults.ContainsKey(t))
                    .ForEachTry(t => t.SetFieldValue("_loaded", true));
                moduleTypes.Where(t => !faults.ContainsKey(t))
                    .ForEachTry(t => loadedModules.Add(t.Name));
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

        // ReSharper disable once InconsistentNaming
        private bool APISubmoduleFilter(Type type)
        {
            if (type == null) return false;
            var attr = type.GetCustomAttribute<CommonAPISubmodule>();

            if (attr == null)
                return false;

            /*if (R2API.DebugMode) {
                return true;
            }*/

            // Comment this out if you want to try every submodules working (or not) state
            if (!moduleSet.Contains(type.Name)) {
                var shouldload = new object[1];
                InvokeStage(type, InitStage.LoadCheck, shouldload);
                if (!(shouldload[0] is bool)) {
                    return false;
                }

                if (!(bool)shouldload[0]) {
                    return false;
                }
            }

            if (attr.Build != default && attr.Build != _build)
                logger.Log(LogLevel.Debug,
                    $"{type.Name} was built for build {attr.Build}, current build is {_build}.");

            return true;
        }

        private void InvokeStage(Type type, InitStage stage, object[]? parameters) {
            var method = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(m => m.GetCustomAttributes(typeof(CommonAPISubmoduleInit))
                .Any(a => ((CommonAPISubmoduleInit)a).Stage.HasFlag(stage))).ToList();

            if (method.Count == 0) {
                logger.Log(LogLevel.Debug, $"{type.Name} has no static method registered for {stage}");
                return;
            }

            method.ForEach(m => m.Invoke(null, parameters));
        }
    }

    public static class EnumerableExtensions {

        /// <summary>
        /// ForEach but with a try catch in it.
        /// </summary>
        /// <param name="list">the enumerable object</param>
        /// <param name="action">the action to do on it</param>
        /// <param name="exceptions">the exception dictionary that will get filled, null by default if you simply want to silence the errors if any pop.</param>
        /// <typeparam name="T"></typeparam>
        public static void ForEachTry<T>(this IEnumerable<T> list, Action<T> action, IDictionary<T, Exception> exceptions = null!) {
            list.ToList().ForEach(element => {
                try {
                    action.Invoke(element);
                }
                catch (Exception exception) {
                    exceptions.Add(element, exception);
                }
            });
        }
    }
}
