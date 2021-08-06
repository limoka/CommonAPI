using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using BepInEx;
using BepInEx.Logging;
using crecheng.DSPModSave;
using HarmonyLib;

[module: UnverifiableCode]
#pragma warning disable 618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore 618

namespace CommonAPI
{
    
    [BepInPlugin(GUID, NAME, VERSION)]
    public class CommonAPIPlugin : BaseUnityPlugin, IModCanSave
    {
        public const string ID = "CommonAPI";
        public const string GUID = "org.kremnev8.api." + ID;
        public const string NAME = "DSP Common API";
        
        public const string VERSION = "1.0.0";
        
        public static ManualLogSource logger;

        public static Dictionary<string, ISerializeState> registries = new Dictionary<string, ISerializeState>();
        

        void Awake()
        {
            logger = Logger;

            UnityThread.initUnityThread();
            ProtoRegistry.Init();
            
            Harmony harmony = new Harmony(GUID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            
            registries.Add($"{ID}:SystemsRegistry", CustomFactory.systemRegistry);
            registries.Add($"{ID}:ComponentRegistry", ComponentSystem.componentRegistry);
            registries.Add($"{ID}:RecipeTypeRegistry", RecipeTypes.typesRegistry);

            CustomFactory.systemRegistry.Register(ComponentSystem.systemID, typeof(ComponentSystem));
            
            NetworksRegistry.AddHandler(new PowerNetworkHandler());
            
            logger.LogInfo("Common API is initialized!");
        }

        public void Import(BinaryReader r)
        {
            int ver = r.ReadInt32();
            
            while (true)
            {
                if (r.ReadByte() == 0) break;

                string key = r.ReadString();

                if (registries.ContainsKey(key))
                {
                    r.ReadInt64();
                    registries[key].Import(r);
                }
                else
                {
                    long len = r.ReadInt64();
                    r.ReadBytes((int)len);
                }
            }

            CustomFactory.Init();
            CustomFactory.Import(r);
        }

        public void Export(BinaryWriter w)
        {
            w.Write(0);

            foreach (var kv in registries)
            {
                w.Write((byte)1);
                w.Write(kv.Key);
                MemoryStream stream = new MemoryStream();
                BinaryWriter tw = new BinaryWriter(stream);
                kv.Value.Export(tw);
                w.Write(stream.Length);
                w.Write(stream.ToArray());
            }

            w.Write((byte)0);

            CustomFactory.Export(w);
        }

        public void IntoOtherSave()
        {
            CustomFactory.Init();
        }
    }
}