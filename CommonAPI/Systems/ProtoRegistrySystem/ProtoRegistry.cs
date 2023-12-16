using System;
using System.Collections.Generic;
using System.Linq;
using CommonAPI.Patches;
using CommonAPI.Systems.ModLocalization;
using HarmonyLib;
using UnityEngine;
using xiaoye97;

// ReSharper disable InconsistentNaming

namespace CommonAPI.Systems
{
    /// <summary>
    /// Indicates that loading something has failed
    /// </summary>
    public class LoadException : Exception
    {
        public LoadException(string message) : base(message) { }
    }
    
    public class ProtoRegistry : BaseSubmodule
    {
        public const string UNKNOWN_MOD = "Unknown";
        internal static string currentMod = "";
        
        //Local proto dictionaries
        internal static Dictionary<int, ItemProto> items = new Dictionary<int, ItemProto>();
        internal static Dictionary<int, IconToolNew.IconDesc> itemIconDescs = new Dictionary<int, IconToolNew.IconDesc>();
        internal static Dictionary<int, int> itemUpgradeList = new Dictionary<int, int>();

        internal static Dictionary<int, RecipeProto> recipes = new Dictionary<int, RecipeProto>();
        internal static Dictionary<int, RecipeProto> recipeReplace = new Dictionary<int, RecipeProto>();

        internal static Dictionary<int, TechProto> techs = new Dictionary<int, TechProto>();
        internal static Dictionary<int, List<TechProto>> techUpdateList = new Dictionary<int, List<TechProto>>();

        internal static Dictionary<int, ModelProto> models = new Dictionary<int, ModelProto>();
        internal static Dictionary<string, LodMaterials> modelMats = new Dictionary<string, LodMaterials>();

        internal static Dictionary<int, AudioProto> audios = new Dictionary<int, AudioProto>();
        internal static Dictionary<int, MIDIProto> midiProtos = new Dictionary<int, MIDIProto>();
        
        internal static Dictionary<int, SignalProto> signals = new Dictionary<int, SignalProto>();

        internal static List<ResourceData> modResources = new List<ResourceData>();

        public static event Action onLoadingFinished;


        internal static int[] textureNames;
        internal static string[] spriteFileExtensions;
        internal static string[] audioClipFileExtensions;
        
        internal static ProtoRegistry Instance => CommonAPIPlugin.GetModuleInstance<ProtoRegistry>();
        internal override Type[] Dependencies => new[] { typeof(PickerExtensionsSystem), typeof(LocalizationModule) };

        internal override void SetHooks()
        {
            CommonAPIPlugin.harmony.PatchAll(typeof(ResourcesPatch));
            CommonAPIPlugin.harmony.PatchAll(typeof(StorageComponentPatch));
            CommonAPIPlugin.harmony.PatchAll(typeof(UIBuildMenuPatch));
            CommonAPIPlugin.harmony.PatchAll(typeof(VertaBufferPatch));

            CommonAPIPlugin.harmony.PatchAll(typeof(IconSetPatch));
            CommonAPIPlugin.harmony.PatchAll(typeof(GameMain_Patch));
            
            CommonAPIPlugin.harmony.PatchAll(typeof(ProtoSet_Patch));
        }


        internal override void Load()
        {

            int mainTex = Shader.PropertyToID("_MainTex");
            int normalTex = Shader.PropertyToID("_NormalTex");
            int msTex = Shader.PropertyToID("_MS_Tex");
            int emissionTex = Shader.PropertyToID("_EmissionTex");
            int emissionJitterTex = Shader.PropertyToID("_EmissionJitterTex");

            textureNames = new[] {mainTex, normalTex, msTex, emissionTex, emissionJitterTex};
            spriteFileExtensions = new[] {".jpg", ".png", ".tif"};
            audioClipFileExtensions = new[] {".mp3", ".ogg", ".waw", ".aif"};

            LDBTool.PostAddDataAction += OnPostAdd;
            LDBTool.EditDataAction += EditProto;
        }
        
        internal override void PostLoad()
        {
            LocalizationModule.RegisterTranslation("ModItemMissingWarnTitle", "Missing mod machines");
            LocalizationModule.RegisterTranslation("ModItemMissingWarnDesc", "Following mods had missing machines that were removed from your save:");
        }

        /// <summary>
        /// Registers mod resources for loading
        /// </summary>
        /// <param name="resource"></param>
        public static void AddResource(ResourceData resource)
        {
            Instance.ThrowIfNotLoaded();
            modResources.Add(resource);
        }

        /// <summary>
        /// Inform CommonAPI that your mod is starting to add it's items.
        /// </summary>
        /// <param name="modGUID">Your mod GUID</param>
        /// <exception cref="ArgumentException">If a mod is trying to interrupt other mods loading phase</exception>
        public static IDisposable StartModLoad(string modGUID)
        {
            Instance.ThrowIfNotLoaded();
            if (currentMod.Equals(""))
            {
                currentMod = modGUID;
                return new ModLoadState(modGUID);
            }
            
            throw new ArgumentException(
                $"Invalid request! Mod {modGUID} is trying to start it's loading phase, while {currentMod} is still loading. Please report this to {modGUID} author!");
        }

        //Post register fixups
        private static void OnPostAdd()
        {
            try
            {
                foreach (var kv in models)
                {
                    kv.Value.Preload();
                    PrefabDesc pdesc = kv.Value.prefabDesc;
                    
                    if (kv.Value.ID > LDB.models.modelArray.Length)
                    {
                        Array.Resize(ref LDB.models.modelArray, kv.Value.ID + 64);
                    }

                    if (!modelMats.ContainsKey(kv.Value.PrefabPath))
                    {
                        LDB.models.modelArray[kv.Value.ID] = kv.Value;
                        continue;
                    }

                    LodMaterials mats = modelMats[kv.Value.PrefabPath];

                    for (int i = 0; i < pdesc.lodCount; i++)
                    {
                        for (int j = 0; j < pdesc.lodMaterials[i].Length; j++)
                        {
                            if (mats.HasLod(i))
                            {
                                pdesc.lodMaterials[i][j] = mats[i][j];
                            }
                        }
                    }

                    LDB.models.modelArray[kv.Value.ID] = kv.Value;
                }

                foreach (var kv in items)
                {
                    kv.Value.Preload(kv.Value.index);
                }

                foreach (var kv in recipes)
                {
                    kv.Value.Preload(kv.Value.index);
                }

                foreach (var kv in techs)
                {
                    kv.Value.Preload();
                    kv.Value.Preload2();
                }

                foreach (var kv in techUpdateList)
                {
                    TechProto oldTech = LDB.techs.Select(kv.Key);
                    oldTech.postTechArray = oldTech.postTechArray.AddRangeToArray(kv.Value.ToArray());
                }

                foreach (var kv in audios)
                {
                    kv.Value.Preload();
                    int index = LDB.audios.dataIndices[kv.Value.ID];
                    LDB.audios.nameIndices.Add(kv.Value.Name, index);
                }

                foreach (var midiProto in midiProtos)
                {
                    midiProto.Value.Preload();
                }

                foreach (var kv in signals)
                {
                    kv.Value.Preload();
                    kv.Value.description = kv.Value.description.Translate();
                }
                ItemProto.InitFuelNeeds();
                ItemProto.InitFluids();
                ItemProto.InitItemIds();
                ItemProto.InitItemIndices();

                onLoadingFinished?.Invoke();

                CommonAPIPlugin.logger.LogInfo("Post loading is complete!");
            }
            catch (Exception e)
            {
                CommonAPIPlugin.logger.LogError($"Error initializing proto data!\n{e.Message}, stacktrace:\n{e.StackTrace}");
            }
        }

