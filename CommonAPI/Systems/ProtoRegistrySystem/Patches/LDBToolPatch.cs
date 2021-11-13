using System.Collections.Generic;
using BepInEx.Configuration;
using CommonAPI.Systems;
using HarmonyLib;
using UnityEngine;
using xiaoye97;

namespace CommonAPI.Patches
{
    [HarmonyPatch]
    static class LDBToolPatch
    {
        [HarmonyPatch(typeof(LDBTool), "PreAddProto")]
        [HarmonyPrefix]
        public static void AddStrings(ProtoType protoType, Proto proto)
        {
            if (!(proto is StringProto))
                return;

            int id = ProtoRegistry.FindAvailableStringID();
            proto.ID = id;
            ProtoRegistry.strings.Add(id, (StringProto) proto);
        }


        [HarmonyPatch(typeof(LDBTool), "IdBind")]
        [HarmonyPrefix]
        public static bool FixStringBinding2(ProtoType protoType, Proto proto)
        {
            return !(proto is StringProto);
        }

        [HarmonyPatch(typeof(LDBTool), "StringBind")]
        [HarmonyPrefix]
        public static bool FixStringBinding(ProtoType protoType, Proto proto)
        {
            if (!(proto is StringProto))
                return false;

            StringProto stringProto = (StringProto) proto;
            ConfigEntry<string> configEntry1 =
                LDBTool.CustomStringZHCN.Bind(protoType.ToString(), stringProto.Name, stringProto.ZHCN, stringProto.Name);
            ConfigEntry<string> configEntry2 =
                LDBTool.CustomStringENUS.Bind(protoType.ToString(), stringProto.Name, stringProto.ENUS, stringProto.Name);
            ConfigEntry<string> configEntry3 =
                LDBTool.CustomStringFRFR.Bind(protoType.ToString(), stringProto.Name, stringProto.FRFR, stringProto.Name);
            stringProto.ZHCN = configEntry1.Value;
            stringProto.ENUS = configEntry2.Value;
            stringProto.FRFR = configEntry3.Value;
            if (LDBTool.ZHCNDict != null)
            {
                if (!LDBTool.ZHCNDict.ContainsKey(protoType))
                    LDBTool.ZHCNDict.Add(protoType, new Dictionary<string, ConfigEntry<string>>());
                if (LDBTool.ZHCNDict[protoType].ContainsKey(proto.Name))
                {
                    Debug.LogError($"[LDBTool.CustomLocalization.ZHCN]Name:{proto.Name} There is a conflict, please check.");
                    Debug.LogError($"[LDBTool.CustomLocalization.ZHCN]姓名:{proto.Name} 存在冲突，请检查。");
                }
                else
                    LDBTool.ZHCNDict[protoType].Add(proto.Name, configEntry1);
            }

            if (LDBTool.ENUSDict != null)
            {
                if (!LDBTool.ENUSDict.ContainsKey(protoType))
                    LDBTool.ENUSDict.Add(protoType, new Dictionary<string, ConfigEntry<string>>());
                if (LDBTool.ENUSDict[protoType].ContainsKey(proto.Name))
                {
                    Debug.LogError($"[LDBTool.CustomLocalization.ENUS]Name:{proto.Name} There is a conflict, please check.");
                    Debug.LogError($"[LDBTool.CustomLocalization.ENUS]姓名:{proto.Name} 存在冲突，请检查。");
                }
                else
                    LDBTool.ENUSDict[protoType].Add(proto.Name, configEntry2);
            }

            if (LDBTool.FRFRDict != null)
            {
                if (!LDBTool.FRFRDict.ContainsKey(protoType))
                    LDBTool.FRFRDict.Add(protoType, new Dictionary<string, ConfigEntry<string>>());
                if (LDBTool.FRFRDict[protoType].ContainsKey(proto.Name))
                {
                    Debug.LogError($"[LDBTool.CustomLocalization.FRFR]Name:{proto.Name} There is a conflict, please check.");
                    Debug.LogError($"[LDBTool.CustomLocalization.FRFR]姓名:{proto.Name} 存在冲突，请检查。");
                }
                else
                    LDBTool.FRFRDict[protoType].Add(proto.Name, configEntry3);
            }

            return false;
        }
    }
}