using System;
using UnityEngine;
using UnityEngine.UI;

namespace CommonAPI.Systems
{
    public class UINumberPickerExtension : MonoBehaviour, IMouseHandlerExtension<UISignalPicker>
    {
        public Slider slider;
        public InputField field;
        public int currentValue { get; private set; } = 0;

        private bool ignoreEvents;

        public bool requestBoth;

        public int lastSelectedIndex;

        public Action<int> onReturnSignal;
        public Action<int> onReturnCount;
        public Action<int, int> onReturnBoth;

        public int previousSignal;

        private static UINumberPickerExtension numberPanel;


        public static UINumberPickerExtension GetExtension()
        {
            PickerExtensionsSystem.Instance.ThrowIfNotLoaded();
            UISignalPicker picker = UISignalPickerExtension.PreparePicker();

            if (numberPanel == null)
            {
                GameObject prefab = CommonAPIPlugin.resource.bundle.LoadAsset<GameObject>("Assets/CommonAPI/UI/number-panel.prefab");
                GameObject obj = Instantiate(prefab, picker.transform, false);
                numberPanel = obj.GetComponent<UINumberPickerExtension>();
            }

            return numberPanel;
        }

        public static void Popup(Vector2 pos, Action<int> onItem, Action<int> onCount, int currentValue = 0, Func<int, bool> filterFunc = null)
        {
            PickerExtensionsSystem.Instance.ThrowIfNotLoaded();
            UINumberPickerExtension extension = GetExtension();

            extension.onReturnSignal = onItem;
            extension.onReturnCount = onCount;
            extension.currentValue = currentValue;
            UISignalPickerExtension.currentFilter = filterFunc;

            UISignalPickerExtension.Popup(pos, extension);
        }

        public static void Popup(Vector2 pos, Action<int, int> onBoth, int currentValue = 0, int currentItem = 0, Func<int, bool> filterFunc = null) 
        {
            PickerExtensionsSystem.Instance.ThrowIfNotLoaded();
            UINumberPickerExtension extension = GetExtension();

            extension.onReturnSignal = null;
            extension.onReturnCount = null;
            extension.onReturnBoth = onBoth;
            extension.requestBoth = true;
            extension.currentValue = currentValue;
            extension.previousSignal = currentItem;
            UISignalPickerExtension.currentFilter = filterFunc;

            UISignalPickerExtension.Popup(pos, extension);
        }

        public static long Clamp(long value, long min, long max)
        {
            return value < min ? min : value > max ? max : value;
        }

        public void SetValue(int value)
        {
            ignoreEvents = true;
            currentValue = value;
            field.text = currentValue.ToString();
            slider.value = Mathf.Log10(currentValue);
            ignoreEvents = false;
        }

        public void SliderChanged(float newValue)
        {
            if (!ignoreEvents)
            {
                ignoreEvents = true;
                long tmp = Convert.ToInt64(Math.Pow(10f, newValue));
                currentValue = (int) Clamp(tmp, int.MinValue, int.MaxValue);
                field.text = currentValue.ToString();
            }

            ignoreEvents = false;
        }

        public void FieldChanged(string newValue)
        {
            if (!ignoreEvents)
            {
                ignoreEvents = true;
                long tmp = long.Parse(newValue);
                currentValue = (int) Clamp(tmp, int.MinValue, int.MaxValue);
                field.text = currentValue.ToString();
                slider.value = Mathf.Log10(currentValue);
            }

            ignoreEvents = false;
        }

        public void SubmitClicked()
        {
            UISignalPicker picker = UIRoot.instance.uiGame.signalPicker;
            VFInput.UseMouseLeft();
            picker.onReturn = null;
            if (requestBoth)
            {
                onReturnBoth?.Invoke(picker.selectedSignal, currentValue);
                picker._Close();
            }
            else
            {
                picker._Close();
                onReturnCount?.Invoke(currentValue);
            }
        }

        public bool OnBoxMouseDown(UISignalPicker picker)
        {
            if (!requestBoth || picker.hoveredIndex < 0) return true;
            
            lastSelectedIndex = picker.hoveredIndex;
            picker.selectedSignal = picker.signalArray[picker.hoveredIndex];
            VFInput.UseMouseLeft();
            return false;

        }

        public void TestMouseIndex(UISignalPicker picker)
        {
            if (!requestBoth || lastSelectedIndex < 0) return;
            
            Vector2 newPos = new Vector2(lastSelectedIndex % 14 * 46 - 1, -Mathf.FloorToInt(lastSelectedIndex / 14f) * 46 + 1);
            picker.selImage.rectTransform.anchoredPosition = newPos;
            picker.selImage.gameObject.SetActive(true);
        }

        public void Open(UISignalPicker picker)
        {
            gameObject.SetActive(true);
            ((RectTransform) picker.transform).sizeDelta = new Vector2(692, 696);
        }

        public void Close(UISignalPicker picker)
        {
            requestBoth = false;
            gameObject.SetActive(false);
            ((RectTransform) picker.transform).sizeDelta = new Vector2(692, 614);
        }

        public void OnPopup(UISignalPicker picker)
        {
            picker.onReturn = onReturnSignal;
        }

        public void PostPopup(UISignalPicker picker)
        {
            if (requestBoth)
            {
                if (previousSignal != 0)
                {
                    picker.currentType = GetTab(previousSignal);
                    picker.RefreshIcons();
                    picker.selectedSignal = previousSignal;
                
                    for (int i = 0; i < picker.signalArray.Length; i++)
                    {
                        int signal = picker.signalArray[i];
                        if (signal != previousSignal) continue;
                        
                        lastSelectedIndex = i;
                        break;
                    }
                }
            }
            SetValue(currentValue);
        }
        
        public int GetTab(int signalId)
        {
            if (signalId < 1000) return 1;

            if (signalId < 20000)
            {
                ItemProto itemProto = LDB.items.Select(signalId);
                if (itemProto == null) return 1;
                
                int type = itemProto.GridIndex / 1000;
                return type + 1;
            }

            if (signalId < 40000) return 4;
            if (signalId >= 60000) return 1;
            
            TechProto techProto = LDB.techs.Select(signalId - 40000);
            if (techProto == null) return 5;

            return techProto.ID < 2000 ? 5 : 6; 
        }
    }
}