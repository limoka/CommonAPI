using System;
using UnityEngine;

namespace CommonAPI.Systems
{
    public class UIRecipePickerExtension
    {
        public static Func<RecipeProto, bool> currentFilter;
        public static bool showLocked = false;
        
        /// <summary>
        /// Open UIRecipePicker with custom filters
        /// </summary>
        /// <param name="pos">position on screen</param>
        /// <param name="_onReturn">callback to call, when user selects an item</param>
        /// <param name="showLockedRecipes">Should locked items be visible</param>
        /// <param name="filter">Filter function</param>
        public static void Popup(Vector2 pos, Action<RecipeProto> _onReturn, bool showLockedRecipes, Func<RecipeProto, bool> filter)
        {
            PickerExtensionsSystem.ThrowIfNotLoaded();
            showLocked = showLockedRecipes;
            currentFilter = filter;
            if (UIRoot.instance == null)
            {
                _onReturn?.Invoke(null);
                return;
            }

            UIRecipePicker recipePicker = UIRoot.instance.uiGame.recipePicker;
            if (!recipePicker.inited || recipePicker.active)
            {
                _onReturn?.Invoke(null);
                return;
            }

            recipePicker.filter = ERecipeType.None;
            recipePicker.onReturn = _onReturn;
            recipePicker._Open();
            recipePicker.pickerTrans.anchoredPosition = pos;
        }
        
        /// <summary>
        /// Open UIRecipePicker with custom filters
        /// </summary>
        /// <param name="pos">position on screen</param>
        /// <param name="_onReturn">callback to call, when user selects an item</param>
        /// <param name="filter">Filter function</param>
        public static void Popup(Vector2 pos, Action<RecipeProto> _onReturn, Func<RecipeProto, bool> filter)
        {
            Popup(pos, _onReturn, false, filter);
        }
    }
}