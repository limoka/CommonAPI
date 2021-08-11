using System;
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
            
            TypeDefinition recipeType = module.Types.First (t => t.FullName == "ERecipeType");

            bool flag = entityData == null || prefabDesc == null || recipeType == null;
            if (flag)
            {
                logSource.LogInfo("Preloader patching failed!");
                return;
            }

            //FieldDefinition field = new FieldDefinition("customType", FieldAttributes.Public, module.ImportReference(typeof(int)));
            //recipeProto.Fields.Add(field);
            //var ca_2 = new CustomAttribute(assembly.MainModule.ImportReference(typeof(NonSerializedAttribute).GetConstructor(new Type[] {})));
            //field.CustomAttributes.Add(ca_2);
            
            var enumCustomValue = new FieldDefinition("Custom", FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.Public | FieldAttributes.HasDefault, recipeType) { Constant = 20 };
            recipeType.Fields.Add(enumCustomValue);
            
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