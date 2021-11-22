using System;
using UnityEngine;

namespace CommonAPI.Systems
{
    public class UIRecipePickerExtension
    {
        public static Func<RecipeProto, bool> currentFilter;
        
        public static void Popup(Vector2 pos, Action<RecipeProto> _onReturn, Func<RecipeProto, bool> filter)
        {
            PickerExtensionsSystem.ThrowIfNotLoaded();
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
    }
}