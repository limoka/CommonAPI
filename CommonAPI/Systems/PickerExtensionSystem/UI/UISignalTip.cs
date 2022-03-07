using System;
using UnityEngine;
using UnityEngine.UI;

namespace CommonAPI.Systems.UI
{
    public class UISignalTip : MonoBehaviour
    {
        private static GameObject prefab;
        
        public Text nameText;
        public Text descText;
        public Image iconImage;
        public RectTransform trans;
        
        [NonSerialized]
        public int showingSignalId;
        
        private void OnDisable()
        {
            showingSignalId = 0;
        }
        
        public static UISignalTip Create(int signalId, int corner, Vector2 offset, Transform parent)
        {
            if (prefab == null)
            {
                prefab = CommonAPIPlugin.resource.bundle.LoadAsset<GameObject>("Assets/CommonAPI/UI/item-tooltip.prefab");
            }
            
            GameObject instance = Instantiate(prefab, parent);
            UISignalTip tip = instance.GetComponent<UISignalTip>();
            tip.SetTip(signalId, corner, offset, parent);
            return tip;
        }

        public void SetTip(int signalId, int corner, Vector2 offset, Transform parent)
        {
            showingSignalId = signalId;
            trans.SetParent(parent, true);
            SignalProto proto = LDB.signals.Select(signalId);

            if (proto != null)
            {
                nameText.text = proto.name;
                descText.text = proto.description;
                iconImage.sprite = proto.iconSprite;
            }
            else
            {
                nameText.text = "Unknown Signal";
                descText.text = "";
                iconImage.sprite = null;
            }
            
            Vector2 anchorMin;
            Vector2 pivot;
            switch (corner)
            {
                case 1:
                    anchorMin = new Vector2(0f, 0f);
                    pivot = new Vector2(1f, 1f);
                    break;
                case 2:
                    anchorMin = new Vector2(0.5f, 0f);
                    pivot = new Vector2(0.5f, 1f);
                    break;
                case 3:
                    anchorMin = new Vector2(1f, 0f);
                    pivot = new Vector2(0f, 1f);
                    break;
                case 4:
                    anchorMin = new Vector2(0f, 0.5f);
                    pivot = new Vector2(1f, 0.5f);
                    break;
                case 5:
                    anchorMin = new Vector2(0.5f, 0.5f);
                    pivot = new Vector2(0.5f, 0.5f);
                    break;
                case 6:
                    anchorMin = new Vector2(1f, 0.5f);
                    pivot = new Vector2(0f, 0.5f);
                    break;
                case 7:
                    anchorMin = new Vector2(0f, 1f);
                    pivot = new Vector2(0f, 1f);
                    break;
                case 8:
                    anchorMin = new Vector2(0.5f, 1f);
                    pivot = new Vector2(0.5f, 0f);
                    break;
                case 9:
                    anchorMin = new Vector2(1f, 1f);
                    pivot = new Vector2(0f, 0f);
                    break;
                default:
                    anchorMin = new Vector2(0.5f, 0.5f);
                    pivot = new Vector2(0.5f, 0.5f);
                    break;
            }
            
            trans.anchorMax = trans.anchorMin = anchorMin;
            trans.pivot = pivot;
            trans.anchoredPosition = offset;
            
            int textHeight = string.IsNullOrEmpty(descText.text) ? 0 : (int)descText.preferredHeight;

            int needHeight = 61 + textHeight;
            if (needHeight <= 120)
            {
                needHeight = 120;
            }

            trans.sizeDelta = new Vector2(290, needHeight);
            trans.SetParent(UIRoot.instance.itemTipTransform, true);
            
            Rect globalTipRect = UIRoot.instance.itemTipTransform.rect;
            float globalWidth = Mathf.RoundToInt(globalTipRect.width);
            float globalHeight = Mathf.RoundToInt(globalTipRect.height);
            float xPos = trans.anchorMin.x * globalWidth + trans.anchoredPosition.x;
            float yPos = trans.anchorMin.y * globalHeight + trans.anchoredPosition.y;
            Rect clampedRect = trans.rect;
            clampedRect.x += xPos;
            clampedRect.y += yPos;
            Vector2 zero = Vector2.zero;
            if (clampedRect.xMin < 0f)
            {
                zero.x -= clampedRect.xMin;
            }
            if (clampedRect.yMin < 0f)
            {
                zero.y -= clampedRect.yMin;
            }
            if (clampedRect.xMax > globalWidth)
            {
                zero.x -= clampedRect.xMax - globalWidth;
            }
            if (clampedRect.yMax > globalHeight)
            {
                zero.y -= clampedRect.yMax - globalHeight;
            }

            var anchoredPosition = trans.anchoredPosition;
            anchoredPosition += zero;
            trans.anchoredPosition = anchoredPosition;
            trans.localScale = new Vector3(1f, 1f, 1f);
        }
    }
}