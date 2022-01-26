using System;
using System.Collections.Generic;
using System.Reflection;
using CommonAPI.Patches;

namespace CommonAPI.Systems
{
    [CommonAPISubmodule(Dependencies = new []{typeof(PickerExtensionsSystem), typeof(CustomDescSystem), typeof(ProtoRegistry)})]
    public static class AssemblerRecipeSystem
    {
        internal static Registry recipeTypes = new Registry();
        internal static List<List<int>> recipeTypeLists = new List<List<int>>();
        
        public static bool Loaded {
            get => _loaded;
            internal set => _loaded = value;
        }

        private static bool _loaded;


        [CommonAPISubmoduleInit(Stage = InitStage.SetHooks)]
        internal static void SetHooks()
        {
            CommonAPIPlugin.harmony.PatchAll(typeof(AssemblerComponentPatch));
            CommonAPIPlugin.harmony.PatchAll(typeof(UIAssemblerWindowPatch));
        }


        [CommonAPISubmoduleInit(Stage = InitStage.Load)]
        internal static void load()
        {
            CommonAPIPlugin.registries.Add($"{ CommonAPIPlugin.ID}:RecipeTypeRegistry", recipeTypes);
            recipeTypeLists.Add(null);
        }
        
        internal static void ThrowIfNotLoaded()
        {
            if (!Loaded)
            {
                Type submoduleType = MethodBase.GetCurrentMethod().DeclaringType;
                string message = $"{submoduleType.Name} is not loaded. Please use [{nameof(CommonAPISubmoduleDependency)}(nameof({submoduleType.Name})]";
                throw new InvalidOperationException(message);
            }
        }

        public static bool IsRecipeTypeRegistered(int type)
        {
            ThrowIfNotLoaded();
            return type < recipeTypeLists.Count;
        }
        
        /// <summary>
        /// Register new recipe type. This can be used to create new machine types independent of vanilla machines.
        /// </summary>
        /// <param name="typeId">Unique string ID</param>
        /// <returns>Assigned integer ID</returns>
        public static int RegisterRecipeType(string typeId)
        {
            ThrowIfNotLoaded();
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
            ThrowIfNotLoaded();
            recipeTypeLists[type].Add(recipe.ID);
            Algorithms.ListSortedAdd(recipeTypeLists[type], recipe.ID);
        }

        /// <summary>
        /// Checks if provided <see cref="RecipeProto"/> belongs to recipe type
        /// </summary>
        /// <param name="proto">Recipe</param>
        /// <param name="typeId">Integer ID</param>
        /// <returns></returns>
        public static bool BelongsToType(this RecipeProto proto, int typeId)
        {
            ThrowIfNotLoaded();
            if (typeId >= recipeTypeLists.Count) return false;

            return recipeTypeLists[typeId].BinarySearch(proto.ID) >= 0;
        }
    }
}