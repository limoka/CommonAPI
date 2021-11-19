using CommonAPI.ShotScene.Patches;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.UI;

namespace CommonAPI.ShotScene
{
    public class GeneratorSceneController : MonoBehaviour
    {
        public static GeneratorSceneController shotGenerator;
        public static PostEffectController postEffectController;

        public static Vector3 oldCameraPos;
        public static Quaternion oldCameraRot;
        
        public static bool isShotSceneLoaded;
        public static bool patchLoaded;
        public static bool pointerInside;
        public bool l_pointerInside;

        public TPCameraController cameraController;
        public TPCameraController lightController;
        public Light light;
        public InputField lightColorField;
        public Slider lightColorSlider;
        public bool ignoreLightEvents;
        
        
        public RuntimeIconGenerator generator;
        public Transform center;
        public Canvas canvas;

        public void OnCameraToggleChanged(bool value)
        {
            if (cameraController != null)
                cameraController.enabled = value;
        }
        
        public void OnLightToggleChanged(bool value)
        {
            if (lightController != null)
                lightController.enabled = value;
        }
        
        public void OnCameraYSliderChanged(float value)
        {
            if (cameraController != null)
            {
                cameraController.cameraOffset.y = -value + 2;
                cameraController.RecalculatePosition();
            }
        }

        public void PointerIn()
        {
            pointerInside = true;
            l_pointerInside = true;
        }

        public void PointerOut()
        {
            pointerInside = false;
            l_pointerInside = false;
        }
        
        
        public void OnLightColorSliderChanged(float value)
        {
            if (light != null && !ignoreLightEvents)
            {
                int temp = Mathf.RoundToInt(value);
                light.color = Mathf.CorrelatedColorTemperatureToRGB(temp);
                ignoreLightEvents = true;
                lightColorField.text = temp.ToString();
                ignoreLightEvents = false;
            }
        }
        
        public void OnLightColorEntered(string input)
        {
            if (light != null && !ignoreLightEvents)
            {
                int value = int.Parse(input);
                if (value < 1000)
                {
                    value = 1000;
                }else if (value > 15000)
                {
                    value = 15000;
                }
                light.color = Mathf.CorrelatedColorTemperatureToRGB(value);
                ignoreLightEvents = true;
                lightColorField.text = value.ToString();
                lightColorSlider.value = value;
                ignoreLightEvents = false;
            }
        }

        public void SetLightIntensity(float value)
        {
            if (light != null)
            {
                light.intensity = value;
            }
        }
        
        
        public static void LoadIconGeneratorScene()
        {
            if (!isShotSceneLoaded)
            {
                isShotSceneLoaded = true;
                DSPGame.EndGame();
                
                if (!patchLoaded)
                {
                    CommonAPIPlugin.harmony.PatchAll(typeof(PostProcessingMaterialFactoryPatch));
                    patchLoaded = true;
                }
                
                UIRoot.instance.overlayCanvas.enabled = false;
                oldCameraPos = GameCamera.main.transform.position;
                oldCameraRot = GameCamera.main.transform.rotation;

                GameCamera.instance.enabled = false;
                GameCamera.instance.gameObject.SetActive(false);
                GameCamera.main.backgroundColor = Color.clear;
                GameCamera.main.fieldOfView = 30;
                GameObject prefab = CommonAPIPlugin.resource.bundle.LoadAsset<GameObject>("Assets/CommonAPI/IconShotPrefab.prefab");
                shotGenerator = Instantiate(prefab).GetComponent<GeneratorSceneController>();
                shotGenerator.lightController.enabled = false;
                
                shotGenerator.cameraController = GameCamera.main.gameObject.AddComponent<TPCameraController>();
                shotGenerator.cameraController.lookAt = shotGenerator.center;
                shotGenerator.generator.Load(GameCamera.main);
                
                shotGenerator.canvas.worldCamera = UIRoot.instance.overlayCanvas.worldCamera;
                shotGenerator.canvas.planeDistance = 20;

                postEffectController = shotGenerator.cameraController.GetComponent<PostEffectController>();
                BloomModel.Settings bloomSettings = postEffectController.menuProfile.bloom.settings;
                bloomSettings.lensDirt.intensity = 0;
                bloomSettings.bloom.intensity = 0.05f;
                bloomSettings.bloom.radius = 0;
                postEffectController.menuProfile.bloom.settings = bloomSettings;
                postEffectController.menuProfile.antialiasing.enabled = false;
            }
        }

        public static void UnloadIconGeneratorScene()
        {
            if (isShotSceneLoaded)
            {
                isShotSceneLoaded = false;
                BloomModel.Settings bloomSettings = postEffectController.menuProfile.bloom.settings;
                bloomSettings.lensDirt.intensity = 5;
                bloomSettings.bloom.intensity = 0.3f;
                bloomSettings.bloom.radius = 4;
                postEffectController.menuProfile.bloom.settings = bloomSettings;
                postEffectController.menuProfile.antialiasing.enabled = true;
                Destroy(shotGenerator.cameraController);
                Destroy(shotGenerator.gameObject);
                shotGenerator = null;
                
                GameCamera.main.backgroundColor = Color.black;
                GameCamera.main.targetTexture = null;
                GameCamera.main.fieldOfView = 60;
                GameCamera.main.transform.position = oldCameraPos;
                GameCamera.main.transform.rotation = oldCameraRot;
                GameCamera.instance.enabled = true;
                
                GameCamera.instance.gameObject.SetActive(true);
                UIRoot.instance.overlayCanvas.enabled = true;
                DSPGame.StartDemoGame(Random.Range(-2, 0));
            }
        }
    }
}