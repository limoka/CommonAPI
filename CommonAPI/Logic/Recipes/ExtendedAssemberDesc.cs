using UnityEngine;

namespace CommonAPI
{
    public class ExtendedAssemberDesc : CustomDesc
    {
        public string recipeType;
        
        public float speed = 1f;
        
        public override void ApplyProperties(PrefabDesc desc)
        {
            desc.assemblerSpeed = Mathf.RoundToInt(speed * 10000f);
            desc.assemblerRecipeType = (ERecipeType)20;
            desc.isAssembler = (desc.assemblerRecipeType > ERecipeType.None);
        }
    }
}