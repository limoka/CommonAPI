using System;
using System.Collections.Generic;
using CommonAPI.Systems.UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CommonAPI.Systems
{
    public class UISignalPickerExtension
    {
        public static Func<int, bool> currentFilter;
        
        public static List<IPickerExtension<UISignalPicker>> extensions = new List<IPickerExtension<UISignalPicker>>();
        
        private static UIShowSignalTipExtension tipHandler;
        
        public static UISignalPicker PreparePicker()
        {
            PickerExtensionsSystem.Instance.ThrowIfNotLoaded();
            if (UIRoot.instance == null)
            {
                throw new PickerNotReadyException();
            }

            UISignalPicker signalPicker = UIRoot.instance.uiGame.signalPicker;
            if (!signalPicker.inited || signalPicker.active)
            {
                throw new PickerNotReadyException();
            }

            return signalPicker;
        }
        
        internal static UIShowSignalTipExtension GetTipExtension()
        {
            if (tipHandler == null)
            {
                tipHandler = new UIShowSignalTipExtension();
            }

            return tipHandler;
        }
        
        /// <summary>
        /// Open UIRecipePicker with custom filters
        /// </summary>
        /// <param name="pos">position on screen</param>
        /// <param name="_onReturn">callback to call, when user selects an item</param>
        /// <param name="filter">Filter function</param>
        public static void Popup(Vector2 pos, Action<int> _onReturn, Func<int, bool> filter)
        {
            PickerExtensionsSystem.Instance.ThrowIfNotLoaded();
            try
            {
                extensions.Clear();
                extensions.Add(GetTipExtension());
                
                UISignalPicker signalPicker = PreparePicker();
                if (signalPicker == null)
                {
                    _onReturn?.Invoke(0);
                }

                currentFilter = filter;

                signalPicker.onReturn = _onReturn;
                signalPicker._Open();
                signalPicker.pickerTrans.anchoredPosition = pos;
            }
            catch (PickerNotReadyException)
            {
                _onReturn?.Invoke(0);
            }
        }

        public static void Popup(Vector2 pos, IPickerExtension<UISignalPicker> extension)
        {
            PickerExtensionsSystem.Instance.ThrowIfNotLoaded();
            try
            {
                extensions.Clear();
                extensions.Add(GetTipExtension());
                extensions.Add(extension);
                UISignalPicker signalPicker = PreparePicker();

                extension.OnPopup(signalPicker);

                signalPicker._Open();
                signalPicker.pickerTrans.anchoredPosition = pos;

                extension.PostPopup(signalPicker);
            }
            catch (PickerNotReadyException) { }
        }
    }
}