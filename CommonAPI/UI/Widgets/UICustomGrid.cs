using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CommonAPI
{
    public class UICustomGrid : ManualBehaviour
    {
        public int colCount = 10;

        public int rowCount = 5;

        private const int kGridSize = 50;

        private const int kPadding = 2;

        public RectTransform rectTrans;

        public RectTransform contentRect;

        public Image bgImage;

        public RawImage iconImage;

        public Text prefabNumText;

        public bool showTips = true;

        public float showTipsDelay = 0.4f;

        public int tipAnchor = 7;

        private Material iconImageMat;

        private Material bgImageMat;

        private uint[] iconIndexArray;

        private uint[] stateArray;

        private ComputeBuffer iconIndexBuffer;

        private ComputeBuffer stateBuffer;

        private Text[] numTexts;

        private int[] numbers;

        private const int kMaxGrid = 400;

        private IStorage storage;

        private StringBuilder strb = new StringBuilder("      ", 6);

        private float mouseInTime;

        private int mouseOnX = -1;

        private int mouseOnY = -1;

        private UIItemTip tip;

        private static readonly int buffer = Shader.PropertyToID("_StateBuffer");
        private static readonly int indexBuffer = Shader.PropertyToID("_IndexBuffer");
        private static readonly int bans = Shader.PropertyToID("_Bans");
        private static readonly int gridProp = Shader.PropertyToID("_Grid");
        private static readonly int rect = Shader.PropertyToID("_Rect");

        public override void _OnCreate()
        {
            numTexts = new Text[400];
            numbers = new int[400];
            iconIndexArray = new uint[1024];
            iconIndexBuffer = new ComputeBuffer(iconIndexArray.Length, 4);
            stateArray = new uint[1024];
            stateBuffer = new ComputeBuffer(stateArray.Length, 4);
            bgImageMat = Instantiate(bgImage.material);
            iconImageMat = Instantiate(iconImage.material);
            bgImageMat.SetBuffer(buffer, stateBuffer);
            iconImageMat.SetBuffer(indexBuffer, iconIndexBuffer);
            bgImage.material = bgImageMat;
            iconImage.material = iconImageMat;
        }


        public override void _OnDestroy()
        {
            if (tip != null)
            {
                Destroy(tip.gameObject);
                tip = null;
            }

            Destroy(bgImageMat);
            Destroy(iconImageMat);
            iconIndexBuffer.Release();
            stateBuffer.Release();
            numTexts = null;
            numbers = null;
            bgImageMat = null;
            iconImageMat = null;
            iconIndexArray = null;
            iconIndexBuffer = null;
            stateArray = null;
            stateBuffer = null;
        }


        public override bool _OnInit()
        {
            iconImage.texture = GameMain.iconSet.texture;
            SetStorageData(data as IStorage);
            return data != null;
        }


        public override void _OnFree()
        {
            SetStorageData(null);
        }


        public override void _OnOpen() { }


        public override void _OnClose()
        {
            if (tip != null)
            {
                tip.gameObject.SetActive(false);
            }

            mouseInTime = 0f;
            OnContentMouseExit(null);
            SetStorageData(null);
        }


        public override void _OnUpdate()
        {
            if (showTips)
            {
                int gridX = -1;
                int gridY = -1;
                if (GameMain.mainPlayer.inhandItemId > 0)
                {
                    mouseInTime = -1f;
                }
                else if (UIRoot.instance.uiGame.gridSplit.active)
                {
                    mouseInTime = -1f;
                }
                else
                {
                    GetGridPos(out gridX, out gridY, true);
                }

                if (mouseOnX != gridX || mouseOnY != gridY)
                {
                    mouseOnX = gridX;
                    mouseOnY = gridY;
                    if (mouseInTime > 0f)
                        mouseInTime = 0f;
                }

                UpdateTips(gridX, gridY);
            }

            if (storage != null && storage.changed)
            {
                OnStorageContentChanged();
                storage.changed = false;
            }
        }

        protected void UpdateTips(int gridX, int gridY)
        {
            int itemId = GetItemId(mouseOnX, mouseOnY); 

            if (itemId == 0)
            {
                mouseOnX = -1;
                mouseOnY = -1;
            }
            if (mouseOnX >= 0 && mouseOnY >= 0)
            {
                mouseInTime += Time.deltaTime;
                if (mouseInTime > showTipsDelay)
                {
                    if (tip == null)
                    {
                        tip = UIItemTip.Create(itemId, tipAnchor, new Vector2(gridX * 50 + 15, -(float) gridY * 50 - 50),
                            contentRect);
                    }

                    if (!tip.gameObject.activeSelf)
                    {
                        tip.gameObject.SetActive(true);
                        tip.SetTip(itemId, tipAnchor, new Vector2(gridX * 50 + 15, -(float) gridY * 50 - 50), contentRect);
                    }
                    else if (tip.showingItemId != itemId)
                    {
                        tip.SetTip(itemId, tipAnchor, new Vector2(gridX * 50 + 15, -(float) gridY * 50 - 50), contentRect);
                    }
                }
            }
            else
            {
                if (mouseInTime > 0f)
                {
                    mouseInTime = 0f;
                }

                if (tip != null)
                {
                    tip.showingItemId = 0;
                    tip.gameObject.SetActive(false);
                }
            }
        }

        protected int GetItemId(int gridX, int gridY)
        {
            int itemId = 0;
            if (gridX >= 0 && gridY >= 0)
            {
                int gridIndex = gridX + gridY * colCount;
                itemId = storage.GetAt(gridIndex).GetItemId();
            }

            return itemId;
        }


        private void OnApplicationFocus(bool focus)
        {
            if (!focus)
            {
                mouseInTime = 0f;
                OnContentMouseExit(null);
            }
        }

        protected void SetStorageData(IStorage _storage)
        {
            if (_storage != storage)
            {
                storage = _storage;
                OnStorageDataChanged();
            }
        }


        protected void SetRectSize()
        {
            if (rectTrans == null) return;

            rectTrans.sizeDelta = new Vector2(colCount * 50 + 4, rowCount * 50 + 4);

            Vector4 value = new Vector4(colCount, rowCount, 0.04f, 0.04f);
            bgImageMat.SetVector(gridProp, value);
            iconImageMat.SetVector(gridProp, value);
            Vector4 value2 = new Vector4(0.1f, 0.1f, 1.25f, 1.25f);
            iconImageMat.SetVector(rect, value2);
        }


        protected void CreateGridGraphic(int index)
        {
            if (numTexts[index] == null)
            {
                numTexts[index] = Instantiate(prefabNumText, rectTrans);
            }

            RepositionGridGraphic(index);
        }


        protected void SetGridGraphic(int index, bool newState)
        {
            if (numTexts[index].gameObject.activeSelf != newState)
            {
                numTexts[index].gameObject.SetActive(newState);
            }

            if (newState) return;

            numTexts[index].text = "";
            numbers[index] = 0;
            stateArray[index] = 0U;
            iconIndexArray[index] = 0U;
        }


        protected void DeactiveAllGridGraphics()
        {
            for (int i = 0; i < 400; i++)
            {
                if (numTexts[i] != null && numTexts[i].gameObject.activeSelf)
                {
                    numTexts[i].text = "";
                    numTexts[i].gameObject.SetActive(false);
                }
            }

            Array.Clear(numbers, 0, 400);
            Array.Clear(stateArray, 0, stateArray.Length);
            Array.Clear(iconIndexArray, 0, iconIndexArray.Length);
        }


        protected void RepositionGridGraphic(int index)
        {
            int num = index % colCount;
            int num2 = index / colCount;
            numTexts[index].rectTransform.anchoredPosition = new Vector2(num * 50, num2 * -50 - 34);
        }


        public void OnStorageDataChanged()
        {
            Array.Clear(stateArray, 0, stateArray.Length);
            stateBuffer.SetData(stateArray);
            bgImageMat.SetBuffer(buffer, stateBuffer);
            Array.Clear(iconIndexArray, 0, iconIndexArray.Length);
            iconIndexBuffer.SetData(iconIndexArray);
            iconImageMat.SetBuffer(indexBuffer, iconIndexBuffer);
            OnStorageSizeChanged();
        }


        public void OnStorageSizeChanged()
        {
            if (storage == null)
            {
                rowCount = 1;
            }
            else
            {
                rowCount = (storage.size - 1) / colCount + 1;
            }

            SetRectSize();
            DeactiveAllGridGraphics();
            for (int i = 0; i < colCount * rowCount; i++)
            {
                CreateGridGraphic(i);
            }

            OnStorageContentChanged();
        }


        public void OnStorageContentChanged()
        {
            if (storage == null)
            {
                return;
            }

            if ((storage.size - 1) / colCount + 1 != rowCount)
            {
                OnStorageSizeChanged();
            }
            
            for (int i = 0; i < storage.size; i++)
            {
                IItem item = storage.GetAt(i);
                
                if (item.GetItemId() > 0)
                {
                    SetGridGraphic(i, true);
                    iconIndexArray[i] = GameMain.iconSet.itemIconIndex[item.GetItemId()];
                    stateArray[i] = 1U;
                    bool updateValue = numbers[i] != item.GetCount();
                    numbers[i] = item.GetCount();

                    if (!updateValue) continue;

                    if (item.GetMaxStackSize() > 1)
                    {
                        StringBuilderUtility.WriteKMG(strb, 5, item.GetCount());
                        numTexts[i].text = strb.ToString();
                    }
                    else
                    {
                        numTexts[i].text = "";
                    }
                }
                else
                {
                    iconIndexArray[i] = 0U;
                    stateArray[i] = 0U;
                    SetGridGraphic(i, false);
                }
            }

            iconIndexBuffer.SetData(iconIndexArray);
            stateBuffer.SetData(stateArray);
        }


        public void OnContentMouseEnter(BaseEventData eventData)
        {
            mouseInTime = 0f;
        }


        public void OnContentMouseExit(BaseEventData eventData)
        {
            mouseInTime = 0f;
            mouseOnX = -1;
            mouseOnY = -1;
        }


        public void OnContentMouseDown(BaseEventData eventData)
        {
            if (GameMain.mainPlayer == null)
            {
                return;
            }

            if (GetGridPos(out int gridX, out int gridY)) return;

            int grid = gridX + gridY * colCount;
            if (eventData is PointerEventData pointerEventData)
            {
                OnGridMouseDown(grid, (int) pointerEventData.button, VFInput.shift, VFInput.control, GameMain.mainPlayer);
            }
        }

        protected bool GetGridPos(out int gridX, out int gridY, bool filterEdges = false)
        {
            gridX = -1;
            gridY = -1;
            if (!UIRoot.ScreenPointIntoRect(Input.mousePosition, contentRect, out Vector2 point)) return false;

            gridX = Mathf.FloorToInt(point.x / 50f);
            gridY = Mathf.FloorToInt(-point.y / 50f);
            if (gridX < 0 || gridX >= colCount)
            {
                gridX = -1;
                gridY = -1;
                return false;
            }

            if (gridY < 0 || gridY >= rowCount)
            {
                gridX = -1;
                gridY = -1;
                return false;
            }
            
            int diffX = (int) (point.x - gridX * 50);
            int diffY = (int) (-point.y - gridY * 50);
            if (diffX <= 3 || diffY <= 3 || diffX >= 47 || diffY >= 47)
            {
                gridX = -1;
                gridY = -1;
                return false;
            }

            return true;
        }


        public void OnContentMouseUp(BaseEventData eventData)
        {
            if (GameMain.mainPlayer == null)
            {
                return;
            }

            PointerEventData pointer = eventData as PointerEventData;
            if (pointer.button == PointerEventData.InputButton.Right)
            {
                OnGridRightMouseUp(GameMain.mainPlayer);
            }

            if (GetGridPos(out int gridX, out int gridY)) return;

            int grid = gridX + gridY * colCount;
            OnGridMouseUp(grid, (int) pointer.button, VFInput.shift, VFInput.control, GameMain.mainPlayer);
        }

        public virtual void OnSort()
        {
        }
        
        protected virtual void OnGridMouseDown(int grid, int button, bool shift, bool control, Player player) { }


        protected virtual void OnGridMouseUp(int grid, int button, bool shift, bool control, Player player) { }


        protected virtual void OnGridRightMouseUp(Player player) { }


        protected virtual int HandTake(Player player, int grid, int count = 0)
        {
            return 0;
        }


        protected virtual void HandPut(Player player, int grid, int count = 0) { }
    }
}