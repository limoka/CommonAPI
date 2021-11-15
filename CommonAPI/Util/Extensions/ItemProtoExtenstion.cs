using UnityEngine;

namespace CommonAPI
{
    public static class ItemProtoExtenstion
    {
        public static void SetIcon(this ItemProto proto, string path, bool propageToRecipe = true)
        {
            if (string.IsNullOrEmpty(path)) return;

            Sprite sprite = Resources.Load<Sprite>(path);

            proto.IconPath = path;
            proto._iconSprite = sprite;

            if (propageToRecipe && proto.maincraft != null)
            {
                CommonAPIPlugin.logger.LogDebug("Setting recipe icon!");
                RecipeProto recipe = LDB.recipes.Select(proto.maincraft.ID);
                
                recipe.IconPath = "";
                recipe._iconSprite = sprite;
            }
        }
    }
}