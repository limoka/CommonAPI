using UnityEngine;

namespace CommonAPI
{
    public static class ItemProtoExtenstion
    {
        public static void SetIcon(this ItemProto proto, string path, bool propageToRecipe = true)
        {
            if (string.IsNullOrEmpty(path)) return;
            
            proto.IconPath = path;
            proto._iconSprite = Resources.Load<Sprite>(path);
        }
    }
}