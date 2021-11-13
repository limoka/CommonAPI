using System;
using UnityEngine;

namespace CommonAPI.Systems
{
    public class PickerNotReadyException : Exception
    {
        public PickerNotReadyException() : base("UIItemPicker is not ready!") { }
    }
    
    public class UIItemPickerExtension
    {
        public static Func<ItemProto, bool> currentFilter;

        public static IItemPickerExtension currentExtension;
        
        
        public static UIItemPicker PreparePicker()
        {
            PickerExtensionsSystem.ThrowIfNotLoaded();
            if (UIRoot.instance == null)
            {
                throw new PickerNotReadyException();
            }

            UIItemPicker itemPicker = UIRoot.instance.uiGame.itemPicker;
            if (!itemPicker.inited || itemPicker.active)
            {
                throw new PickerNotReadyException();
            }

            return itemPicker;
        }

        public static void Popup(Vector2 pos, Action<ItemProto> _onReturn, Func<ItemProto, bool> filter)
        {
            PickerExtensionsSystem.ThrowIfNotLoaded();
            try
            {
                currentExtension = null;
                UIItemPicker itemPicker = PreparePicker();
                if (itemPicker == null)
                {
                    _onReturn?.Invoke(null);
                }

                currentFilter = filter;

                itemPicker.onReturn = _onReturn;
                itemPicker._Open();
                itemPicker.pickerTrans.anchoredPosition = pos;
            }
            catch (PickerNotReadyException)
            {
                _onReturn?.Invoke(null);
            }
        }

        public static void Popup(Vector2 pos, IItemPickerExtension extension)
        {
            PickerExtensionsSystem.ThrowIfNotLoaded();
            try
            {
                currentExtension = extension;
                UIItemPicker itemPicker = PreparePicker();

                extension.OnPopup(itemPicker);

                itemPicker._Open();
                itemPicker.pickerTrans.anchoredPosition = pos;

                extension.PostPopup(itemPicker);
            }
            catch (PickerNotReadyException) { }
        }
    }
}