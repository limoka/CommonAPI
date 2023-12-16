using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace CommonAPI.Systems.ModLocalization.Patch
{
    public static class Localization_Patch
    {
        [HarmonyPatch(typeof(Localization), nameof(Localization.LoadSettings))]
        [HarmonyPostfix]
        public static void OnLoadSettings()
        {
            CommonAPIPlugin.logger.LogInfo("Editing localizations!");

            AddModLanguages();
            AddModKeys();
        }

        [HarmonyPatch(typeof(Localization), nameof(Localization.LoadLanguage))]
        [HarmonyPostfix]
        public static void OnLoadLanguage(int index)
        {
            int languageCount = Localization.Languages.Length;
            if ((ulong)index >= (ulong)languageCount)
            {
                return;
            }

            var language = Localization.Languages[index];

            AddModTranslations(index, language);
            EditTranslations(index, language);
        }

        [HarmonyPatch(typeof(Localization), nameof(Localization.LoadLanguage))]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(true,
                    new CodeMatch(i => i.IsLdloc()),
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(StringBuilder), nameof(StringBuilder.Clear))),
                    new CodeMatch(OpCodes.Pop),
                    new CodeMatch(i => i.IsLdloc()),
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(StringBuilder), nameof(StringBuilder.Clear))),
                    new CodeMatch(OpCodes.Pop));

            matcher.Advance(1);
            matcher.InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0))
                .InsertAndAdvance(Transpilers.EmitDelegate<Action<int>>(index =>
                {
                    var language = Localization.Languages[index];
                    
                    if (language.lcId >= LocalizationModule.FIRST_MOD_LANGUAGE_ID && language.fallback != 0)
                    {
                        LoadFallbackInfo(index, language);
                    }
                }));

            return matcher.InstructionEnumeration();
        }

        private static void LoadFallbackInfo(int index, Localization.Language language)
        {
            if (language.fallback == language.lcId) return;
            CommonAPIPlugin.logger.LogInfo($"Loading fallback info for {language.abbr}");

            int fallbackIndex = Array.FindIndex(Localization.Languages, other => other.lcId == language.fallback);
            if (fallbackIndex < 0) return;

            if (!Localization.LanguageLoaded(fallbackIndex))
            {
                CommonAPIPlugin.logger.LogDebug("Fallback language isn't loaded, loading it now");
                if (!Localization.LoadLanguage(fallbackIndex)) return;
            }

            string[] ourStrings = Localization.strings[index];
            string[] fallbackStrings = Localization.strings[fallbackIndex];

            for (int i = 0; i < ourStrings.Length; i++)
            {
                if (string.IsNullOrEmpty(ourStrings[i]) &&
                    !string.IsNullOrEmpty(fallbackStrings[i]))
                {
                    ourStrings[i] = fallbackStrings[i];
                }
            }
        }

        private static void AddModTranslations(int index, Localization.Language language)
        {
            if (!LocalizationModule.modStrings.ContainsKey(language.lcId)) return;

            CommonAPIPlugin.logger.LogDebug($"Applying translations for {language.abbr}");
            string[] strings = Localization.strings[index];

            var modStrings = LocalizationModule.modStrings[language.lcId];
            foreach (var pair in modStrings)
            {
                // Ignore key if it does not exist in namesIndexer: it must be invalid!
                if (!Localization.namesIndexer.ContainsKey(pair.Key)) continue;

                int nameIndex = Localization.namesIndexer[pair.Key];
                strings[nameIndex] = pair.Value;
            }
        }

        private static void EditTranslations(int index, Localization.Language language)
        {
            if (!LocalizationModule.stringsToEdit.ContainsKey(language.lcId)) return;

            CommonAPIPlugin.logger.LogDebug($"Editing translations for {language.abbr}");
            string[] strings = Localization.strings[index];

            var modStrings = LocalizationModule.stringsToEdit[language.lcId];
            foreach (var pair in modStrings)
            {
                // Ignore key if it does not exist in namesIndexer: it must be invalid!
                if (!Localization.namesIndexer.ContainsKey(pair.Key))
                {
                    CommonAPIPlugin.logger.LogWarning($"Tried to edit key '{pair.Key}', which does not exist!");
                    continue;
                }

                int nameIndex = Localization.namesIndexer[pair.Key];
                strings[nameIndex] = pair.Value;
            }
        }

        private static void AddModLanguages()
        {
            var languagesList = Localization.Languages.ToList();
            foreach (var pair in LocalizationModule.modLanguages)
            {
                var language = languagesList.FirstOrDefault(language => pair.Value.lcId == language.lcId || pair.Value.abbr == language.abbr);
                if (language != null)
                {
                    CommonAPIPlugin.logger.LogWarning($"Ignoring mod language '{pair.Value.abbr}', because it already exists!");
                    continue;
                }

                languagesList.Add(pair.Value);
            }

            Localization.Languages = languagesList.ToArray();

            int languageCount = Localization.Languages.Length;

            // At this stage these aren't filled with anything.
            Localization.strings = new string[languageCount][];
            Localization.floats = new float[languageCount][];
        }

        private static void AddModKeys()
        {
            var unqiueKeys = LocalizationModule.modStrings
                .SelectMany(pair => pair.Value.Keys)
                .Distinct()
                .ToList();

            foreach (string key in unqiueKeys)
            {
                if (!Localization.namesIndexer.ContainsKey(key))
                {
                    Localization.namesIndexer.Add(key, Localization.namesIndexer.Count);
                }
            }
        }
    }
}