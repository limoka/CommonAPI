using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx.Configuration;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using xiaoye97;
using Object = UnityEngine.Object;

// ReSharper disable InconsistentNaming

namespace CommonAPI
{
    [UsedImplicitly]
    public class ResourceData
    {
        public string modId;
        public string modPath;
        public string keyWord;

        public AssetBundle bundle;
        public string vertaFolder;

        public ResourceData(string modId, string keyWord, string modPath)
        {
            this.modId = modId;
            this.modPath = modPath;
            this.keyWord = keyWord;
        }

        public bool HasVertaFolder()
        {
            return !vertaFolder.Equals("");
        }

        public bool HasAssetBundle()
        {
            return bundle != null;
        }

        public void LoadAssetBundle(string bundleName)
        {
            bundle = AssetBundle.LoadFromFile($"{modPath}/{bundleName}");
            if (bundle == null)
            {
                throw new LoadException($"Failed to load asset bundle at {modPath}/{bundleName}");
            }
        }

        public void ResolveVertaFolder()
        {
            FileInfo folder = new FileInfo($"{modPath}/Verta/");
            FileInfo folder1 = new FileInfo($"{modPath}/plugins/");

            if (Directory.Exists(folder.Directory?.FullName))
            {
                vertaFolder = modPath;
            }
            else if (Directory.Exists(folder1.Directory?.FullName))
            {
                vertaFolder = $"{modPath}/plugins";
            }
            else
            {
                vertaFolder = "";
                throw new LoadException($"Failed to resolve verta folder at {modPath}");
            }
        }
    }

    public class LoadException : Exception
    {
        public LoadException(string message) : base(message) { }
    }

    public class LodMaterials
    {
        public Material[] this[int key]
        {
            get => materials[key];
            set => materials[key] = value;
        }

        public LodMaterials()
        {
            materials = new Material[4][];
        }
        
        public LodMaterials(int lod, Material[] lod0)
        {
            materials = new Material[4][];
            AddLod(lod, lod0);
        }
        
        public LodMaterials(Material[] lod0) : this(0, lod0)
        {
        }

        public void AddLod(int lod, Material[] mats)
        {
            if (lod >= 0 && lod < 4)
            {
                materials[lod] = mats;
            }
        }

        public bool HasLod(int lod)
        {
            return materials[lod] != null;
        }

        public Material[][] materials;
    }

    public static class ProtoRegistry
    {
        //Local proto dictionaries
        internal static Dictionary<int, ItemProto> items = new Dictionary<int, ItemProto>();
        internal static Dictionary<int, int> itemUpgradeList = new Dictionary<int, int>();

        internal static Dictionary<int, RecipeProto> recipes = new Dictionary<int, RecipeProto>();
        internal static Dictionary<int, RecipeProto> recipeReplace = new Dictionary<int, RecipeProto>();

        internal static Dictionary<int, StringProto> strings = new Dictionary<int, StringProto>();
        internal static int lastStringId = 1000;

        internal static Dictionary<string, StringProto> stringReplace = new Dictionary<string, StringProto>();

        internal static Dictionary<int, TechProto> techs = new Dictionary<int, TechProto>();
        internal static Dictionary<int, TechProto> techUpdateList = new Dictionary<int, TechProto>();

        internal static Dictionary<int, ModelProto> models = new Dictionary<int, ModelProto>();
        internal static Dictionary<string, LodMaterials> modelMats = new Dictionary<string, LodMaterials>();

        internal static List<ResourceData> modResources = new List<ResourceData>();

        public static Registry recipeTypes = new Registry();
        public static List<List<int>> recipeTypeLists = new List<List<int>>();


        public static event Action onLoadingFinished;


        internal static int[] textureNames;
        internal static string[] spriteFileExtensions;

        internal static void Init()
        {
            int mainTex = Shader.PropertyToID("_MainTex");
            int normalTex = Shader.PropertyToID("_NormalTex");
            int msTex = Shader.PropertyToID("_MS_Tex");
            int emissionTex = Shader.PropertyToID("_EmissionTex");
            int emissionJitterTex = Shader.PropertyToID("_EmissionJitterTex");

            textureNames = new[] {mainTex, normalTex, msTex, emissionTex, emissionJitterTex};
            spriteFileExtensions = new[] {".jpg", ".png", ".tif"};

            recipeTypeLists.Add(null);

            LDBTool.PostAddDataAction += OnPostAdd;
            LDBTool.EditDataAction += EditProto;
        }

        /// <summary>
        /// Registers mod resources for loading
        /// </summary>
        /// <param name="resource"></param>
        public static void AddResource(ResourceData resource)
        {
            modResources.Add(resource);
        }

        public static int RegisterRecipeType(string typeId)
        {
            int id = recipeTypes.Register(typeId);
            if (id >= recipeTypeLists.Capacity)
            {
                recipeTypeLists.Capacity *= 2;
            }

            recipeTypeLists.Add(new List<int>());
            return id;
        }

