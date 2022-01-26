using System;
using UnityEngine;

namespace CommonAPI.Systems
{
    public class ExtendedAssemberDesc : CustomDesc
    {
        public const string RECIPE_TYPE_NAME = CommonAPIPlugin.ID + ":recipeType";
        
        public string recipeType;
        
        public float speed = 1f;
        
        public override void ApplyProperties(PrefabDesc desc)
        {
            AssemblerRecipeSystem.ThrowIfNotLoaded();
            desc.assemblerSpeed = Mathf.RoundToInt(speed * 10000f);
            desc.assemblerRecipeType = ERecipeType.Custom;
            desc.SetProperty(RECIPE_TYPE_NAME, AssemblerRecipeSystem.recipeTypes.GetUniqueId(recipeType));
            
            desc.isAssembler = (desc.assemblerRecipeType > ERecipeType.None);
        }
    }
}