        private static void EditProto(Proto proto)
        {
            if (proto is ItemProto itemProto)
            {
                if (itemUpgradeList.ContainsKey(itemProto.ID))
                {
                    itemProto.Grade = itemUpgradeList[itemProto.ID];
                    CommonAPIPlugin.logger.LogDebug($"Changing grade of {itemProto.name} to {itemProto.Grade}");
                }

                if (itemProto.Grade == 0 || items.ContainsKey(itemProto.ID)) return;

                foreach (var kv in items)
                {
                    if (kv.Value.Grade == 0 || kv.Value.Upgrades == null) continue;
                    if (itemProto.Grade > kv.Value.Upgrades.Length) continue;

                    if (kv.Value.Upgrades[itemProto.Grade - 1] == itemProto.ID)
                    {
                        itemProto.Upgrades = kv.Value.Upgrades;
                        CommonAPIPlugin.logger.LogDebug($"Updating upgrade list of {itemProto.name} to {itemProto.Upgrades.Join()}");
                    }
                }
            }
            else if (proto is RecipeProto recipeProto)
            {
                if (recipeReplace.ContainsKey(recipeProto.ID))
                {
                    RecipeProto newProto = recipeReplace[recipeProto.ID];
                    newProto.CopyPropsTo(ref recipeProto);
                    recipeProto.Preload(recipeProto.index);
                }
            }
        }

        /// <summary>
        /// Creates custom material with given shader name.
        /// _MainTex ("Albedo (RGB) diffuse reflection (A) color mask", 2D)
        /// _NormalTex ("Normal map", 2D)
        /// _MS_Tex ("Metallic (R) transparent paste (G) metal (a) highlight", 2D)
        /// _EmissionTex ("Emission (RGB) self-luminous (A) jitter mask", 2D)
        /// </summary>
        /// <param name="shaderName">Name of shader to use</param>
        /// <param name="materialName">Name of finished material, can be anything</param>
        /// <param name="color">Tint color (In html format, #RRGGBBAA)</param>
        /// <param name="textures">Array of texture names in this order: albedo, normal, metallic, emission</param>
        /// <param name="keywords">Array of keywords to use</param>
        /// <param name="textureIDs">Array of texture property ids (Use Shader.PropertyToID)</param>
        public static Material CreateMaterial(string shaderName, string materialName, string color,
            string[] textures, string[] keywords)
        {
            return CreateMaterial(shaderName, materialName, color, textures, keywords, null);
        }

        /// <summary>
        /// Creates custom material with given shader name.
        /// _MainTex ("Albedo (RGB) diffuse reflection (A) color mask", 2D)
        /// _NormalTex ("Normal map", 2D)
        /// _MS_Tex ("Metallic (R) transparent paste (G) metal (a) highlight", 2D)
        /// _EmissionTex ("Emission (RGB) self-luminous (A) jitter mask", 2D)
        /// </summary>
        /// <param name="shaderName">Name of shader to use</param>
        /// <param name="materialName">Name of finished material, can be anything</param>
        /// <param name="color">Tint color (In html format, #RRGGBBAA)</param>
        /// <param name="textures">Array of texture names in this order: albedo, normal, metallic, emission</param>
        /// <param name="keywords">Array of keywords to use</param>
        /// <param name="textureIDs">Array of texture property ids (Use Shader.PropertyToID)</param>
        public static Material CreateMaterial(string shaderName, string materialName, string color,
            string[] textures)
        {
            return CreateMaterial(shaderName, materialName, color, textures, null, null);
        }

        /// <summary>
        /// Creates custom material with given shader name.
        /// _MainTex ("Albedo (RGB) diffuse reflection (A) color mask", 2D)
        /// _NormalTex ("Normal map", 2D)
        /// _MS_Tex ("Metallic (R) transparent paste (G) metal (a) highlight", 2D)
        /// _EmissionTex ("Emission (RGB) self-luminous (A) jitter mask", 2D)
        /// </summary>
        /// <param name="shaderName">Name of shader to use</param>
        /// <param name="materialName">Name of finished material, can be anything</param>
        /// <param name="color">Tint color (In html format, #RRGGBBAA)</param>
        /// <param name="textures">Array of texture names in this order: albedo, normal, metallic, emission</param>
        /// <param name="keywords">Array of keywords to use</param>
        /// <param name="textureIDs">Array of texture property ids (Use Shader.PropertyToID)</param>
        public static Material CreateMaterial(string shaderName, string materialName, string color)
        {
            return CreateMaterial(shaderName, materialName, color, null, null, null);
        }

        /// <summary>
        /// Creates custom material with given shader name.
        /// _MainTex ("Albedo (RGB) diffuse reflection (A) color mask", 2D)
        /// _NormalTex ("Normal map", 2D)
        /// _MS_Tex ("Metallic (R) transparent paste (G) metal (a) highlight", 2D)
        /// _EmissionTex ("Emission (RGB) self-luminous (A) jitter mask", 2D)
        /// </summary>
        /// <param name="shaderName">Name of shader to use</param>
        /// <param name="materialName">Name of finished material, can be anything</param>
        /// <param name="color">Tint color (In html format, #RRGGBBAA)</param>
        /// <param name="textures">Array of texture names in this order: albedo, normal, metallic, emission</param>
        /// <param name="keywords">Array of keywords to use</param>
        /// <param name="textureIDs">Array of texture property ids (Use Shader.PropertyToID)</param>
        public static Material CreateMaterial(string shaderName, string materialName, string color,
            string[] textures, string[] keywords, int[] textureIDs)
        {
            ColorUtility.TryParseHtmlString(color, out Color newCol);
            return CreateMaterial(shaderName, materialName, newCol, textures, keywords, textureIDs);
        }

        /// <summary>
        /// Creates custom material with given shader name.
        /// _MainTex ("Albedo (RGB) diffuse reflection (A) color mask", 2D)
        /// _NormalTex ("Normal map", 2D)
        /// _MS_Tex ("Metallic (R) transparent paste (G) metal (a) highlight", 2D)
        /// _EmissionTex ("Emission (RGB) self-luminous (A) jitter mask", 2D)
        /// </summary>
        /// <param name="shaderName">Name of shader to use</param>
        /// <param name="materialName">Name of finished material, can be anything</param>
        /// <param name="color">Tint color</param>
        /// <param name="textures">Array of texture names in this order: albedo, normal, metallic, emission</param>
        /// <param name="keywords">Array of keywords to use</param>
        /// <param name="textureIDs">Array of texture property ids (Use Shader.PropertyToID)</param>
        public static Material CreateMaterial(string shaderName, string materialName, Color color,
            string[] textures, string[] keywords)
        {
            return CreateMaterial(shaderName, materialName, color, textures, keywords, null);
        }

        /// <summary>
        /// Creates custom material with given shader name.
        /// _MainTex ("Albedo (RGB) diffuse reflection (A) color mask", 2D)
        /// _NormalTex ("Normal map", 2D)
        /// _MS_Tex ("Metallic (R) transparent paste (G) metal (a) highlight", 2D)
        /// _EmissionTex ("Emission (RGB) self-luminous (A) jitter mask", 2D)
        /// </summary>
        /// <param name="shaderName">Name of shader to use</param>
        /// <param name="materialName">Name of finished material, can be anything</param>
        /// <param name="color">Tint color</param>
        /// <param name="textures">Array of texture names in this order: albedo, normal, metallic, emission</param>
        /// <param name="keywords">Array of keywords to use</param>
        /// <param name="textureIDs">Array of texture property ids (Use Shader.PropertyToID)</param>
        public static Material CreateMaterial(string shaderName, string materialName, Color color,
            string[] textures)
        {
            return CreateMaterial(shaderName, materialName, color, textures, null, null);
        }

