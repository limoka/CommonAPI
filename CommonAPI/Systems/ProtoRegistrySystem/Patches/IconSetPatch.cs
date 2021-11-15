using System.Reflection;
using CommonAPI.Systems;
using HarmonyLib;
using UnityEngine;

namespace CommonAPI.Patches
{
    [HarmonyPatch]
    public static class IconSetPatch
    {
        [HarmonyPatch(typeof(IconSet), "Create")]
        [HarmonyPostfix]
        public static void AddIconDescs(IconSet __instance)
        {
            foreach (var kv in ProtoRegistry.itemIconDescs)
            {
                if (kv.Key <= 0 || kv.Key >= 12000) continue;
                uint index = __instance.itemIconIndex[kv.Key];
                if (index <= 0U) continue;

                IconToolNew.IconDesc desc = kv.Value;

                FieldInfo[] fields = typeof(IconToolNew.IconDesc).GetFields(BindingFlags.Instance | BindingFlags.Public);
                uint offset = 0;
                
                foreach (var field in fields)
                {
                    if (field.FieldType == typeof(float))
                    {
                        __instance.itemDescArr[index * 40U + offset++] = (float)field.GetValue(desc);
                    }else if (field.FieldType == typeof(Color))
                    {
                        Color color = (Color)field.GetValue(desc);
                        __instance.itemDescArr[index * 40U + offset++] = color.r;
                        __instance.itemDescArr[index * 40U + offset++] = color.g;
                        __instance.itemDescArr[index * 40U + offset++] = color.b;
                        __instance.itemDescArr[index * 40U + offset++] = color.a;
                    }
                }
            }

            __instance.itemIconDescBuffer.SetData(__instance.itemDescArr);
        }
    }
}