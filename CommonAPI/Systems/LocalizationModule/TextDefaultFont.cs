using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace CommonAPI.Systems.ModLocalization
{
    /// <summary>
    ///     Additional container for Text component - this should be always present but it's not a monobehaviour.
    /// <para>Use static api of <see cref="TextFontManager.Get"/> to request underlying font information</para>
    /// </summary>
    public class TextDefaultFont
    {
        /// <summary>
        ///     Field info helper to access font data
        /// </summary>
        private static readonly FieldInfo FieldInfo_FontData = AccessTools.Field(typeof(Text), "m_FontData");

        /// <summary>
        ///     Default font used by this component
        /// </summary>
        public Font DefaultFont;
        
        /// <summary>
        ///     Stored text refernece
        /// </summary>
        public Text Reference;
        
        /// <summary>
        ///     FontData private field reference
        /// </summary>
        private FontData FontData;

        /// <summary>
        ///     Default constructor
        /// </summary>
        /// <param name="reference"></param>
        public TextDefaultFont(Text reference)
        {
            Reference = reference;
            FontData = (FontData)FieldInfo_FontData.GetValue(Reference);
            DefaultFont = FontData.font;
        }

        /// <summary>
        ///     Method invoked exclusively by <see cref="Text_Font_Gettter_Harmony"/>
        /// </summary>
        public void OnGetFont()
        {
            if (DefaultFont == null || FontData == null) return;

            int currentLanguageId = Localization.CurrentLanguageLCID;
            if (!LocalizationModule.extraDataEntires.ContainsKey(currentLanguageId)) return;

            var languageData = LocalizationModule.extraDataEntires[currentLanguageId];

            if (languageData.customFont != null)
            {
                if (FontData.font != languageData.customFont)
                {
                    FontData.font = languageData.customFont;
                }
            }
            else
            {
                if (FontData.font != DefaultFont)
                {
                    FontData.font = DefaultFont;
                }
            }
        }

        /// <summary>
        ///     Apply custom fond immediately skipping TextFontManager
        /// </summary>
        /// <param name="fontToUse"></param>
        public void UseCustomFontImmediate(Font fontToUse)
        {
            Reference.font = fontToUse;
        }
    }
}