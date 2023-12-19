using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CommonAPI.Systems.ModLocalization.Patch;
using UnityEngine;
using UnityEngine.UI;

namespace CommonAPI.Systems.ModLocalization
{
    /// <summary>
    /// Submodule that helps to add and modify game translations
    /// </summary>
    public class LocalizationModule : BaseSubmodule
    {
        #region PublicInterface

        /// <summary>
        /// Add custom language. Do not set its 'lcId', id's will be assigned automatically. To add translations via file or via other means use language 'abbr'
        /// </summary>
        /// <param name="language">Info about new language</param>
        public static int AddLanguage(Localization.Language language)
        {
            Instance.ThrowIfNotLoaded();
            int id = languageRegistry.Register(language.abbr);

            language.lcId = id;
            modLanguages[id] = language;
            return id;
        }

        /// <summary>
        /// Register existing system font to be used with this language.
        /// Get list of available fonts via <see cref="Font.GetOSInstalledFontNames"/>
        /// </summary>
        /// <param name="languageId">Id of the target language</param>
        /// <param name="systemFontName">System font name</param>
        public static void RegisterFontForLanguage(int languageId, string systemFontName)
        {
            var font = Font.CreateDynamicFontFromOSFont(systemFontName, 12);
            var languageData = GetOrCreateExtraData(languageId);
            languageData.customFont = font;
        }

        /// <summary>
        /// Load fonts from asset bundle at path and register it for use with this language
        /// The first font asset found inside the bundle will be used
        /// </summary>
        /// <param name="languageId">Id of the target language</param>
        /// <param name="assetBundlePath">Full path to the asset bundle</param>
        public static void RegisterFontForLanguageFromBundle(int languageId, string assetBundlePath)
        {
            AssetBundle fontBundle = AssetBundle.LoadFromFile(assetBundlePath);
            if (fontBundle == null)
            {
                CommonAPIPlugin.logger.LogWarning($"Failed to load asset bundle from path: {assetBundlePath}");
                return;
            }

            var fonts = fontBundle.LoadAllAssets<Font>();
            if (fonts == null || fonts.Length <= 0)
            {
                CommonAPIPlugin.logger.LogWarning($"Asset bundle at path {assetBundlePath} does not contain any fonts!");
                return;
            }

            var languageData = GetOrCreateExtraData(languageId);
            languageData.customFont = fonts[0];
        }
        
        /// <summary>
        /// Get language id by it's abbreviation
        /// </summary>
        /// <param name="abbreviation">Language abbreviation</param>
        /// <returns></returns>
        public static int GetLanguageId(string abbreviation)
        {
            if (int.TryParse(abbreviation, out int languageId))
            {
                return languageId;
            }

            switch (abbreviation)
            {
                case "enUS": return Localization.LCID_ENUS;
                case "zhCN": return Localization.LCID_ZHCN;
                case "frFR": return Localization.LCID_FRFR;
            }

            var language = modLanguages.Values.FirstOrDefault(language => language.abbr.Equals(abbreviation));
            if (language == null) return 0;

            return language.lcId;
        }
        
