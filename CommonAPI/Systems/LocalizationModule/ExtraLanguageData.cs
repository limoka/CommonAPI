using System.Collections.Generic;
using UnityEngine;

namespace CommonAPI.Systems.ModLocalization
{
    public class ExtraLanguageData
    {
        private Dictionary<string, string> _modStrings;
        private Dictionary<string, string> _stringsToEdit;
        public Font customFont;

        public Dictionary<string, string> modStrings
        {
            get
            {
                _modStrings ??= new Dictionary<string, string>();
                return _modStrings;
            }
        }

        public Dictionary<string, string> stringsToEdit
        {
            get
            {
                _stringsToEdit ??= new Dictionary<string, string>();
                return _stringsToEdit;
            }
        }
    }
}