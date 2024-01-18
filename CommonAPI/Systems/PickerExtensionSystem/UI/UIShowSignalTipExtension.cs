using UnityEngine;

namespace CommonAPI.Systems.UI
{
    public class UIShowSignalTipExtension : IUpdatePickerExtension<UISignalPicker>
    {
        private float mouseInTime;
        private UISignalTip screenSignalTip;
        
        public void OnUpdate(UISignalPicker picker)
        {
            int xPos = -1;
            int yPos = -1;
            int signalId = 0;
            if (picker.hoveredIndex >= 0)
            {
                signalId = picker.signalArray[picker.hoveredIndex];
                xPos = picker.hoveredIndex % 14;
                yPos = picker.hoveredIndex / 14;
            }
            
            if (signalId != 0 && signalId < 1000)
            {
                mouseInTime += Time.deltaTime;
                if (mouseInTime > picker.showItemTipsDelay)
                {
                    if (screenSignalTip == null)
                    {
                        screenSignalTip = UISignalTip.Create(signalId, picker.itemTipAnchor, new Vector2(xPos * 46 + 15, -(float)yPos * 46 - 50), picker.iconImage.transform);
                    }
                    if (!screenSignalTip.gameObject.activeSelf)
                    {
                        screenSignalTip.gameObject.SetActive(true);
                        screenSignalTip.SetTip(signalId, picker.itemTipAnchor, new Vector2(xPos * 46 + 15, -(float)yPos * 46 - 50), picker.iconImage.transform);
                        return;
                    }
                    if (screenSignalTip.showingSignalId != signalId)
                    {
                        screenSignalTip.SetTip(signalId, picker.itemTipAnchor, new Vector2(xPos * 46 + 15, -(float)yPos * 46 - 50), picker.iconImage.transform);
                    }
                }
            }
            else
            {
                CloseTip();
            }
        }
        
        public void Open(UISignalPicker picker)
        {
            CloseTip();
        }

        public void Close(UISignalPicker picker)
        {
            CloseTip();
        }

        private void CloseTip()
        {
            if (mouseInTime > 0f)
            {
                mouseInTime = 0f;
            }

            if (screenSignalTip != null)
            {
                screenSignalTip.showingSignalId = 0;
                screenSignalTip.gameObject.SetActive(false);
            }
        }

        public void OnPopup(UISignalPicker picker)
        {
        }

        public void PostPopup(UISignalPicker picker)
        {
        }
    }
}