        /// <summary>
        /// Load mod translations from path. This path should point to your mod plugin folder, and have the following structure:
        /// <code>
        /// Locale (pass path to this) <br/>
        ///   - enUS <br/>
        ///   - zhCN <br/>
        /// </code>
        /// Each folder should contain at least one txt file in tab separated format of: key -> (unused) -> (unused) -> value <br/>
        /// Two unused values are here to have compatibility with vanilla translation file format <br/>
        /// </summary>
        /// <param name="path">Path to locale root folder</param>
        public static void LoadTranslationsFromFolder(string path)
        {
            Instance.ThrowIfNotLoaded();
            if (!Directory.Exists(path))
            {
                CommonAPIPlugin.logger.LogError($"Failed to load translations: {path} is not a valid directory!");
                return;
            }

            var subDirectories = Directory.EnumerateDirectories(path);
            int loadedCount = 0;
            int failCount = 0;

            foreach (string directory in subDirectories)
            {
                string directoryName = Path.GetFileName(directory);
                int languageId = GetLanguageId(directoryName);
                
                if (languageId == 0)
                {
                    CommonAPIPlugin.logger.LogWarning($"Failed to load translations from folder: {directoryName}: not a valid language");
                    continue;
                }

                var subFiles = Directory.EnumerateFiles(directory).ToList();
                
                subFiles.Sort((path1, path2) =>
                {
                    var file1 = Path.GetFileNameWithoutExtension(path1);
                    var file2 = Path.GetFileNameWithoutExtension(path2);

                    var index1 = GetOrder(file1);
                    var index2 = GetOrder(file2);

                    return index1.CompareTo(index2);
                });

                foreach (string file in subFiles)
                {
                    if (!File.Exists(file)) continue;
                    
                    try
                    {
                        var streamReader = new StreamReader(file, true);
                        while (true)
                        {
                            string line = streamReader.ReadLine();
                            if (line == null) break;
                            if (line.Equals(string.Empty)) continue;

                            if (!LoadTranslationFromLine(line, languageId, true))
                                failCount++;
                        }
                    }
                    catch (Exception e)
                    {
                        CommonAPIPlugin.logger.LogError($"Failed to load translations from file: {file}: {e.Message}\n{e.StackTrace}");
                    }
                }

                loadedCount++;
            }

            if (loadedCount == 0)
            {
                CommonAPIPlugin.logger.LogWarning($"Failed to load translations from '{path}': found no language subdirectories!");
                return;
            }
            
            if (failCount == 0)
            {
                CommonAPIPlugin.logger.LogInfo($"Successfully loaded translations from '{path}'");
            }
            else
            {
                CommonAPIPlugin.logger.LogWarning($"{failCount} errors occured while loading translations from '{path}'!");
            }
        }

        /// <summary>
        /// Load localizations from string data. Use this to load localizations from asset bundles or other locations.
        /// This method uses format identical to <see cref="LoadTranslationsFromFolder"/>
        /// </summary>
        /// <param name="data">Raw file data</param>
        /// <param name="languageAbbr">Abbreviation of target language, for example: 'enUS'</param>
        public static void LoadTranslationsFromString(string data, string languageAbbr)
        {
            Instance.ThrowIfNotLoaded();
            int languageId = GetLanguageId(languageAbbr);
            if (languageId == 0)
            {
                CommonAPIPlugin.logger.LogWarning($"Failed to interpret '{languageAbbr}' as a language!");
                return;
            }
            
            LoadTranslationsFromString(data, languageId);
        }
        
        /// <summary>
        /// Load localizations from string data. Use this to load localizations from asset bundles or other locations.
        /// This method uses format identical to <see cref="LoadTranslationsFromFolder"/>
        /// </summary>
        /// <param name="data">Raw file data</param>
        /// <param name="languageId">ID of the target language</param>
        public static void LoadTranslationsFromString(string data, int languageId)
        {
            Instance.ThrowIfNotLoaded();
            var lines = data.Split(separators, StringSplitOptions.None);
            int failCount = 0;
            
            foreach (string line in lines)
            {
                if (!LoadTranslationFromLine(line, languageId, false))
                    failCount++;
            }

            if (failCount == 0)
            {
                CommonAPIPlugin.logger.LogInfo("Successfully loaded translations from string data");
            }
            else
            {
                CommonAPIPlugin.logger.LogWarning($"{failCount} errors occured while loading translations from string data for language {languageId}!");
            }
        }

        /// <summary>
        /// Register translation for english.
        /// Please note that you CANNOT override translations using this method. For that use <see cref="EditTranslation(string,string)"/>
        /// </summary>
        /// <param name="key">Translation key</param>
        /// <param name="enTrans">English translation</param>
        public static void RegisterTranslation(string key, string enTrans)
        {
            Instance.ThrowIfNotLoaded();
            var english = GetOrCreateExtraData(Localization.LCID_ENUS);
            
            if (!english.modStrings.ContainsKey(key))
                english.modStrings[key] = enTrans;
            else
            {
                CommonAPIPlugin.logger.LogWarning("Trying to override translations via RegisterTranslation, this is not supported. Use EditTranslation");
            }
        }

