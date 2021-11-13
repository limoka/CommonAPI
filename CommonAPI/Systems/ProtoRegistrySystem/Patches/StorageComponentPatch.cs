using CommonAPI.Systems;
using HarmonyLib;

namespace CommonAPI.Patches
{
    //Fix item stack size not working
    [HarmonyPatch]
    static class StorageComponentPatch
    {
        private static bool staticLoad;

        [HarmonyPatch(typeof(StorageComponent), "LoadStatic")]
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (!staticLoad)
            {
                foreach (var kv in ProtoRegistry.items)
                {
                    StorageComponent.itemIsFuel[kv.Key] = (kv.Value.HeatValue > 0L);
                    StorageComponent.itemStackCount[kv.Key] = kv.Value.StackSize;
                }

                staticLoad = true;
            }
        }
    }
}