        /// <summary>
        /// Creates custom material with given shader name.
        /// _MainTex ("Albedo (RGB) diffuse reflection (A) color mask", 2D)
        /// _NormalTex ("Normal map", 2D)
        /// _MS_Tex ("Metallic (R) transparent paste (G) metal (a) highlight", 2D)
        /// _EmissionTex ("Emission (RGB) self-luminous (A) jitter mask", 2D)
        /// </summary>
        /// <param name="shaderName">Name of shader to use</param>
        /// <param name="materialName">Name of finished material, can be anything</param>
        /// <param name="color">Tint color</param>
        /// <param name="textures">Array of texture names in this order: albedo, normal, metallic, emission</param>
        /// <param name="keywords">Array of keywords to use</param>
        /// <param name="textureIDs">Array of texture property ids (Use Shader.PropertyToID)</param>
        public static Material CreateMaterial(string shaderName, string materialName, Color color)
        {
            return CreateMaterial(shaderName, materialName, color, null, null, null);
        }

        /// <summary>
        /// Creates custom material with given shader name.
        /// _MainTex ("Albedo (RGB) diffuse reflection (A) color mask", 2D)
        /// _NormalTex ("Normal map", 2D)
        /// _MS_Tex ("Metallic (R) transparent paste (G) metal (a) highlight", 2D)
        /// _EmissionTex ("Emission (RGB) self-luminous (A) jitter mask", 2D)
        /// </summary>
        /// <param name="shaderName">Name of shader to use</param>
        /// <param name="materialName">Name of finished material, can be anything</param>
        /// <param name="color">Tint color</param>
        /// <param name="textures">Array of texture names in this order: albedo, normal, metallic, emission</param>
        /// <param name="keywords">Array of keywords to use</param>
        /// <param name="textureIDs">Array of texture property ids (Use Shader.PropertyToID)</param>
        public static Material CreateMaterial(string shaderName, string materialName, Color color,
            string[] textures, string[] keywords, int[] textureIDs)
        {
            Instance.ThrowIfNotLoaded();

            Material mainMat = new Material(Shader.Find(shaderName))
            {
                shaderKeywords = keywords ?? new[] {"_ENABLE_VFINST"},
                color = color,
                name = materialName
            };

            if (textures == null) return mainMat;
            int[] texIds = textureIDs ?? textureNames;

            for (int i = 0; i < textures.Length; i++)
            {
                if (i >= texIds.Length || textures[i].Equals("")) continue;

                Texture2D texture = Resources.Load<Texture2D>(textures[i]);
                mainMat.SetTexture(texIds[i], texture);
            }

            return mainMat;
        }

        /// <summary>
        /// Helper method to define Grid Index
        /// </summary>
        /// <param name="tab">Number of tab</param>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <returns>Grid Index</returns>
        public static int GetGridIndex(int tab, int x, int y)
        {
            return tab * 1000 + y * 100 + x;
        }
        

        /// <summary>
        /// Registers a ModelProto
        /// </summary>
        /// <param name="id">UNIQUE id of your model</param>
        /// <param name="prefabPath">Path to the prefab, starting from asset folder in your unity project</param>
        /// <param name="mats">List of materials to use</param>
        public static ModelProto RegisterModel(int id, string prefabPath, Material[] mats = null, int rendererType = 0)
        {
            Instance.ThrowIfNotLoaded();
            ModelProto model = new ModelProto
            {
                Name = id.ToString(),
                PrefabPath = prefabPath,
                ID = id,
                RendererType = rendererType
            };

            LDBTool.PreAddProto(model);
            models.Add(model.ID, model);

            if (mats != null)
                modelMats.Add(prefabPath, new LodMaterials(mats));

            return model;
        }

        /// <summary>
        /// Registers a ModelProto and links an proto item to it
        /// </summary>
        /// <param name="id">UNIQUE id of your model</param>
        /// <param name="proto">ItemProto which will be turned into building</param>
        /// <param name="prefabPath">Path to the prefab, starting from asset folder in your unity project</param>
        /// <param name="mats">List of materials to use</param>
        /// <param name="descFields">int Array of used description fields</param>
        /// <param name="buildIndex">Index in build Toolbar, FSS, F - first submenu, S - second submenu</param>
        /// <param name="grade">Grade of the building, used to add upgrading</param>
        /// <param name="upgradesIDs">List of buildings ids, that are upgradable to this one. You need to include all of them here in order. ID of this building should be zero</param>
        public static ModelProto RegisterModel(int id, ItemProto proto, string prefabPath, Material[] mats,
            int[] descFields, int buildIndex)
        {
            return RegisterModel(id, proto, prefabPath, mats, descFields, buildIndex, 0, new int[] { }, 0);
        }

        /// <summary>
        /// Registers a ModelProto and links an proto item to it
        /// </summary>
        /// <param name="id">UNIQUE id of your model</param>
        /// <param name="proto">ItemProto which will be turned into building</param>
        /// <param name="prefabPath">Path to the prefab, starting from asset folder in your unity project</param>
        /// <param name="mats">List of materials to use</param>
        /// <param name="descFields">int Array of used description fields</param>
        /// <param name="buildIndex">Index in build Toolbar, FSS, F - first submenu, S - second submenu</param>
        /// <param name="grade">Grade of the building, used to add upgrading</param>
        /// <param name="upgradesIDs">List of buildings ids, that are upgradable to this one. You need to include all of them here in order. ID of this building should be zero</param>
        public static ModelProto RegisterModel(int id, ItemProto proto, string prefabPath, Material[] mats,
            int[] descFields, int buildIndex, int grade, int[] upgradesIDs)
        {
            return RegisterModel(id, proto, prefabPath, mats, descFields, buildIndex, grade, upgradesIDs, 0);
        }

        /// <summary>
        /// Registers a ModelProto and links an proto item to it
        /// </summary>
        /// <param name="id">UNIQUE id of your model</param>
        /// <param name="proto">ItemProto which will be turned into building</param>
        /// <param name="prefabPath">Path to the prefab, starting from asset folder in your unity project</param>
        /// <param name="mats">List of materials to use</param>
        /// <param name="descFields">int Array of used description fields</param>
        /// <param name="buildIndex">Index in build Toolbar, FSS, F - first submenu, S - second submenu</param>
        /// <param name="grade">Grade of the building, used to add upgrading</param>
        /// <param name="upgradesIDs">List of buildings ids, that are upgradable to this one. You need to include all of them here in order. ID of this building should be zero</param>
        public static ModelProto RegisterModel(int id, ItemProto proto, string prefabPath, Material[] mats,
            int[] descFields, int buildIndex, int grade, int[] upgradesIDs, int rendererType)
        {
            Instance.ThrowIfNotLoaded();
            ModelProto model = new ModelProto
            {
                Name = id.ToString(),
                PrefabPath = prefabPath,
                ID = id,
                RendererType = rendererType
            };

            AddModelToItemProto(model, proto, descFields, buildIndex, grade, upgradesIDs);

            LDBTool.PreAddProto(model);
            models.Add(model.ID, model);

            if (mats != null)
                modelMats.Add(prefabPath, new LodMaterials(mats));

            return model;
        }

