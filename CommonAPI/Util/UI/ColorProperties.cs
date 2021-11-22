using UnityEngine;

namespace CommonAPI
{
    [CreateAssetMenu(fileName = "Color Properties", menuName = "DSP/Color Properties", order = 0)]
    public class ColorProperties : ScriptableObject
    {
        [Header("Colors & Settings")] public Color powerNormalColor;

        public Color powerLowColor;

        public Color powerNormalIconColor;

        public Color powerLowIconColor;

        public Color powerOffColor;

        public Color idleColor;

        public Color workNormalColor;

        public Color workStoppedColor;

        public Color marqueeOnColor;

        public Color marqueeOffColor;
    }
}