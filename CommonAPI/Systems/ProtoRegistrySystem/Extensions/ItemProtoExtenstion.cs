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
                RecipeProto recipe = LDB.recipes.Select(proto.maincraft.ID);
                CommonAPIPlugin.logger.LogInfo($"Changing recipe icon: {recipe != null}");
                
                recipe.IconPath = "";
                recipe._iconSprite = sprite;
            }
        }
    }
}