        /// <summary>
        /// Register Lod Materials for prefab
        /// </summary>
        /// <param name="prefabPath">Path to the prefab</param>
        /// <param name="lod">Level of LOD (from 0 to 3 inclusive)</param>
        /// <param name="mats">Array of materials</param>
        public static void AddLodMaterials(string prefabPath, int lod, Material[] mats)
        {
            Instance.ThrowIfNotLoaded();
            if (modelMats.ContainsKey(prefabPath))
            {
                LodMaterials lodMats = modelMats[prefabPath];
                lodMats.AddLod(lod, mats);
            }
            else
            {
                modelMats.Add(prefabPath, new LodMaterials(lod, mats));
            }
        }

        /// <summary>
        /// Link ModelProto to an ItemProto
        /// </summary>
        /// <param name="model">ModelProto which will contain building model</param>
        /// <param name="item">ItemProto which will be turned into building</param>
        /// <param name="descFields">int Array of used description fields</param>
        /// <param name="buildIndex">Index in build Toolbar, FSS, F - first submenu, S - second submenu</param>
        /// <param name="grade">Grade of the building, used to add upgrading</param>
        /// <param name="upgradesIDs">List of buildings ids, that are upgradable to this one. You need to include all of them here in order. ID of this building should be zero</param>
        public static void AddModelToItemProto(ModelProto model, ItemProto item, int[] descFields, int buildIndex)
        {
            AddModelToItemProto(model, item, descFields, buildIndex, 0, new int[] { });
        }

        /// <summary>
        /// Link ModelProto to an ItemProto
        /// </summary>
        /// <param name="model">ModelProto which will contain building model</param>
        /// <param name="item">ItemProto which will be turned into building</param>
        /// <param name="descFields">int Array of used description fields</param>
        /// <param name="buildIndex">Index in build Toolbar, FSS, F - first submenu, S - second submenu</param>
        /// <param name="grade">Grade of the building, used to add upgrading</param>
        /// <param name="upgradesIDs">List of buildings ids, that are upgradable to this one. You need to include all of them here in order. ID of this building should be zero</param>
        public static void AddModelToItemProto(ModelProto model, ItemProto item, int[] descFields, int buildIndex, int grade, int[] upgradesIDs)
        {
            Instance.ThrowIfNotLoaded();
            item.Type = EItemType.Production;
            item.ModelIndex = model.ID;
            item.ModelCount = 1;
            item.BuildIndex = buildIndex;
            item.BuildMode = 1;
            item.IsEntity = true;
            item.CanBuild = true;
            item.DescFields = descFields;
            if (grade != 0 && upgradesIDs != null)
            {
                item.Grade = grade;
                for (int i = 0; i < upgradesIDs.Length; i++)
                {
                    int itemID = upgradesIDs[i];
                    if (itemID == 0) continue;

                    if (!itemUpgradeList.ContainsKey(itemID))
                        itemUpgradeList.Add(itemID, i + 1);
                }

                upgradesIDs[grade - 1] = item.ID;
                item.Upgrades = upgradesIDs;
            }
            else
            {
                item.Upgrades = new int[0];
            }

            if (currentMod.Equals(""))
            {
                ModProtoHistory.AddModMachine(UNKNOWN_MOD, item.ID); 
            }
            else
            {
                ModProtoHistory.AddModMachine(currentMod, item.ID);
            }
        }

        /// <summary>
        /// Registers a ItemProto
        /// </summary>
        /// <param name="id">UNIQUE id of your item</param>
        /// <param name="name">LocalizedKey of name of the item</param>
        /// <param name="description">LocalizedKey of description of the item</param>
        /// <param name="iconPath">Path to icon, starting from assets folder of your unity project</param>
        /// <param name="gridIndex">Index in craft menu, format : PYXX, P - page</param>
        /// <param name="stackSize">Stack size of the item</param>
        /// <param name="type">What type this is item is</param>
        public static ItemProto RegisterItem(int id, string name, string description, string iconPath,
            int gridIndex, int stackSize, EItemType type)
        {
            return RegisterItem(id, name, description, iconPath, gridIndex, stackSize, type, null);
        }

        /// <summary>
        /// Registers a ItemProto
        /// </summary>
        /// <param name="id">UNIQUE id of your item</param>
        /// <param name="name">LocalizedKey of name of the item</param>
        /// <param name="description">LocalizedKey of description of the item</param>
        /// <param name="iconPath">Path to icon, starting from assets folder of your unity project</param>
        /// <param name="gridIndex">Index in craft menu, format : PYXX, P - page</param>
        /// <param name="stackSize">Stack size of the item</param>
        public static ItemProto RegisterItem(int id, string name, string description, string iconPath,
            int gridIndex, int stackSize)
        {
            return RegisterItem(id, name, description, iconPath, gridIndex, stackSize, EItemType.Material, null);
        }

        /// <summary>
        /// Registers a ItemProto
        /// </summary>
        /// <param name="id">UNIQUE id of your item</param>
        /// <param name="name">LocalizedKey of name of the item</param>
        /// <param name="description">LocalizedKey of description of the item</param>
        /// <param name="iconPath">Path to icon, starting from assets folder of your unity project</param>
        /// <param name="gridIndex">Index in craft menu, format : PYXX, P - page</param>
        /// <param name="stackSize">Stack size of the item</param>
        public static ItemProto RegisterItem(int id, string name, string description, string iconPath,
            int gridIndex)
        {
            return RegisterItem(id, name, description, iconPath, gridIndex, 50, EItemType.Material, null);
        }

        /// <summary>
        /// Registers a ItemProto
        /// </summary>
        /// <param name="id">UNIQUE id of your item</param>
        /// <param name="name">LocalizedKey of name of the item</param>
        /// <param name="description">LocalizedKey of description of the item</param>
        /// <param name="iconPath">Path to icon, starting from assets folder of your unity project</param>
        /// <param name="gridIndex">Index in craft menu, format : PYXX, P - page</param>
        /// <param name="stackSize">Stack size of the item</param>
        /// <param name="type">What type this is item is</param>
        /// <param name="beltItemDesc">Item appearance on belts description</param>
        public static ItemProto RegisterItem(int id, string name, string description, string iconPath,
            int gridIndex, int stackSize, EItemType type, IconToolNew.IconDesc beltItemDesc)
        {
            Instance.ThrowIfNotLoaded();

            beltItemDesc ??= GetDefaultIconDesc(Color.gray, Color.gray);

            ItemProto proto = new ItemProto
            {
                Type = type,
                StackSize = stackSize,
                FuelType = 0,
                IconPath = iconPath,
                Name = name,
                Description = description,
                GridIndex = gridIndex,
                DescFields = new[] {1},
                ID = id
            };

            LDBTool.PreAddProto(proto);

            items.Add(proto.ID, proto);
            itemIconDescs.Add(proto.ID, beltItemDesc);
            return proto;
        }

        public static IconToolNew.IconDesc GetDefaultIconDesc(Color faceColor, Color sideColor)
        {
            return new IconToolNew.IconDesc
            {
                faceColor = faceColor,
                sideColor = sideColor,
                faceEmission = Color.black,
                sideEmission = Color.black,
                iconEmission = new Color(0.2f, 0.2f, 0.2f, 1f),
                metallic = 0.8f,
                smoothness = 0.5f,
                solidAlpha = 1f,
                iconAlpha = 1f,
                
            };
        }
        
