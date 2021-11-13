using CommonAPI.Systems;
using HarmonyLib;

namespace CommonAPI.Patches
{
    [HarmonyPatch]
    static class UIBuildMenuPatch
    {
        [HarmonyPatch(typeof(UIBuildMenu), "StaticLoad")]
        [HarmonyPostfix]
        public static void Postfix(ItemProto[,] ___protos)
        {
            foreach (var kv in ProtoRegistry.items)
            {
                int buildIndex = kv.Value.BuildIndex;
                if (buildIndex > 0)
                {
                    int num = buildIndex / 100;
                    int num2 = buildIndex % 100;
                    if (num <= 12 && num2 <= 12)
                    {
                        ___protos[num, num2] = kv.Value;
                    }
                }
            }
        }
    }
}