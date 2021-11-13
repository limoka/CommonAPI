using CommonAPI.Systems;
using HarmonyLib;

namespace CommonAPI.Patches
{
    [HarmonyPatch]
    public static class AssemblerComponentPatch
    {
        [HarmonyPatch(typeof(AssemblerComponent), "SetRecipe")]
        [HarmonyPrefix]
        public static bool CheckRecipe(AssemblerComponent __instance, int recpId, SignData[] signPool)
        {
            if (recpId > 0)
            {
                RecipeProto recipeProto = LDB.recipes.Select(recpId);
                if (recipeProto.Type == ERecipeType.Custom)
                {
                    int protoId = GameMain.localPlanet.factory.entityPool[__instance.entityId].protoId;
                    int recipeType = LDB.items.Select(protoId).prefabDesc.GetProperty<int>(ExtendedAssemberDesc.RECIPE_TYPE_NAME);
                    if (!recipeProto.BelongsToType(recipeType))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}