        public static IconToolNew.IconDesc GetDefaultIconDesc(Color faceColor, Color sideColor, Color faceEmission, Color sideEmission)
        {
            IconToolNew.IconDesc desc = GetDefaultIconDesc(faceColor, sideColor);
            desc.faceEmission = faceEmission;
            desc.sideEmission = sideEmission;
            return desc;
        }


        /// <summary>
        /// Registers a RecipeProto with a custom type
        /// </summary>
        /// <param name="id">UNIQUE id of your recipe</param>
        /// <param name="type">Recipe type in string form</param>
        /// <param name="time">Time in ingame ticks. How long item is being made</param>
        /// <param name="input">Array of input IDs</param>
        /// <param name="inCounts">Array of input COUNTS</param>
        /// <param name="output">Array of output IDs</param>
        /// <param name="outCounts">Array of output COUNTS</param>
        /// <param name="description">LocalizedKey of description of this item</param>
        /// <param name="techID">Tech id, which unlock this recipe</param>
        public static RecipeProto RegisterRecipe(int id, int type, int time, int[] input, int[] inCounts,
            int[] output,
            int[] outCounts, string description, int techID)
        {
            return RegisterRecipe(id, type, time, input, inCounts, output, outCounts, description, techID, 0);
        }

        /// <summary>
        /// Registers a RecipeProto with a custom type
        /// </summary>
        /// <param name="id">UNIQUE id of your recipe</param>
        /// <param name="type">Recipe type in string form</param>
        /// <param name="time">Time in ingame ticks. How long item is being made</param>
        /// <param name="input">Array of input IDs</param>
        /// <param name="inCounts">Array of input COUNTS</param>
        /// <param name="output">Array of output IDs</param>
        /// <param name="outCounts">Array of output COUNTS</param>
        /// <param name="description">LocalizedKey of description of this item</param>
        /// <param name="techID">Tech id, which unlock this recipe</param>
        public static RecipeProto RegisterRecipe(int id, int type, int time, int[] input, int[] inCounts,
            int[] output,
            int[] outCounts, string description)
        {
            return RegisterRecipe(id, type, time, input, inCounts, output, outCounts, description, 0, 0);
        }
        
        /// <summary>
        /// Registers a RecipeProto with a custom type. <see cref="AssemblerRecipeSystem"/> must be loaded to use this method!
        /// </summary>
        /// <param name="id">UNIQUE id of your recipe</param>
        /// <param name="type">Recipe type in string form</param>
        /// <param name="time">Time in ingame ticks. How long item is being made</param>
        /// <param name="input">Array of input IDs</param>
        /// <param name="inCounts">Array of input COUNTS</param>
        /// <param name="output">Array of output IDs</param>
        /// <param name="outCounts">Array of output COUNTS</param>
        /// <param name="description">LocalizedKey of description of this item</param>
        /// <param name="techID">Tech id, which unlock this recipe</param>
        public static RecipeProto RegisterRecipe(int id, int type, int time, int[] input, int[] inCounts,
            int[] output,
            int[] outCounts, string description, int techID, int gridIndex)
        {
            Instance.ThrowIfNotLoaded();

            return RegisterRecipe(id, type, time, input, inCounts, output, outCounts, description, techID, gridIndex, "", "");
        }

        /// <summary>
        /// Registers a RecipeProto with a custom type. <see cref="AssemblerRecipeSystem"/> must be loaded to use this method!
        /// </summary>
        /// <param name="id">UNIQUE id of your recipe</param>
        /// <param name="type">Recipe type in string form</param>
        /// <param name="time">Time in ingame ticks. How long item is being made</param>
        /// <param name="input">Array of input IDs</param>
        /// <param name="inCounts">Array of input COUNTS</param>
        /// <param name="output">Array of output IDs</param>
        /// <param name="outCounts">Array of output COUNTS</param>
        /// <param name="description">LocalizedKey of description of this item</param>
        /// <param name="techID">Tech id, which unlock this recipe</param>
        public static RecipeProto RegisterRecipe(int id, int type, int time, int[] input, int[] inCounts,
            int[] output,
            int[] outCounts, string description, int techID, int gridIndex, string iconPath)
        {
            Instance.ThrowIfNotLoaded();

            return RegisterRecipe(id, type, time, input, inCounts, output, outCounts, description, techID, gridIndex, "", iconPath);
        }

        /// <summary>
        /// Registers a RecipeProto with a custom type. <see cref="AssemblerRecipeSystem"/> must be loaded to use this method!
        /// </summary>
        /// <param name="id">UNIQUE id of your recipe</param>
        /// <param name="type">Recipe type in string form</param>
        /// <param name="time">Time in ingame ticks. How long item is being made</param>
        /// <param name="input">Array of input IDs</param>
        /// <param name="inCounts">Array of input COUNTS</param>
        /// <param name="output">Array of output IDs</param>
        /// <param name="outCounts">Array of output COUNTS</param>
        /// <param name="description">LocalizedKey of description of this item</param>
        /// <param name="techID">Tech id, which unlock this recipe</param>
        public static RecipeProto RegisterRecipe(int id, int type, int time, int[] input, int[] inCounts,
            int[] output,
            int[] outCounts, string description, int techID, int gridIndex, string name, string iconPath)
        {
            Instance.ThrowIfNotLoaded();
            if (AssemblerRecipeSystem.IsRecipeTypeRegistered(type))
            {
                RecipeProto recipe = RegisterRecipe(id, ERecipeType.Custom, time, input, inCounts, output, outCounts, description, techID, gridIndex, name, iconPath);
                
                AssemblerRecipeSystem.BindRecipeToType(recipe, type);
                return recipe;
            }

            throw new ArgumentException($"Recipe Type: {type} is not registered!");
        }

        /// <summary>
        /// Registers a RecipeProto with vanilla types
        /// </summary>
        /// <param name="id">UNIQUE id of your recipe</param>
        /// <param name="type">Recipe type</param>
        /// <param name="time">Time in ingame ticks. How long item is being made</param>
        /// <param name="input">Array of input IDs</param>
        /// <param name="inCounts">Array of input COUNTS</param>
        /// <param name="output">Array of output IDs</param>
        /// <param name="outCounts">Array of output COUNTS</param>
        /// <param name="description">LocalizedKey of description of this item</param>
        /// <param name="techID">Tech id, which unlock this recipe</param>
        public static RecipeProto RegisterRecipe(int id, ERecipeType type, int time, int[] input, int[] inCounts,
            int[] output,
            int[] outCounts, string description, int techID)
        {
            return RegisterRecipe(id, type, time, input, inCounts, output, outCounts, description, techID, 0);
        }

        /// <summary>
        /// Registers a RecipeProto with vanilla types
        /// </summary>
        /// <param name="id">UNIQUE id of your recipe</param>
        /// <param name="type">Recipe type</param>
        /// <param name="time">Time in ingame ticks. How long item is being made</param>
        /// <param name="input">Array of input IDs</param>
        /// <param name="inCounts">Array of input COUNTS</param>
        /// <param name="output">Array of output IDs</param>
        /// <param name="outCounts">Array of output COUNTS</param>
        /// <param name="description">LocalizedKey of description of this item</param>
        /// <param name="techID">Tech id, which unlock this recipe</param>
        public static RecipeProto RegisterRecipe(int id, ERecipeType type, int time, int[] input, int[] inCounts,
            int[] output,
            int[] outCounts, string description)
        {
            return RegisterRecipe(id, type, time, input, inCounts, output, outCounts, description, 0, 0);
        }