        /// <summary>
        /// Register translation for languages.
        /// Please note that you CANNOT override translations using this method. For that use <see cref="EditTranslation(string,string,string,string)"/>
        /// </summary>
        /// <param name="key">Translation key</param>
        /// <param name="enTrans">English translation</param>
        /// <param name="cnTrans">Chinese translation</param>
        /// <param name="frTrans">French translation</param>
        public static void RegisterTranslation(string key, string enTrans, string cnTrans, string frTrans)
        {
            Instance.ThrowIfNotLoaded();
            var english = GetOrCreateExtraData(Localization.LCID_ENUS);
            if (!english.modStrings.ContainsKey(key))
                english.modStrings[key] = enTrans;
            else
                CommonAPIPlugin.logger.LogWarning($"Trying to override translation '{key}' via RegisterTranslation, this is not supported. Use EditTranslation");
            
            if (!string.IsNullOrEmpty(cnTrans))
            {
                var chinese = GetOrCreateExtraData(Localization.LCID_ZHCN);

                if (!chinese.modStrings.ContainsKey(key))
                    chinese.modStrings[key] = cnTrans;
                else
                    CommonAPIPlugin.logger.LogWarning($"Trying to override translation '{key}' via RegisterTranslation, this is not supported. Use EditTranslation");
            }
            

            if (!string.IsNullOrEmpty(frTrans))
            {
                var french = GetOrCreateExtraData(Localization.LCID_FRFR);

                if (!french.modStrings.ContainsKey(key))
                    french.modStrings[key] = frTrans;
                else
                    CommonAPIPlugin.logger.LogWarning($"Trying to override translation '{key}' via RegisterTranslation, this is not supported. Use EditTranslation");

            }
        }

        /// <summary>
        /// Register translation for languages.
        /// Please note that you CANNOT override translations using this method. For that use <see cref="EditTranslation(string,Dictionary{string, string})"/>
        /// </summary>
        /// <param name="key">Translation key</param>
        /// <param name="translations">Dictionary containing all needed translations, key is language id or abbr</param>
        public static void RegisterTranslation(string key, Dictionary<string, string> translations)
        {
            Instance.ThrowIfNotLoaded();
            foreach (KeyValuePair<string,string> pair in translations)
            {
                int languageId = GetLanguageId(pair.Key);
                if (languageId == 0)
                {
                    CommonAPIPlugin.logger.LogWarning($"Failed to interpret '{pair.Key}' as a language!");
                    continue;
                }

                var languageData = GetOrCreateExtraData(languageId);

                if (!languageData.modStrings.ContainsKey(key))
                    languageData.modStrings[key] = pair.Value;
                else
                    CommonAPIPlugin.logger.LogWarning($"Trying to override translation '{key}' via RegisterTranslation, this is not supported. Use EditTranslation");

            }
        }

        /// <summary>
        /// Edit english translation.
        /// </summary>
        /// <param name="key">Translation key</param>
        /// <param name="enTrans">English translation</param>
        public static void EditTranslation(string key, string enTrans)
        {
            EditTranslation(key, enTrans, "", "");
        }

        /// <summary>
        /// Edit english, chinese or french translations
        /// </summary>
        /// <param name="key">Translation key</param>
        /// <param name="enTrans">English translation</param>
        /// <param name="cnTrans">Chinese translation</param>
        /// <param name="frTrans">French translation</param>
        public static void EditTranslation(string key, string enTrans, string cnTrans, string frTrans)
        {
            Instance.ThrowIfNotLoaded();
            var english = GetOrCreateExtraData(Localization.LCID_ENUS);
            english.stringsToEdit[key] = enTrans;
            if (!string.IsNullOrEmpty(cnTrans))
            {
                var chinese = GetOrCreateExtraData(Localization.LCID_ZHCN);
                chinese.stringsToEdit[key] = cnTrans;
            }

            if (!string.IsNullOrEmpty(frTrans))
            {
                var french = GetOrCreateExtraData(Localization.LCID_FRFR);
                french.stringsToEdit[key] = frTrans;
            }
        }

        /// <summary>
        /// Edit translation of many languages
        /// </summary>
        /// <param name="key">Translation key</param>
        /// <param name="translations">Dictionary containing all needed translations, key is language id or abbr</param>
        public static void EditTranslation(string key, Dictionary<string, string> translations)
        {
            Instance.ThrowIfNotLoaded();
            foreach (KeyValuePair<string,string> pair in translations)
            {
                int languageId = GetLanguageId(pair.Key);
                if (languageId == 0)
                {
                    CommonAPIPlugin.logger.LogWarning($"Failed to interpret '{pair.Key}' as a language!");
                    continue;
                }

                var languageData = GetOrCreateExtraData(languageId);
                languageData.modStrings[key] = pair.Value;
            }
        }
        
        #endregion

        #region PrivateImplementation

        private static string[] separators = { "\r\n", "\r", "\n" };
        
        internal static LocalizationModule Instance => CommonAPIPlugin.GetModuleInstance<LocalizationModule>();
        internal const int FIRST_MOD_LANGUAGE_ID = 50000;

