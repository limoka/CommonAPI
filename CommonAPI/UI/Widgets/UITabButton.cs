using System;
using UnityEngine;
using UnityEngine.UI;

namespace CommonAPI
{
    public class UITabButton : MonoBehaviour
    {
        public string tabName;
        public int tabIndex;

        public Localizer localizer;
        public UIButton button;
        public Image icon;
        

        public void Init(Sprite newIcon, string name, int index, Action<int> pressCallback)
        { 
            tabName = name;
            tabIndex = index;
            localizer.stringKey = tabName;
            icon.sprite = newIcon;
            button.data = index;
            button.onClick += pressCallback;
        }

        public void TabSelected(int index)
        {
            button.highlighted = index == tabIndex;
            button.button.interactable = index != tabIndex;
        }
    }
}