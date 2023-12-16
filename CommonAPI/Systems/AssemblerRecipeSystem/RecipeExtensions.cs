namespace CommonAPI.Systems
{
    public static class RecipeExtensions
    {
        /// <summary>
        /// Checks if provided <see cref="RecipeProto"/> belongs to recipe type
        /// </summary>
        /// <param name="proto">Recipe</param>
        /// <param name="typeId">Integer ID</param>
        /// <returns></returns>
        public static bool BelongsToType(this RecipeProto proto, int typeId)
        {
            AssemblerRecipeSystem.Instance.ThrowIfNotLoaded();
            if (typeId >= AssemblerRecipeSystem.recipeTypeLists.Count) return false;

            return AssemblerRecipeSystem.recipeTypeLists[typeId].BinarySearch(proto.ID) >= 0;
        }
    }
}