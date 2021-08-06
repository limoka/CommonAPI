using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using Mono.Cecil;
using Mono.Cecil.Rocks;


namespace CommonAPI
{
    public static class Preloader
    {
        public static ManualLogSource logSource;
        
        public static void Initialize()
        {
            logSource = Logger.CreateLogSource("Common API Preloader");
        }
        // List of assemblies to patch
        // ReSharper disable once InconsistentNaming
        public static IEnumerable<string> TargetDLLs { get; } = new[] {"Assembly-CSharp.dll"};

        // Patches the assemblies
        public static void Patch(AssemblyDefinition assembly)
        {
            ModuleDefinition module = assembly.MainModule;
            TypeDefinition entityData = module.Types.First (t => t.FullName == "EntityData");
            TypeDefinition prefabDesc = module.Types.First (t => t.FullName == "PrefabDesc");

            bool flag = entityData == null || prefabDesc == null;
            if (flag)
            {
                logSource.LogInfo("Preloader patching failed!");
                return;
            }

            entityData.Fields.Add(new FieldDefinition("customId", FieldAttributes.Public, module.ImportReference(typeof(int))));
            entityData.Fields.Add(new FieldDefinition("customType", FieldAttributes.Public, module.ImportReference(typeof(int))));
            
            entityData.Fields.Add(new FieldDefinition("customData", FieldAttributes.Public,
                module.ImportReference(typeof(Dictionary<,>)).MakeGenericInstanceType(module.TypeSystem.String, module.TypeSystem.Object)));

            prefabDesc.Fields.Add(new FieldDefinition("customData", FieldAttributes.Public,
                module.ImportReference(typeof(Dictionary<,>)).MakeGenericInstanceType(module.TypeSystem.String, module.TypeSystem.Object)));

            logSource.LogInfo("Preloader patching is successful!");
        }
    }
}