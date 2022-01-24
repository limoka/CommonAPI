using System.Collections.Generic;
using System.IO;

namespace CommonAPI.Systems
{
    /// <summary>
    /// This class handles removing machines placed in the world from uninstalled mods or mods which have some content disabled after save load.
    /// All missing machines data is removed from save data upon saving. 
    /// </summary>
    internal static class ModProtoHistory
    {
        public static readonly Dictionary<string, List<int>> modMachines = new Dictionary<string, List<int>>();
        public static readonly Dictionary<string, List<int>> missingMachines = new Dictionary<string, List<int>>();

        private static readonly Dictionary<string, int> removedMachines = new Dictionary<string, int>();
        
        internal static void AddModMachine(string modGUID, int itemID)
        {
            if (modMachines.ContainsKey(modGUID))
            {
                if (!modMachines[modGUID].Contains(itemID))
                    modMachines[modGUID].Add(itemID);
            }
            else
            {
                modMachines.Add(modGUID, new List<int> {itemID});
            }
        }
        
        internal static void AddMissingMachine(string modGUID, int itemID)
        {
            if (missingMachines.ContainsKey(modGUID))
            {
                if (!missingMachines[modGUID].Contains(itemID))
                {
                    missingMachines[modGUID].Add(itemID);
                    CommonAPIPlugin.logger.LogInfo($"Machine from {modGUID} mod with proto ID {itemID} is missing!");
                }
            }
            else
            {
                missingMachines.Add(modGUID, new List<int> {itemID});
                CommonAPIPlugin.logger.LogInfo($"Machine from {modGUID} mod with proto ID {itemID} is missing!");
            }
        }

        private static void IncrementRemoved(string modGUID)
        {
            if (removedMachines.ContainsKey(modGUID))
            {
                removedMachines[modGUID]++;
            }
            else
            {
                removedMachines.Add(modGUID, 1);
            }
        }

        internal static bool IsMachineMissing(int itemID, out string modGUID)
        {
            foreach (var modMacines in missingMachines)
            {
                if (modMacines.Value.Contains(itemID))
                {
                    modGUID = modMacines.Key;
                    return true;
                }
                
            }

            modGUID = "";
            return false;
        }

        private static void CheckMissingMachines()
        {
            CommonAPIPlugin.logger.LogInfo("Checking missing machines!");
            for (int i = 0; i < GameMain.data.factoryCount; i++)
            {
                PlanetFactory factory = GameMain.data.factories[i];
                if (factory == null) continue;

                for (int j = 1; j < factory.entityCursor; j++)
                {
                    if (factory.entityPool[j].id != j) continue;

                    int protoId = factory.entityPool[j].protoId;
                    if (IsMachineMissing(protoId, out string modGuid))
                    {
                        CommonAPIPlugin.logger.LogInfo($"Removing entity ID: {j}, proto ID: {protoId} from mod {modGuid}");
                        factory.RemoveEntityWithComponents(j);
                        IncrementRemoved(modGuid);
                    }
                }
            }
        }

        internal static void DisplayRemovedMessage()
        {
            if (removedMachines.Count <= 0) return;
            string text = "";

            foreach (var mod in removedMachines)
            {
                if (mod.Value <= 0) continue;
                
                text = $"{text}\r\nRemoved {mod.Value} machines from {mod.Key} mod";
            }
            
            if (text.Equals("")) return;

            UIMessageBox.Show("ModItemMissingWarnTitle".Translate(), "ModItemMissingWarnDesc".Translate() + text, "确定".Translate(), 0);
        }
        


        internal static void Export(BinaryWriter w)
        {
            w.Write((byte)0);
            w.Write(ProtoRegistry.Loaded);

            if (!ProtoRegistry.Loaded) return;
            
            w.Write((byte)modMachines.Count);
            foreach (var machines in modMachines)
            {
                w.Write(machines.Key);
                w.Write((short)machines.Value.Count);

                foreach (int itemID in machines.Value)
                {
                    w.Write(itemID);
                }
            }

        }

        internal static void Import(BinaryReader r)
        {
            removedMachines.Clear();
            
            int ver = r.ReadByte();
            bool wasLoaded = r.ReadBoolean();

            if (!wasLoaded) return;
            
            int mods = r.ReadByte();
            for (int i = 0; i < mods; i++)
            {
                string modGUID = r.ReadString();
                int itemCount = r.ReadInt16();

                if (modGUID.Equals(ProtoRegistry.UNKNOWN_MOD) || modGUID.IsModInstalled())
                {
                    for (int j = 0; j < itemCount; j++)
                    {
                        int itemID = r.ReadInt32();
                        if (LDB.items.Select(itemID) == null)
                        {
                            AddMissingMachine(modGUID, itemID);
                        }
                    }
                }
                else
                {
                    for (int j = 0; j < itemCount; j++)
                    {
                        int itemID = r.ReadInt32();
                        AddMissingMachine(modGUID, itemID);
                    }
                }
            }

            CheckMissingMachines();
        }

        internal static void InitOnLoad()
        {
            removedMachines.Clear();
        }
    }
}