        /// <summary>
        /// Registers a RecipeProto with vanilla types
        /// </summary>
        /// <param name="id">UNIQUE id of your recipe</param>
        /// <param name="type">Recipe type</param>
        /// <param name="time">Time in ingame ticks. How long item is being made</param>
        /// <param name="input">Array of input IDs</param>
        /// <param name="inCounts">Array of input COUNTS</param>
        /// <param name="output">Array of output IDs</param>
        /// <param name="outCounts">Array of output COUNTS</param>
        /// <param name="description">LocalizedKey of description of this item</param>
        /// <param name="techID">Tech id, which unlock this recipe</param>
        public static RecipeProto RegisterRecipe(int id, ERecipeType type, int time, int[] input, int[] inCounts,
            int[] output,
            int[] outCounts, string description, int techID, int gridIndex)
        {
            Instance.ThrowIfNotLoaded();
            RecipeProto proto = NewRecipeProto(id, type, time, input, inCounts, output, outCounts, description, techID, gridIndex, "", "");

            LDBTool.PreAddProto(proto);
            recipes.Add(id, proto);

            return proto;
        }
        
        /// <summary>
        /// Registers a RecipeProto with vanilla types
        /// </summary>
        /// <param name="id">UNIQUE id of your recipe</param>
        /// <param name="type">Recipe type</param>
        /// <param name="time">Time in ingame ticks. How long item is being made</param>
        /// <param name="input">Array of input IDs</param>
        /// <param name="inCounts">Array of input COUNTS</param>
        /// <param name="output">Array of output IDs</param>
        /// <param name="outCounts">Array of output COUNTS</param>
        /// <param name="description">LocalizedKey of description of this item</param>
        /// <param name="techID">Tech id, which unlock this recipe</param>
        public static RecipeProto RegisterRecipe(int id, ERecipeType type, int time, int[] input, int[] inCounts,
            int[] output,
            int[] outCounts, string description, int techID, int gridIndex, string iconPath)
        {
            Instance.ThrowIfNotLoaded();
            RecipeProto proto = NewRecipeProto(id, type, time, input, inCounts, output, outCounts, description, techID, gridIndex, "", iconPath);

            LDBTool.PreAddProto(proto);
            recipes.Add(id, proto);

            return proto;
        }
        
        /// <summary>
        /// Registers a RecipeProto with vanilla types
        /// </summary>
        /// <param name="id">UNIQUE id of your recipe</param>
        /// <param name="type">Recipe type</param>
        /// <param name="time">Time in ingame ticks. How long item is being made</param>
        /// <param name="input">Array of input IDs</param>
        /// <param name="inCounts">Array of input COUNTS</param>
        /// <param name="output">Array of output IDs</param>
        /// <param name="outCounts">Array of output COUNTS</param>
        /// <param name="description">LocalizedKey of description of this item</param>
        /// <param name="techID">Tech id, which unlock this recipe</param>
        public static RecipeProto RegisterRecipe(int id, ERecipeType type, int time, int[] input, int[] inCounts,
            int[] output,
            int[] outCounts, string description, int techID, int gridIndex, string name, string iconPath)
        {
            Instance.ThrowIfNotLoaded();
            RecipeProto proto = NewRecipeProto(id, type, time, input, inCounts, output, outCounts, description, techID, gridIndex, name, iconPath);

            LDBTool.PreAddProto(proto);
            recipes.Add(id, proto);

            return proto;
        }

        private static RecipeProto NewRecipeProto(int id, ERecipeType type, int time, int[] input, int[] inCounts, int[] output, int[] outCounts,
            string description,
            int techID, int gridIndex, string name, string iconPath)
        {
            Instance.ThrowIfNotLoaded();
            if (output.Length > 0)
            {
                ItemProto first = null;
                if (name.Equals("") || iconPath.Equals(""))
                {
                    first = items.ContainsKey(output[0]) ? items[output[0]] : LDB.items.Select(output[0]);
                }
                
                TechProto tech = null;
                if (techID != 0 && LDB.techs.Exist(techID))
                {
                    tech = LDB.techs.Select(techID);
                }

                RecipeProto proto = new RecipeProto
                {
                    Type = type,
                    Handcraft = true,
                    TimeSpend = time,
                    Items = input,
                    ItemCounts = inCounts,
                    Results = output,
                    ResultCounts = outCounts,
                    Description = description,
                    GridIndex = gridIndex == 0 ? first.GridIndex : gridIndex,
                    IconPath = iconPath.Equals("") ? first.IconPath : iconPath,
                    Name = name.Equals("") ? first.Name + "Recipe" : name,
                    preTech = tech,
                    ID = id
                };

                return proto;
            }

            throw new ArgumentException("Output array must not be empty");
        }

        /// <summary>
        /// Entirely replace recipe proto with specified ID
        /// </summary>
        /// <param name="id">ID of target recipe</param>
        /// <param name="type">Recipe type</param>
        /// <param name="time">Time in ingame ticks. How long item is being made</param>
        /// <param name="input">Array of input IDs</param>
        /// <param name="inCounts">Array of input COUNTS</param>
        /// <param name="output">Array of output IDs</param>
        /// <param name="outCounts">Array of output COUNTS</param>
        /// <param name="description">LocalizedKey of description of this item</param>
        /// <param name="techID">Tech id, which unlock this recipe</param>
        public static void EditRecipe(int id, ERecipeType type, int time, int[] input, int[] inCounts,
            int[] output,
            int[] outCounts, string description, int techID)
        {
            EditRecipe(id, type, time, input, inCounts, output, outCounts, description, techID, 0);
        }

        /// <summary>
        /// Entirely replace recipe proto with specified ID
        /// </summary>
        /// <param name="id">ID of target recipe</param>
        /// <param name="type">Recipe type</param>
        /// <param name="time">Time in ingame ticks. How long item is being made</param>
        /// <param name="input">Array of input IDs</param>
        /// <param name="inCounts">Array of input COUNTS</param>
        /// <param name="output">Array of output IDs</param>
        /// <param name="outCounts">Array of output COUNTS</param>
        /// <param name="description">LocalizedKey of description of this item</param>
        /// <param name="techID">Tech id, which unlock this recipe</param>
        public static void EditRecipe(int id, ERecipeType type, int time, int[] input, int[] inCounts,
            int[] output,
            int[] outCounts, string description)
        {
            EditRecipe(id, type, time, input, inCounts, output, outCounts, description, 0, 0);
        }
        
        /// <summary>
        /// Entirely replace recipe proto with specified ID
        /// </summary>
        /// <param name="id">ID of target recipe</param>
        /// <param name="type">Recipe type</param>
        /// <param name="time">Time in ingame ticks. How long item is being made</param>
        /// <param name="input">Array of input IDs</param>
        /// <param name="inCounts">Array of input COUNTS</param>
        /// <param name="output">Array of output IDs</param>
        /// <param name="outCounts">Array of output COUNTS</param>
        /// <param name="description">LocalizedKey of description of this item</param>
        /// <param name="techID">Tech id, which unlock this recipe</param>
        public static void EditRecipe(int id, ERecipeType type, int time, int[] input, int[] inCounts,
            int[] output,
            int[] outCounts, string description, int techID, int gridIndex)
        {
            EditRecipe(id, type, time, input, inCounts, output, outCounts, description, techID, gridIndex, "");
        }

