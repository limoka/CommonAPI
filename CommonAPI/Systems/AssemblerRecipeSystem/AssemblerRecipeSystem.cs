using System;
using System.Collections.Generic;
using System.Reflection;
using CommonAPI.Patches;

namespace CommonAPI.Systems
{
    public class AssemblerRecipeSystem : BaseSubmodule
    {
        internal static Registry recipeTypes = new Registry();
        internal static List<List<int>> recipeTypeLists = new List<List<int>>();


        internal static AssemblerRecipeSystem Instance => CommonAPIPlugin.GetModuleInstance<AssemblerRecipeSystem>();

        internal override Type[] Dependencies => new[] { typeof(PickerExtensionsSystem), typeof(CustomDescSystem), typeof(ProtoRegistry) };

        internal override void SetHooks()
        {
            CommonAPIPlugin.harmony.PatchAll(typeof(AssemblerComponentPatch));
            CommonAPIPlugin.harmony.PatchAll(typeof(UIAssemblerWindowPatch));
        }

        
        internal override void Load()
        {
            CommonAPIPlugin.registries.Add($"{ CommonAPIPlugin.ID}:RecipeTypeRegistry", recipeTypes);
            recipeTypeLists.Add(null);
        }

        public static bool IsRecipeTypeRegistered(int type)
        {
            Instance.ThrowIfNotLoaded();
            return type < recipeTypeLists.Count;
        }
        
        /// <summary>
        /// Register new recipe type. This can be used to create new machine types independent of vanilla machines.
        /// </summary>
        /// <param name="typeId">Unique string ID</param>
        /// <returns>Assigned integer ID</returns>
        public static int RegisterRecipeType(string typeId)
        {
            Instance.ThrowIfNotLoaded();
            int id = recipeTypes.Register(typeId);
            if (id >= recipeTypeLists.Capacity)
            {
                recipeTypeLists.Capacity *= 2;
            }

            recipeTypeLists.Add(new List<int>());
            return id;
        }

        internal static void BindRecipeToType(RecipeProto recipe, int type)
        {
            Instance.ThrowIfNotLoaded();
            recipeTypeLists[type].Add(recipe.ID);
            Algorithms.ListSortedAdd(recipeTypeLists[type], recipe.ID);
        }
    }
}