        public static bool BelongsToType(this RecipeProto proto, int typeId)
        {
            if (typeId >= recipeTypeLists.Count) return false;

            return recipeTypeLists[typeId].BinarySearch(proto.ID) >= 0;
        }

        //Post register fixups
        private static void OnPostAdd()
        {
            foreach (var kv in models)
            {
                kv.Value.Preload();
                PrefabDesc pdesc = kv.Value.prefabDesc;

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
                oldTech.postTechArray = oldTech.postTechArray.AddToArray(kv.Value);
            }

            onLoadingFinished?.Invoke();

            CommonAPIPlugin.logger.LogInfo("Post loading is complete!");
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

        internal static bool HasStringIdRegisted(int id)
        {
            if (LDB.strings.dataIndices.ContainsKey(id))
            {
                return true;
            }

            if (strings.ContainsKey(id))
            {
                return true;
            }

            return false;
        }


        internal static int FindAvailableStringID()
        {
            int id = lastStringId + 1;

            while (true)
            {
                if (!HasStringIdRegisted(id))
                {
                    break;
                }

                if (id > 12000)
                {
                    CommonAPIPlugin.logger.LogError("Failed to find free index!");
                    throw new ArgumentException("No free indices available!");
                }

                id++;
            }

            lastStringId = id;

            return id;
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
            string[] textures = null, string[] keywords = null, int[] textureIDs = null)
        {
            ColorUtility.TryParseHtmlString(color, out Color newCol);

            Material mainMat = new Material(Shader.Find(shaderName))
            {
                shaderKeywords = keywords ?? new[] {"_ENABLE_VFINST"},
                color = newCol,
                name = materialName
            };

            if (textures == null) return mainMat;
            int[] texIds = textureIDs ?? textureNames;

            for (int i = 0; i < textures.Length; i++)
            {
                if (i >= texIds.Length) continue;

                Texture2D texture = Resources.Load<Texture2D>(textures[i]);
                mainMat.SetTexture(texIds[i], texture);
            }

            return mainMat;
        }

        public static int GetGridIndex(int tab, int x, int y)
        {
            return tab * 1000 + y * 100 + x;
        }


        //All of these register a specified proto in LDBTool

        /// <summary>
        /// Registers a ModelProto
        /// </summary>
        /// <param name="id">UNIQUE id of your model</param>
        /// <param name="prefabPath">Path to the prefab, starting from asset folder in your unity project</param>
        /// <param name="mats">List of materials to use</param>
        public static ModelProto RegisterModel(int id, string prefabPath, Material[] mats = null)
        {
            ModelProto model = new ModelProto
            {
                Name = id.ToString(),
                PrefabPath = prefabPath,
                ID = id
            };

            LDBTool.PreAddProto(ProtoType.Model, model);
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
            int[] descFields, int buildIndex, int grade = 0, int[] upgradesIDs = null)
        {
            ModelProto model = new ModelProto
            {
                Name = id.ToString(),
                PrefabPath = prefabPath,
                ID = id
            };

            AddModelToItemProto(model, proto, descFields, buildIndex, grade, upgradesIDs);

            LDBTool.PreAddProto(ProtoType.Model, model);
            models.Add(model.ID, model);

            if (mats != null)
                modelMats.Add(prefabPath, new LodMaterials(mats));

            return model;
        }

        public static void AddLodMaterials(string prefabPath, int lod, Material[] mats)
        {
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
        public static void AddModelToItemProto(ModelProto model, ItemProto item, int[] descFields, int buildIndex, int grade = 0, int[] upgradesIDs = null)
        {
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
            int gridIndex, int stackSize = 50, EItemType type = EItemType.Material)
        {
            //int id = findAvailableID(1001, LDB.items, items);

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

            LDBTool.PreAddProto(ProtoType.Item, proto);

            items.Add(proto.ID, proto);
            return proto;
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
            int[] outCounts, string description, int techID = 0, int gridIndex = 0)
        {
            if (type >= recipeTypeLists.Count) throw new ArgumentException($"Type {type} is not registered!");

            RecipeProto recipe = RegisterRecipe(id, ERecipeType.Custom, time, input, inCounts, output, outCounts, description, techID, gridIndex);

            recipeTypeLists[type].Add(recipe.ID);
            Algorithms.ListSortedAdd(recipeTypeLists[type], recipe.ID);

            return recipe;
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
            int[] outCounts, string description, int techID = 0, int gridIndex = 0)
        {
            RecipeProto proto = NewRecipeProto(id, type, time, input, inCounts, output, outCounts, description, techID, gridIndex);

            LDBTool.PreAddProto(ProtoType.Recipe, proto);
            recipes.Add(id, proto);

            return proto;
        }

        private static RecipeProto NewRecipeProto(int id, ERecipeType type, int time, int[] input, int[] inCounts, int[] output, int[] outCounts,
            string description,
            int techID, int gridIndex)
        {
            if (output.Length > 0)
            {
                ItemProto first = items.ContainsKey(output[0]) ? items[output[0]] : LDB.items.Select(output[0]);

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
                    IconPath = first.IconPath,
                    Name = first.Name + "Recipe",
                    preTech = tech,
                    ID = id
                };

                return proto;
            }

            throw new ArgumentException("Output array must not be empty");
        }

       /* private static void CopyRecipeProto(RecipeProto target, RecipeProto source)
        {
            target.Type = source.Type;
            target.Handcraft = source.Handcraft;
            target.TimeSpend = source.TimeSpend;
            target.Items = source.Items;
            target.ItemCounts = source.ItemCounts;
            target.Results = source.Results;
            target.ResultCounts = source.ResultCounts;
            target.Description = source.Description;
            target.GridIndex = source.GridIndex;
            target.IconPath = source.IconPath;
            target.Name = source.Name;
            target.preTech = source.preTech;
            target.ID = source.ID;
        }
*/
        public static void ReplaceRecipe(int id, ERecipeType type, int time, int[] input, int[] inCounts,
            int[] output,
            int[] outCounts, string description, int techID = 0, int gridIndex = 0)
        {
            RecipeProto proto = NewRecipeProto(id, type, time, input, inCounts, output, outCounts, description, techID, gridIndex);
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
                AddItems = new int[] { }, // what items to gift after research is done
                AddItemCounts = new int[] { },
                Position = position,
                PreTechsImplicit = new int[] { }, //Those funky implicit requirements
                UnlockFunctions = new int[] { }, //Upgrades.
                UnlockValues = new double[] { },
            };

            foreach (int tech in preTechs)
            {
                //Do not do LDB.techs.Select here, proto could be not added yet.
                techUpdateList.Add(tech, proto);
            }

            LDBTool.PreAddProto(ProtoType.Tech, proto);
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
        public static void EditString(string key, string enTrans, string cnTrans = "", string frTrans = "")
        {
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
        public static void RegisterString(string key, string enTrans, string cnTrans = "", string frTrans = "")
        {
            //int id = FindAvailableStringID(3500);

            StringProto proto = new StringProto
            {
                Name = key,
                ENUS = enTrans,
                ZHCN = cnTrans.Equals("") ? enTrans : cnTrans,
                FRFR = frTrans.Equals("") ? enTrans : frTrans,
                ID = -1
            };

            LDBTool.PreAddProto(ProtoType.String, proto);
        }
    }


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

    //Loading custom resources
    [HarmonyPatch]
    static class ResourcesPatch
    {
        [HarmonyPatch(typeof(Resources), "Load", typeof(string), typeof(Type))]
        [HarmonyPrefix]
        public static bool Prefix(ref string path, Type systemTypeInstance, ref Object __result)
        {
            foreach (ResourceData resource in ProtoRegistry.modResources)
            {
                if (!path.Contains(resource.keyWord) || !resource.HasAssetBundle()) continue;

                if (resource.bundle.Contains(path + ".prefab") && systemTypeInstance == typeof(GameObject))
                {
                    Object myPrefab = resource.bundle.LoadAsset(path + ".prefab");
                    CommonAPIPlugin.logger.LogDebug($"Loading registered asset {path}: {(myPrefab != null ? "Success" : "Failure")}");

                    if (!ProtoRegistry.modelMats.ContainsKey(path))
                    {
                        __result = myPrefab;
                        return false;
                    }

                    LodMaterials mats = ProtoRegistry.modelMats[path];
                    if (myPrefab != null && mats.HasLod(0))
                    {
                        MeshRenderer[] renderers = ((GameObject) myPrefab).GetComponentsInChildren<MeshRenderer>();
                        foreach (MeshRenderer renderer in renderers)
                        {
                            Material[] newMats = new Material[renderer.sharedMaterials.Length];
                            for (int i = 0; i < newMats.Length; i++)
                            {
                                newMats[i] = mats[0][i];
                            }

                            renderer.sharedMaterials = newMats;
                        }
                    }

                    __result = myPrefab;
                    return false;
                }

                foreach (string extension in ProtoRegistry.spriteFileExtensions)
                {
                    if (!resource.bundle.Contains(path + extension)) continue;

                    Object mySprite = resource.bundle.LoadAsset(path + extension, systemTypeInstance);

                    CommonAPIPlugin.logger.LogDebug($"Loading registered asset {path}: {(mySprite != null ? "Success" : "Failure")}");

                    __result = mySprite;
                    return false;
                }
            }

            return true;
        }
    }

    [HarmonyPatch]
    static class VertaBufferPatch
    {
        [HarmonyPatch(typeof(VertaBuffer), "LoadFromFile")]
        [HarmonyPrefix]
        public static bool Prefix(ref string filename)
        {
            foreach (var resource in ProtoRegistry.modResources)
            {
                if (!filename.ToLower().Contains(resource.keyWord.ToLower()) || !resource.HasVertaFolder()) continue;

                string newName = $"{resource.vertaFolder}/{filename}";
                if (!File.Exists(newName)) continue;

                filename = newName;
                CommonAPIPlugin.logger.LogDebug("Loading registered verta file " + filename);
                break;
            }

            return true;
        }
    }
}