        /// <summary>
        /// Entirely replace recipe proto with specified ID
        /// </summary>
        /// <param name="id">ID of target recipe</param>
        /// <param name="type">Recipe type</param>
        /// <param name="time">Time in ingame ticks. How long item is being made</param>
        /// <param name="input">Array of input IDs</param>
        /// <param name="inCounts">Array of input COUNTS</param>
        /// <param name="output">Array of output IDs</param>
        /// <param name="outCounts">Array of output COUNTS</param>
        /// <param name="description">LocalizedKey of description of this item</param>
        /// <param name="techID">Tech id, which unlock this recipe</param>
        public static void EditRecipe(int id, ERecipeType type, int time, int[] input, int[] inCounts,
            int[] output,
            int[] outCounts, string description, int techID, int gridIndex, string iconPath)
        {
            Instance.ThrowIfNotLoaded();
            RecipeProto proto = NewRecipeProto(id, type, time, input, inCounts, output, outCounts, description, techID, gridIndex, "", iconPath);
            recipeReplace.Add(proto.ID, proto);
        }


        /// <summary>
        /// Registers a TechProto for a technology.
        /// Total amount of each jello is calculated like this: N = H*C/3600, where H - total hash count, C - items per minute of jello.
        /// </summary>
        /// <param name="id"> UNIQUE ID of the technology. Note that if id > 2000 tech will be on upgrades page.</param>
        /// <param name="name">LocalizedKey of name of the tech</param>
        /// <param name="description">LocalizedKey of description of the tech</param>
        /// <param name="conclusion">LocalizedKey of conclusion of the tech upon completion</param>
        /// <param name="iconPath">Path to icon, starting from assets folder of your unity project</param>
        /// <param name="preTechs">Techs which lead to this tech</param>
        /// <param name="jellos">Items required to research the tech</param>
        /// <param name="jelloRate">Amount of items per minute required to research the tech</param>
        /// <param name="hashNeeded">Number of hashes needed required to research the tech</param>
        /// <param name="unlockRecipes">Once the technology has completed, what recipes are unlocked</param>
        /// <param name="position">Vector2 position of the technology on the technology screen</param>
        public static TechProto RegisterTech(int id, string name, string description, string conclusion,
            string iconPath, int[] preTechs, int[] jellos, int[] jelloRate, long hashNeeded,
            int[] unlockRecipes, Vector2 position)

        {
            Instance.ThrowIfNotLoaded();
            bool isLabTech = jellos.Any(itemId => LabComponent.matrixIds.Contains(itemId));


            TechProto proto = new TechProto
            {
                ID = id,
                Name = name,
                Desc = description,
                Published = true,
                Conclusion = conclusion,
                IconPath = iconPath,
                IsLabTech = isLabTech,
                PreTechs = preTechs,
                Items = jellos,
                ItemPoints = jelloRate,
                HashNeeded = hashNeeded,
                UnlockRecipes = unlockRecipes,
                AddItems = Array.Empty<int>(), // what items to gift after research is done
                AddItemCounts = Array.Empty<int>(),
                Position = position,
                PreTechsImplicit = Array.Empty<int>(), //Those funky implicit requirements
                UnlockFunctions = Array.Empty<int>(), //Upgrades.
                UnlockValues = Array.Empty<double>(),
                PropertyOverrideItems = Array.Empty<int>(),
                PropertyItemCounts = Array.Empty<int>(),
                PropertyOverrideItemArray = Array.Empty<IDCNT>()
            };

            foreach (int tech in preTechs)
            {
                if (techUpdateList.ContainsKey(tech))
                {
                    techUpdateList[tech].Add(proto);
                }
                else
                {
                    techUpdateList.Add(tech, new List<TechProto>(new[]{proto}));
                }
            }

            LDBTool.PreAddProto(proto);
            techs.Add(id, proto);

            return proto;
        }

        /// <summary>
        /// Changes already existing localized string.
        /// If new translation for a language is not specified it will not be modified!
        /// </summary>
        /// <param name="key">key of your target localized string</param>
        /// <param name="enTrans">New English translation for this key</param>
        /// <param name="cnTrans">New Chinese translation for this key</param>
        /// <param name="frTrans">New French translation for this key</param>
        public static void EditString(string key, string enTrans)
        {
            EditString(key, enTrans, "", "");
        }

        /// <summary>
        /// Changes already existing localized string.
        /// If new translation for a language is not specified it will not be modified!
        /// </summary>
        /// <param name="key">key of your target localized string</param>
        /// <param name="enTrans">New English translation for this key</param>
        /// <param name="cnTrans">New Chinese translation for this key</param>
        /// <param name="frTrans">New French translation for this key</param>
        public static void EditString(string key, string enTrans, string cnTrans)
        {
            EditString(key, enTrans, cnTrans, "");
        }

        /// <summary>
        /// Changes already existing localized string.
        /// If new translation for a language is not specified it will not be modified!
        /// </summary>
        /// <param name="key">key of your target localized string</param>
        /// <param name="enTrans">New English translation for this key</param>
        /// <param name="cnTrans">New Chinese translation for this key</param>
        /// <param name="frTrans">New French translation for this key</param>
        public static void EditString(string key, string enTrans, string cnTrans, string frTrans)
        {
            ThrowIfNotLoaded();
            StringProto stringProto = LDB.strings[key];

            stringProto.ENUS = enTrans;
            stringProto.ZHCN = cnTrans.Equals("") ? stringProto.ZHCN : cnTrans;
            stringProto.FRFR = frTrans.Equals("") ? stringProto.FRFR : frTrans;
        }


        /// <summary>
        /// Registers a new localized string
        /// </summary>
        /// <param name="key">UNIQUE key of your localizedKey</param>
        /// <param name="enTrans">English translation for this key</param>
        /// <param name="cnTrans">Chinese translation for this key</param>
        /// <param name="frTrans">French translation for this key</param>
        public static void RegisterString(string key, string enTrans, string cnTrans)
        {
            RegisterString(key, enTrans, cnTrans, "");
        }

        /// <summary>
        /// Registers a new localized string
        /// </summary>
        /// <param name="key">UNIQUE key of your localizedKey</param>
        /// <param name="enTrans">English translation for this key</param>
        /// <param name="cnTrans">Chinese translation for this key</param>
        /// <param name="frTrans">French translation for this key</param>
        public static void RegisterString(string key, string enTrans)
        {
            RegisterString(key, enTrans, "", "");
        }

        /// <summary>
        /// Registers a new localized string
        /// </summary>
        /// <param name="key">UNIQUE key of your localizedKey</param>
        /// <param name="enTrans">English translation for this key</param>
        /// <param name="cnTrans">Chinese translation for this key</param>
        /// <param name="frTrans">French translation for this key</param>
        public static void RegisterString(string key, string enTrans, string cnTrans, string frTrans)
        {
            ThrowIfNotLoaded();

            StringProto proto = new StringProto
            {
                Name = key,
                ENUS = enTrans,
                ZHCN = cnTrans.Equals("") ? enTrans : cnTrans,
                FRFR = frTrans.Equals("") ? enTrans : frTrans,
                ID = -1
            };

            LDBTool.PreAddProto(proto);
        }