        internal static Registry languageRegistry = new Registry(FIRST_MOD_LANGUAGE_ID, true);
        

        internal static Dictionary<int, Localization.Language> modLanguages = new Dictionary<int, Localization.Language>();
        internal static Dictionary<int, ExtraLanguageData> extraDataEntires = new Dictionary<int, ExtraLanguageData>();
        
        private static readonly Dictionary<Text, TextDefaultFont> _textReferences = new Dictionary<Text, TextDefaultFont>();
        private static readonly Dictionary<string, int> fileOrder = new Dictionary<string, int>();

        internal override void SetHooks()
        {
            CommonAPIPlugin.harmony.PatchAll(typeof(Localization_Patch));
            CommonAPIPlugin.harmony.PatchAll(typeof(Text_Patch));
            CommonAPIPlugin.harmony.PatchAll(typeof(UIBehaviour_Patch));
        }

        internal override void Load()
        {
            Localization.OnLanguageChange += RefreshAllTextElements;
            
            //TODO too lazy to read the file right now
            fileOrder["base"] = 0;
            fileOrder["combat"] = 0;
            fileOrder["prototype"] = -1;
            fileOrder["dictionary"] = 3;
        }

        private static int GetOrder(string fileName)
        {
            if (fileOrder.TryGetValue(fileName, out int order))
            {
                return order;
            }

            return -9;
        }

        private static readonly StringBuilder tempBuffer1 = new StringBuilder();
        private static readonly StringBuilder tempBuffer2 = new StringBuilder();

        private static ExtraLanguageData GetOrCreateExtraData(int languageId)
        {
            if (extraDataEntires.ContainsKey(languageId))
            {
                return extraDataEntires[languageId];
            }

            var data = new ExtraLanguageData();
            extraDataEntires[languageId] = data;
            return data;
        }

        private static bool LoadTranslationFromLine(string line, int languageId, bool allowOverride)
        {
            if (string.IsNullOrEmpty(line)) return false;
            if (languageId == 0) return false;

            var parts = line.Split('\t');
            if (parts.Length != 4)
            {
                CommonAPIPlugin.logger.LogWarning($"Translation line '{line}' is invalid: wrong tab count!");
                return false;
            }

            tempBuffer1.Clear();
            tempBuffer1.Append(parts[0]);
            string key = Localization.UnescapeString(tempBuffer1, tempBuffer2).ToString();
            if (string.IsNullOrEmpty(key))
            {
                CommonAPIPlugin.logger.LogWarning($"Translation line '{line}' is invalid: key is empty!");
                return false;
            }

            tempBuffer1.Clear();
            tempBuffer1.Append(parts[3]);
            string translation = Localization.UnescapeString(tempBuffer1, tempBuffer2).ToString();
            if (string.IsNullOrEmpty(translation))
            {
                CommonAPIPlugin.logger.LogWarning($"Translation line '{line}' is invalid: translation is empty!");
                return false;
            }

            var strings = GetOrCreateExtraData(languageId);
            
            if (!strings.modStrings.ContainsKey(key))
                strings.modStrings[key] = translation;
            else
            {
                if (!allowOverride)
                {
                    CommonAPIPlugin.logger.LogWarning($"Trying to override translation '{key}' via local files, this is not supported. Use EditTranslation");
                    return false;
                }
                strings.modStrings[key] = translation;
            }

            return true;
        }
        
        internal static TextDefaultFont Get(Text text)
        {
            _textReferences.TryGetValue(text, out var value);
            return value;
        }
        
        internal static void Add(Text text)
        {
            var find = Get(text);
            if (find == null)
            {
                _textReferences.Add(text, new TextDefaultFont(text));
            }
        }
        
        internal static void Remove(Text text)
        {
            var find = Get(text);
            if (find != null)
            {
                _textReferences.Remove(text);
            }        
        }

        public static void RefreshAllTextElements()
        {
            int currentLanguageId = Localization.CurrentLanguageLCID;
            if (!extraDataEntires.ContainsKey(currentLanguageId)) return;

            var languageData = extraDataEntires[currentLanguageId];

            foreach (var text in _textReferences)
            {
                TextDefaultFont textData = text.Value;
                Font fontToUse = languageData.customFont != null ? languageData.customFont : textData.DefaultFont;
                textData.UseCustomFontImmediate(fontToUse);
            }
        }

        #endregion
    }
}