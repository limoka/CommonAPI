using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using CommonAPI.Systems;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CommonAPI.Patches
{

    [HarmonyPatch]
    public static class UIReplicatorPatch
    {
        private static List<UITabButton> tabs;

        [HarmonyPatch(typeof(UIReplicatorWindow), "_OnCreate")]
        [HarmonyPostfix]
        public static void Create(UIReplicatorWindow __instance)
        {
            var datas = TabSystem.GetAllTabs();
            tabs = new List<UITabButton>(datas.Length - 3);
            
            foreach (TabData tab in datas)
            {
                if (tab == null) continue;
                
                GameObject button = Object.Instantiate(TabSystem.GetTabPrefab(), __instance.recipeGroup, false);
                ((RectTransform)button.transform).anchoredPosition = new Vector2( 70 * tab.tabIndex - 95, 50); 
                UITabButton tabButton = button.GetComponent<UITabButton>();
                Sprite sprite = Resources.Load<Sprite>(tab.tabIconPath);
                tabButton.Init(sprite, tab.tabName, tab.tabIndex, __instance.OnTypeButtonClick);
                tabs.Add(tabButton);
            }
        }
        
        [HarmonyPatch(typeof(UIReplicatorWindow), "SetSelectedRecipe")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> AddNewProperty(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher matcher = new CodeMatcher(instructions)
                .MatchForward(false,
                    new CodeMatch(OpCodes.Ldloc_0),
                    new CodeMatch(OpCodes.Ldc_I4_1)
                ).Advance(2);
                
                Label continueLabel = (Label)matcher.Operand;

                matcher.Advance(-1)
                    .InsertAndAdvance(Transpilers.EmitDelegate<Func<int, bool>>(type => type >= 3 && type < TabSystem.tabsRegistry.data.Count))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Brtrue_S, continueLabel))
                    .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_0));
                
            return matcher.InstructionEnumeration();
        }
        
        
        [HarmonyPatch(typeof(UIReplicatorWindow), "OnTypeButtonClick")]
        [HarmonyPostfix]
        public static void OnTypeClicked(int type)
        {
            foreach (UITabButton tab in tabs)
            {
                tab.TabSelected(type);
            }
        }
    }
}