        /// <summary>
        /// Register new MIDI instrument
        /// </summary>
        /// <param name="id">UNIQUE id of your sound</param>
        /// <param name="audioClipPath">Path to your audio clip. Clip name is taken from file name</param>
        /// <param name="volume">Default volume of MIDI instrument</param>
        /// <param name="length">Audio clip length</param>
        /// <param name="fadeIn">Audio fade in</param>
        /// <param name="fadeOut">Audio fade out</param>
        /// <returns>New Audio Proto</returns>
        public static AudioProto RegisterInstrument(int id, string audioClipPath, int volume, float length, float fadeIn, float fadeOut)
        {
            AudioProto audio = RegisterAudio(id, audioClipPath, 32, 1, 1, 0, 1, false, false, false);
            AddMIDIInstrument(id, audio, volume, length, fadeIn, fadeOut);
            return audio;
        }

        /// <summary>
        /// Register new audio that is localized
        /// </summary>
        /// <param name="id">UNIQUE id of your sound</param>
        /// <param name="audioClipPath">Path to your audio clip. Clip name is taken from file name</param>
        /// <param name="volume">Volume of your audio, range 0-1</param>
        /// <returns>New Audio Proto</returns>
        public static AudioProto RegisterLocalizedAudio(int id, string audioClipPath, float volume)
        {
            return RegisterAudio(id, audioClipPath, 1, volume, 1, 0, 0, false, true, true);
        }

        /// <summary>
        /// Register new audio 
        /// </summary>
        /// <param name="id">UNIQUE id of your sound</param>
        /// <param name="audioClipPath">Path to your audio clip. Clip name is taken from file name</param>
        /// <param name="volume">Volume of your audio, range 0-1</param>
        /// <param name="spatialBlend">Sets how much this audio is affected by 3D spatialisation calculations (attenuation, doppler etc). 0.0 makes the sound full 2D, 1.0 makes it full 3D.</param>
        /// <param name="loop">Should sound be played in a loop</param>
        /// <returns>New Audio Proto</returns>
        public static AudioProto RegisterAudio(int id, string audioClipPath, float volume, float spatialBlend, bool loop)
        {
            return RegisterAudio(id, audioClipPath, 1, volume, 1, 0, spatialBlend, loop, false, false);
        }

        /// <summary>
        /// Register new audio 
        /// </summary>
        /// <param name="id">UNIQUE id of your sound</param>
        /// <param name="audioClipPath">Path to your audio clip. Clip name is taken from file name</param>
        /// <param name="clipCount">How many clips do you have? A number is appended to the end of the name like this name-5</param>
        /// <param name="volume">Volume of your audio, range 0-1</param>
        /// <param name="pitch">Pitch of your audio</param>
        /// <param name="pitchRandomness">How random is pitch, from 0 to 1</param>
        /// <param name="spatialBlend">Sets how much this audio is affected by 3D spatialisation calculations (attenuation, doppler etc). 0.0 makes the sound full 2D, 1.0 makes it full 3D.</param>
        /// <param name="loop">Should sound be played in a loop</param>
        /// <param name="localized">Does this audio have localization. Language name is appended to the end. For example: name-en. Language postfix is always last</param>
        /// <param name="bypassEffect">Bypass effects (Applied from filter components or global listener filters).</param>
        /// <returns>New Audio Proto</returns>
        public static AudioProto RegisterAudio(int id, string audioClipPath, int clipCount, float volume, float pitch, float pitchRandomness, float spatialBlend, bool loop, bool localized, bool bypassEffect)
        {
            Instance.ThrowIfNotLoaded();
            string name = audioClipPath.Split('/', '\\').Last();

            AudioProto proto = new AudioProto()
            {
                ID = id,
                Name = name,
                ClipPath = audioClipPath,
                ClipCount = clipCount,
                Pitch = pitch,
                Volume = volume,
                PitchRandomness = pitchRandomness,
                SpatialBlend = spatialBlend,
                Loop = loop,
                Localized = localized,
                BypassEffect = bypassEffect
            };
            
            LDBTool.PreAddProto(proto);
            audios.Add(id, proto);

            return proto;
        }
        
        /// <summary>
        /// Edit existing audio proto
        /// </summary>
        /// <param name="id">ID of target AudioProto</param>
        /// <param name="newAudioClipPath">New clip path. </param>
        /// <param name="volume">New volume. Volume of your audio, range 0-1</param>
        /// <param name="pitch">New pitch. Pitch of your audio</param>
        /// <param name="pitchRandomness">New pitch randomness. How random is pitch, from 0 to 1</param>
        /// <param name="spatialBlend">New spatial blend. Sets how much this audio is affected by 3D spatialisation calculations (attenuation, doppler etc). 0.0 makes the sound full 2D, 1.0 makes it full 3D.</param>
        public static void EditAudio(int id, string newAudioClipPath, float volume, float pitch, float pitchRandomness, float spatialBlend)
        {
            Instance.ThrowIfNotLoaded();
            AudioProto proto = LDB.audios.Select(id);

            proto.ClipPath = newAudioClipPath;
            proto.Volume = volume;
            proto.Pitch = pitch;
            proto.PitchRandomness = pitchRandomness;
            proto.SpatialBlend = spatialBlend;
        }

        internal static void AddMIDIInstrument(int id, AudioProto audio, int volume, float length, float fadeIn, float fadeOut)
        {
            List<int> keys = new List<int>();
            List<Vector2> ranges = new List<Vector2>();
            for (int i = 7; i <= 100; i += 3)
            {
                keys.Add(i);
                ranges.Add(new Vector2(i-1, i+1));
            }

            MIDIProto proto = new MIDIProto()
            {
                ID = id,
                Instrument = true,
                StandardKey = 58,
                PitchRange = new[] {6, 101},
                Key = keys.ToArray(),
                KeyRange = ranges.ToArray(),
                Volume = volume,
                Length = length,
                FadeInDuration = fadeIn,
                FadeOutDuration = fadeOut,
                Name = audio.Name
            };
            
            LDBTool.PreAddProto(proto);
            midiProtos.Add(proto.ID, proto);
        }

        /// <summary>
        /// Register new signal
        /// </summary>
        /// <param name="id">UNIQUE id of new signal</param>
        /// <param name="iconPath">Path to icon, starting from assets folder of your unity project</param>
        /// <param name="gridIndex">Index in picker menu, format : PYXX, P - page (should be 3)</param>
        /// <param name="name">LocalizedKey of name of the signal</param>
        /// <param name="desc">LocalizedKey of description of the signal</param>
        /// <returns>New Signal Proto</returns>
        public static SignalProto RegisterSignal(int id, string iconPath, int gridIndex, string name, string desc)
        {
            Instance.ThrowIfNotLoaded();
            SignalProto proto = new SignalProto()
            {
                ID = id,
                IconPath = iconPath,
                Name = name,
                GridIndex = gridIndex,
                description = desc
            };
            
            LDBTool.PreAddProto(proto);
            signals.Add(proto.ID, proto);
            return proto;
        }

        /// <summary>
        /// Modify existing signal
        /// </summary>
        /// <param name="id">ID of target signal</param>
        /// <param name="iconPath">New path to icon, starting from assets folder of your unity project</param>
        /// <param name="gridIndex">New grid index in picker menu, format : PYXX, P - page (should be 3)</param>
        /// <param name="name">New localizedKey of name of the signal</param>
        public static void EditSignal(int id, string iconPath, int gridIndex, string name)
        {
            Instance.ThrowIfNotLoaded();
            SignalProto proto = LDB.signals.Select(id);

            if (!iconPath.Equals("")) proto.IconPath = iconPath;
            proto.GridIndex = gridIndex;
            proto.Name = name;
        }
    }
}