using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CommonAPI.Systems.ModLocalization.Patch;

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
                int languageId = InterpretLanguageIdFromName(directoryName);
                
                if (languageId == 0)
                {
                    CommonAPIPlugin.logger.LogWarning($"Failed to load translations from folder: {directoryName}: not a valid language");
                    continue;
                }

                var subFiles = Directory.EnumerateFiles(directory);
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

                            if (!LoadTranslationFromLine(line, languageId))
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
            int languageId = InterpretLanguageIdFromName(languageAbbr);
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
                if (!LoadTranslationFromLine(line, languageId))
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
            var english = GetOrCreateLanguageDict(Localization.LCID_ENUS);
            english[key] = enTrans;
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
            var english = GetOrCreateLanguageDict(Localization.LCID_ENUS);
            english[key] = enTrans;
            if (!string.IsNullOrEmpty(cnTrans))
            {
                var chinese = GetOrCreateLanguageDict(Localization.LCID_ZHCN);
                chinese[key] = cnTrans;
            }

            if (!string.IsNullOrEmpty(frTrans))
            {
                var french = GetOrCreateLanguageDict(Localization.LCID_FRFR);
                french[key] = frTrans;
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
                int languageId = InterpretLanguageIdFromName(pair.Key);
                if (languageId == 0)
                {
                    CommonAPIPlugin.logger.LogWarning($"Failed to interpret '{pair.Key}' as a language!");
                    continue;
                }

                var languageData = GetOrCreateLanguageDict(languageId);
                languageData[key] = pair.Value;
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
            var english = GetOrCreateEditLanguageDict(Localization.LCID_ENUS);
            english[key] = enTrans;
            if (!string.IsNullOrEmpty(cnTrans))
            {
                var chinese = GetOrCreateEditLanguageDict(Localization.LCID_ZHCN);
                chinese[key] = cnTrans;
            }

            if (!string.IsNullOrEmpty(frTrans))
            {
                var french = GetOrCreateEditLanguageDict(Localization.LCID_FRFR);
                french[key] = frTrans;
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
                int languageId = InterpretLanguageIdFromName(pair.Key);
                if (languageId == 0)
                {
                    CommonAPIPlugin.logger.LogWarning($"Failed to interpret '{pair.Key}' as a language!");
                    continue;
                }

                var languageData = GetOrCreateEditLanguageDict(languageId);
                languageData[key] = pair.Value;
            }
        }
        
        #endregion

        #region PrivateImplementation

        private static string[] separators = { "\r\n", "\r", "\n" };
        
        internal static LocalizationModule Instance => CommonAPIPlugin.GetModuleInstance<LocalizationModule>();
        internal const int FIRST_MOD_LANGUAGE_ID = 50000;

        internal static Registry languageRegistry = new Registry(FIRST_MOD_LANGUAGE_ID, true);
        
        internal static Dictionary<int, Localization.Language> modLanguages = new Dictionary<int, Localization.Language>();
        internal static Dictionary<int, Dictionary<string, string>> modStrings = new Dictionary<int, Dictionary<string, string>>();
        internal static Dictionary<int, Dictionary<string, string>> stringsToEdit = new Dictionary<int, Dictionary<string, string>>();

        internal override void SetHooks()
        {
            CommonAPIPlugin.harmony.PatchAll(typeof(Localization_Patch));
        }

        internal override void Load() { }

        private static int InterpretLanguageIdFromName(string name)
        {
            if (int.TryParse(name, out int languageId))
            {
                return languageId;
            }

            switch (name)
            {
                case "enUS": return Localization.LCID_ENUS;
                case "zhCN": return Localization.LCID_ZHCN;
                case "frFR": return Localization.LCID_FRFR;
            }

            var language = modLanguages.Values.FirstOrDefault(language => language.abbr.Equals(name));
            if (language == null) return 0;

            return language.lcId;
        }
        
        private static readonly StringBuilder tempBuffer1 = new StringBuilder();
        private static readonly StringBuilder tempBuffer2 = new StringBuilder();

        private static Dictionary<string, string> GetOrCreateLanguageDict(int languageId)
        {
            if (modStrings.ContainsKey(languageId))
            {
                return modStrings[languageId];
            }

            var strings = new Dictionary<string, string>();
            modStrings[languageId] = strings;
            return strings;
        }
        
        private static Dictionary<string, string> GetOrCreateEditLanguageDict(int languageId)
        {
            if (stringsToEdit.ContainsKey(languageId))
            {
                return stringsToEdit[languageId];
            }

            var strings = new Dictionary<string, string>();
            stringsToEdit[languageId] = strings;
            return strings;
        }

        private static bool LoadTranslationFromLine(string line, int languageId)
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

            var strings = GetOrCreateLanguageDict(languageId);
            
            strings[key] = translation;
            return true;
        }

        #endregion
    }
}