using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace CommonAPI
{
    public class UIPowerIndicator : MonoBehaviour
    {
        public Image powerIcon;
        public Text powerText;
        public Text stateText;
        
        public ColorProperties colors;
        private StringBuilder powerServedSb;

        public CustomMachineWindow window;

        public void Init(CustomMachineWindow window)
        {
            this.window = window;
            powerServedSb = new StringBuilder("         W     %", 20);
        }

        public void OnUpdate(int pcId)
        {
            PowerConsumerComponent powerConsumerComponent = window.powerSystem.consumerPool[pcId];
            int networkId = powerConsumerComponent.networkId;
            PowerNetwork powerNetwork = window.powerSystem.netPool[networkId];
            float num = powerNetwork == null || networkId <= 0 ? 0f : (float) powerNetwork.consumerRatio;
            double num2 = powerConsumerComponent.requiredEnergy * 60L;
            long valuel = (long) (num2 * num);
            StringBuilderUtility.WriteKMG(powerServedSb, 8, valuel);
            StringBuilderUtility.WriteUInt(powerServedSb, 12, 3, (uint) (num * 100f));
            if (num == 1f)
            {
                powerText.text = powerServedSb.ToString();
                powerIcon.color = colors.powerNormalIconColor;
                powerText.color = colors.powerNormalColor;
            }
            else if (num > 0.1f)
            {
                powerText.text = powerServedSb.ToString();
                powerIcon.color = colors.powerLowIconColor;
                powerText.color = colors.powerLowColor;
            }
            else
            {
                powerText.text = "未供电".Translate();
                powerIcon.color = Color.clear;
                powerText.color = colors.powerOffColor;
            }

            if (num == 1f)
            {
                stateText.text = "待机".Translate();
                stateText.color = colors.idleColor;
            }
            else if (num > 0.1f)
            {
                stateText.text = "电力不足".Translate();
                stateText.color = colors.powerLowColor;
            }
            else
            {
                stateText.text = "停止运转".Translate();
                stateText.color = colors.powerOffColor;
            }
        }
    }
}