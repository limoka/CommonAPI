using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace CommonAPI.ShotScene
{
    public class RuntimeIconGenerator : MonoBehaviour
    {
        public int itemId;
        public int modelOffset;
        public bool animAuto = false;
        public float animTime;
        public float animPrepare;
        public float animWorking = 1f;
        public uint animState = 1U;
        public float animPower = 1f;

        public bool preload = false;
        public bool loaded = false;
        public bool stopDrawingUIBeforeShot = true;
        public int waitTime = 4;
        public bool directMode = true;

        public RenderTexture outputRenderTexture;
        public RawImage outputImage;
        public Camera camera160;
        public Text outputText;
        public GameObject renderWindow;
        public GameObject background;

        public Image overlayIconImage;
        
        public InputField pathInputField;
        public string defaultPrefix = "ItemIcons";
        public string savingPath = "";
        public string itemDefaultPath = "";
        
        public AmbientDesc ambient;
        public Material[] materials;

        private Dictionary<int, BatchRenderer> batches;
        private ComputeBuffer instBuffer;
        private ComputeBuffer animBuffer;
        private ComputeBuffer argBuffer;
        private GPUOBJECT[] instArr;
        private AnimData[] animArr;
        private uint[] argArr;
        private uint[] idArr;

        private int framesElapsed;
        private bool stopDrawing = false;

        private static readonly int globalCutUnderground = Shader.PropertyToID("_Global_Cut_Underground");
        private static readonly int globalPgi = Shader.PropertyToID("_Global_PGI");
        private static readonly int globalUgi = Shader.PropertyToID("_Global_UGI");
        private static readonly int globalAmbientColor0 = Shader.PropertyToID("_Global_AmbientColor0");
        private static readonly int globalAmbientColor1 = Shader.PropertyToID("_Global_AmbientColor1");
        private static readonly int globalAmbientColor2 = Shader.PropertyToID("_Global_AmbientColor2");
        private static readonly int globalBiomoColor0 = Shader.PropertyToID("_Global_Biomo_Color0");
        private static readonly int globalBiomoColor1 = Shader.PropertyToID("_Global_Biomo_Color1");
        private static readonly int globalBiomoColor2 = Shader.PropertyToID("_Global_Biomo_Color2");
        private static readonly int globalPlanetRadius = Shader.PropertyToID("_Global_Planet_Radius");

        public void OnExitButtonPressed()
        {
            GeneratorSceneController.UnloadIconGeneratorScene();
        }

        public void SetSavingPath(string newPath)
        {
            savingPath = newPath;
        }

        public void ResetSavingPath()
        {
            savingPath = itemDefaultPath;
            pathInputField.text = savingPath;
        }

        public void SetOverlayEnabled(bool value)
        {
            overlayIconImage.enabled = value;
        }
        
        public void PressCapture()
        {
            SetCaptureMode(true, true);
            if (stopDrawingUIBeforeShot)
            {
                framesElapsed = 0;
                stopDrawing = true;
                camera160.targetTexture = null;
            }
            else
            {
                TakeShot();
            }
        }

        public void SetCaptureMode(bool direct)
        {
            SetCaptureMode(direct, false);
        }
        
        public void SetCaptureMode(bool direct, bool tmp)
        {
            if (direct)
            {
                camera160.targetTexture = null;
                RenderTexture.active = null;
                renderWindow.SetActive(false);
                background.SetActive(false);
            }
            else
            {
                camera160.targetTexture = outputRenderTexture;
                RenderTexture.active = outputRenderTexture;
                
                renderWindow.SetActive(true);
                background.SetActive(true);
            }
            
            if (!tmp)
                directMode = direct;
        }

        public void SetEnvIntensity(float value)
        {
            ambient.ambientColor0 = new Color(value, value, value, 1);
        }
        

        private void Start()
        {
            if (camera160 != null)
            {
                Load(camera160);
            }
        }

        public void Load(Camera camera)
        {
            outputText.text = "";
            camera160 = camera;
            loaded = true;
            outputRenderTexture = new RenderTexture(1280, 1280, 24, RenderTextureFormat.ARGB32);
            outputImage.texture = outputRenderTexture;
            SetCaptureMode(false);
            
            instBuffer = new ComputeBuffer(16, 32, ComputeBufferType.Default);
            animBuffer = new ComputeBuffer(16, 20, ComputeBufferType.Default);
            argBuffer = new ComputeBuffer(5120, 4, ComputeBufferType.IndirectArguments);
            instArr = new GPUOBJECT[16];
            animArr = new AnimData[16];
            argArr = new uint[5120];
            idArr = new uint[1];
            batches = new Dictionary<int, BatchRenderer>();
            ModelProto[] dataArray = LDB.models.dataArray;
            int num = 0;
            foreach (ModelProto modelProto in dataArray)
            {
                if (preload)
                    modelProto.Preload();
                PrefabDesc prefabDesc = modelProto.prefabDesc;
                if (prefabDesc != null && prefabDesc.lodCount > 0 && prefabDesc.lodMeshes != null && prefabDesc.lodMaterials != null &&
                    prefabDesc.lodMeshes.Length != 0 && prefabDesc.lodMaterials.Length != 0)
                {
                    Mesh mesh = prefabDesc.lodMeshes[0];
                    BatchRenderer batchRenderer = new BatchRenderer(mesh, prefabDesc.lodMaterials[0], 0, 16, instBuffer, argBuffer, num,
                        prefabDesc.lodVertas[0],  0, prefabDesc.castShadow, prefabDesc.recvShadow);
                    batchRenderer.modelProto = modelProto;
                    int subMeshCount = mesh.subMeshCount;
                    for (int j = 0; j < subMeshCount; j++)
                    {
                        argArr[num] = mesh.GetIndexCount(j);
                        argArr[num + 1] = 1U;
                        argArr[num + 2] = mesh.GetIndexStart(j);
                        argArr[num + 3] = mesh.GetBaseVertex(j);
                        argArr[num + 4] = 0U;
                        num += 5;
                    }

                    if (subMeshCount > 0)
                    {
                        batches[modelProto.ID] = batchRenderer;
                    }
                }
            }

            argBuffer.SetData(argArr);
            if (preload)
            {
                ItemProto[] dataArray2 = LDB.items.dataArray;
                ItemProto.itemProtoById = new ItemProto[12000];
                for (int k = 0; k < dataArray2.Length; k++)
                {
                    dataArray2[k].Preload(k);
                }
            }
        }

        private void OnDestroy()
        {
            if (!loaded) return;

            if (instBuffer != null)
            {
                instBuffer.Release();
                instBuffer = null;
            }

            if (animBuffer != null)
            {
                animBuffer.Release();
                animBuffer = null;
            }

            if (argBuffer != null)
            {
                argBuffer.Release();
                argBuffer = null;
            }

            if (batches != null)
            {
                foreach (KeyValuePair<int, BatchRenderer> keyValuePair in batches)
                {
                    keyValuePair.Value.Free();
                }

                batches.Clear();
                batches = null;
            }
        }

        private void Environment()
        {
            Shader.SetGlobalFloat(globalCutUnderground, 1f);
            Shader.SetGlobalTexture(globalPgi, ambient.reflectionMap);
            Shader.SetGlobalTexture(globalUgi, null);
            Shader.SetGlobalColor(globalAmbientColor0, ambient.ambientColor0);
            Shader.SetGlobalColor(globalAmbientColor1, ambient.ambientColor1);
            Shader.SetGlobalColor(globalAmbientColor2, ambient.ambientColor2);
            Shader.SetGlobalColor(globalBiomoColor0, ambient.biomoColor0);
            Shader.SetGlobalColor(globalBiomoColor1, ambient.biomoColor1);
            Shader.SetGlobalColor(globalBiomoColor2, ambient.biomoColor2);
            Shader.SetGlobalFloat(globalPlanetRadius, 200f);
        }

        private void LateUpdate()
        {
            if (!loaded) return;
            //camera160.pixelRect = new Rect(Screen.width - 350, Screen.height - 350, 350f, 350f);
            camera160.rect = new Rect(0, 0, 1, 1);
            Environment();
            instArr[0].objId = 0U;
            instArr[0].posx = 0f;
            instArr[0].posy = 1f;
            instArr[0].posz = 0f;
            instArr[0].rotx = 0f;
            instArr[0].roty = 0f;
            instArr[0].rotz = 0f;
            instArr[0].rotw = 1f;
            instBuffer.SetData(instArr);
            idArr[0] = 0U;
            ItemProto itemProto = LDB.items.Select(itemId);
            if (itemProto != null && itemProto.ModelIndex != 0)
            {
                int num = itemProto.ModelIndex + modelOffset;
                if (batches.ContainsKey(num))
                {
                    if (!animAuto)
                    {
                        animArr[0].time = animTime;
                        animArr[0].prepare_length = animPrepare;
                        animArr[0].working_length = animWorking;
                        animArr[0].state = animState;
                        animArr[0].power = animPower;
                    }
                    else
                    {
                        animPrepare = (animArr[0].prepare_length = LDB.models.Select(num).prefabDesc.anim_prepare_length);
                        animWorking = (animArr[0].working_length = LDB.models.Select(num).prefabDesc.anim_working_length);
                        if (animWorking > 0f)
                        {
                            animTime = (animArr[0].time = Mathf.Repeat(Time.time, animWorking));
                        }
                        else
                        {
                            animTime = (animArr[0].time = Mathf.Repeat(Time.time, 1f));
                        }

                        animArr[0].state = animState;
                        animArr[0].power = animPower;
                    }

                    animBuffer.SetData(animArr);
                    BatchRenderer batchRenderer = batches[num];
                    batchRenderer.idBuffer.SetCounterValue(1U);
                    batchRenderer.idBuffer.SetData(idArr);
                    batchRenderer.SetAdditionalBuffer("_AnimBuffer", animBuffer);
                    materials = batchRenderer.materials;
                    batchRenderer.Render();
                }
            }
        }

        private void OnGUI()
        {
            framesElapsed++;
            if (stopDrawing)
            {
                if (framesElapsed > waitTime)
                {
                    TakeShot();
                    stopDrawing = false;
                    framesElapsed = 0;
                }

                return;
            }

            if (!loaded) return;

            ItemProto[] dataArray = LDB.items.dataArray;
            for (int i = 0; i < dataArray.Length; i++)
            {
                ItemProto itemProto = dataArray[i];
                int num = i % 15;
                int num2 = i / 15;
                Rect position = new Rect(num * 50 + 50, num2 * 50 + 50, 50f, 50f);
                Rect position2 = new Rect(position.x + 5f, position.y + 5f, 40f, 40f);
                if (GUI.Button(position, ""))
                {
                    itemId = itemProto.ID;
                    ItemProto newItem = LDB.items.Select(itemId);
                    if (newItem != null && newItem.IconPath != "")
                    {
                        itemDefaultPath = Path.Combine(defaultPrefix, newItem.IconPath);
                        savingPath = itemDefaultPath;
                        pathInputField.text = savingPath;
                        outputText.text = "";
                        overlayIconImage.sprite = newItem.iconSprite;
                    }
                }

                if (itemProto.iconSprite != null)
                {
                    GUI.DrawTexture(position2, itemProto.iconSprite.texture);
                }
            }

            ItemProto selectedItem = LDB.items.Select(itemId);
            if (selectedItem != null)
            {
                GUI.Label(new Rect(825f, 50f, 300f, 24f), selectedItem.name);
                GUI.Box(new Rect(825f, 74f, 90f, 90f), "");
                if (selectedItem.iconSprite != null)
                {
                    GUI.DrawTexture(new Rect(830, 79f, 80f, 80f), selectedItem.iconSprite.texture);
                }
            }
        }

        private void TakeShot()
        {
            ItemProto selectedItem = LDB.items.Select(itemId);
            int key = selectedItem.ModelIndex + modelOffset;
            if (batches.ContainsKey(key))
            {
                RenderTexture renderTexture = new RenderTexture(1280, 1280, 24, RenderTextureFormat.ARGB32);
                renderTexture.antiAliasing = 8;
                camera160.targetTexture = renderTexture;
                camera160.pixelRect = new Rect(0f, 0f, 1280f, 1280f);
                BatchRenderer batchRenderer = batches[key];
                batchRenderer.idBuffer.SetCounterValue(1U);
                batchRenderer.idBuffer.SetData(idArr);
                batchRenderer.SetAdditionalBuffer("_AnimBuffer", animBuffer);
                batchRenderer.Render();
                camera160.Render();
                Texture2D texture2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false);
                RenderTexture active = RenderTexture.active;
                RenderTexture.active = renderTexture;
                texture2D.ReadPixels(new Rect(0f, 0f, renderTexture.width, renderTexture.height), 0, 0);
                texture2D.Apply();
                RenderTexture.active = active;
                byte[] pngData = texture2D.EncodeToPNG();
                string filePath = Path.Combine(Application.dataPath, savingPath) + ".png";

                FileInfo fileInfo = new FileInfo(filePath);
                if (!Directory.Exists(fileInfo.Directory.FullName))
                    Directory.CreateDirectory(fileInfo.Directory.FullName);

                try
                {
                    FileStream fileStream = new FileStream(filePath, FileMode.Create);
                    fileStream.Write(pngData, 0, pngData.Length);
                    fileStream.Close();
                    string message = "[" + selectedItem.name + "] icon capture ok! \n" + savingPath;
                    outputText.text = message;
                    Debug.Log(message);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                    Debug.LogError("Failed! " + filePath);
                    outputText.text = $"Saving failed! {e.Message}";
                }

                SetCaptureMode(directMode, true);

                Destroy(texture2D);
                renderTexture.Release();
                Destroy(renderTexture);
            }
        }
    }
}