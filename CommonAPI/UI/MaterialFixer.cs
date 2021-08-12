using CommonAPI;
using UnityEngine;

namespace CommonAPI
{
    public class MaterialFixer : MonoBehaviour
    {
        private static readonly int colorGlass = Shader.PropertyToID("_ColorGlass");
        private static readonly int vibrancy = Shader.PropertyToID("_Vibrancy");
        private static readonly int brightness = Shader.PropertyToID("_Brightness");
        private static readonly int flatten = Shader.PropertyToID("_Flatten");
        
        private void Start()
        {
            if (!Application.isEditor)
            {
                Material trsmat = ProtoRegistry.CreateMaterial("UI/TranslucentImage", "trs-mat", "#00000000", null,
                    new[] {"_EMISSION"});

                trsmat.SetFloat(colorGlass, 1f);
                trsmat.SetFloat(vibrancy, 1.1f);
                trsmat.SetFloat(brightness, -0.5f);
                trsmat.SetFloat(flatten, 0.005f);

                TranslucentImage[] images = GetComponentsInChildren<TranslucentImage>(true);
                foreach (TranslucentImage image in images)
                {
                    image.material = trsmat;
                    image.vibrancy = 1.1f;
                    image.brightness = -0.5f;
                    image.flatten = 0.005f;
                    image.spriteBlending = 0.7f;
                }
            }
        }
    }
}