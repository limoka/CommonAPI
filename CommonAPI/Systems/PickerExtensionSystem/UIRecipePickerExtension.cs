using System;
using UnityEngine;

namespace CommonAPI.Systems
{
    public class UIRecipePickerExtension
    {
        public static Func<RecipeProto, bool> currentFilter;
        public static bool showLocked = false;
        
        public static IPickerExtension<UIRecipePicker> currentExtension;
        
        
        public static UIRecipePicker PreparePicker()
        {
            PickerExtensionsSystem.ThrowIfNotLoaded();
            if (UIRoot.instance == null)
            {
                throw new PickerNotReadyException();
            }

            UIRecipePicker recipePicker = UIRoot.instance.uiGame.recipePicker;
            if (!recipePicker.inited || recipePicker.active)
            {
                throw new PickerNotReadyException();
            }

            return recipePicker;
        }
        
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
            try
            {
                showLocked = showLockedRecipes;
                currentExtension = null;
                UIRecipePicker recipePicker = PreparePicker();
                if (recipePicker == null)
                {
                    _onReturn?.Invoke(null);
                }

                currentFilter = filter;

                recipePicker.filter = ERecipeType.None;
                recipePicker.onReturn = _onReturn;
                recipePicker._Open();
                recipePicker.pickerTrans.anchoredPosition = pos;
            }
            catch (PickerNotReadyException)
            {
                _onReturn?.Invoke(null);
            }
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
        
        public static void Popup(Vector2 pos, IPickerExtension<UIRecipePicker> extension)
        {
            PickerExtensionsSystem.ThrowIfNotLoaded();
            try
            {
                // ReSharper disable once SuspiciousTypeConversion.Global
                showLocked = extension is ShowLocked;
                currentExtension = extension;
                UIRecipePicker recipePicker = PreparePicker();

                extension.OnPopup(recipePicker);

                recipePicker._Open();
                recipePicker.pickerTrans.anchoredPosition = pos;

                extension.PostPopup(recipePicker);
            }
            catch (PickerNotReadyException) { }
        }
    }
}