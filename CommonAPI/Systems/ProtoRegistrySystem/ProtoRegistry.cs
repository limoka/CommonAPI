using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;
using CommonAPI.Patches;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using xiaoye97;
using Object = UnityEngine.Object;

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

    [CommonAPISubmodule]
    public static class ProtoRegistry
    {
        //Local proto dictionaries
        internal static Dictionary<int, ItemProto> items = new Dictionary<int, ItemProto>();
        internal static Dictionary<int, int> itemUpgradeList = new Dictionary<int, int>();

        internal static Dictionary<int, RecipeProto> recipes = new Dictionary<int, RecipeProto>();
        internal static Dictionary<int, RecipeProto> recipeReplace = new Dictionary<int, RecipeProto>();

        internal static Dictionary<int, StringProto> strings = new Dictionary<int, StringProto>();
        internal static int lastStringId = 1000;

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
        
        /// <summary>
        /// Return true if the submodule is loaded.
        /// </summary>
        public static bool Loaded {
            get => _loaded;
            internal set => _loaded = value;
        }

        private static bool _loaded;


        [CommonAPISubmoduleInit(Stage = InitStage.SetHooks)]
        internal static void SetHooks()
        {
            
            CommonAPIPlugin.harmony.PatchAll(typeof(LDBToolPatch));
            CommonAPIPlugin.harmony.PatchAll(typeof(LDBToolPatch2));
            CommonAPIPlugin.harmony.PatchAll(typeof(ResourcesPatch));
            CommonAPIPlugin.harmony.PatchAll(typeof(StorageComponentPatch));
            CommonAPIPlugin.harmony.PatchAll(typeof(UIBuildMenuPatch));
            CommonAPIPlugin.harmony.PatchAll(typeof(VertaBufferPatch));
            
            CommonAPIPlugin.harmony.PatchAll(typeof(AssemblerComponentPatch));
            CommonAPIPlugin.harmony.PatchAll(typeof(UIAssemblerWindowPatch));
        }


        [CommonAPISubmoduleInit(Stage = InitStage.Load)]
        internal static void load()
        {
            CommonAPIPlugin.registries.Add($"{ CommonAPIPlugin.ID}:RecipeTypeRegistry", recipeTypes);
            
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
        
        internal static void ThrowIfNotLoaded()
        {
            if (!Loaded)
            {
                throw new InvalidOperationException(
                    $"{nameof(ProtoRegistry)} is not loaded. Please use [{nameof(CommonAPISubmoduleDependency)}(nameof({nameof(ProtoRegistry)})]");
            }
        }

        /// <summary>
        /// Registers mod resources for loading
        /// </summary>
        /// <param name="resource"></param>
        public static void AddResource(ResourceData resource)
        {
            ThrowIfNotLoaded();
            modResources.Add(resource);
        }

        /// <summary>
        /// Register new recipe type. This can be used to create new machine types independent of vanilla machines.
        /// </summary>
        /// <param name="typeId">Unique string ID</param>
        /// <returns>Assigned integer ID</returns>
        public static int RegisterRecipeType(string typeId)
        {
            ThrowIfNotLoaded();
            int id = recipeTypes.Register(typeId);
            if (id >= recipeTypeLists.Capacity)
            {
                recipeTypeLists.Capacity *= 2;
            }

            recipeTypeLists.Add(new List<int>());
            return id;
        }

        /// <summary>
        /// Checks if provided <see cref="RecipeProto"/> belongs to recipe type
        /// </summary>
        /// <param name="proto">Recipe</param>
        /// <param name="typeId">Integer ID</param>
        /// <returns></returns>
        public static bool BelongsToType(this RecipeProto proto, int typeId)
        {
            ThrowIfNotLoaded();
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
            ThrowIfNotLoaded();
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


        //All of these register a specified proto in LDBTool

        /// <summary>
        /// Registers a ModelProto
        /// </summary>
        /// <param name="id">UNIQUE id of your model</param>
        /// <param name="prefabPath">Path to the prefab, starting from asset folder in your unity project</param>
        /// <param name="mats">List of materials to use</param>
        public static ModelProto RegisterModel(int id, string prefabPath, Material[] mats = null, int rendererType = 0)
        {
            ThrowIfNotLoaded();
            ModelProto model = new ModelProto
            {
                Name = id.ToString(),
                PrefabPath = prefabPath,
                ID = id,
                RendererType = rendererType
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
            int[] descFields, int buildIndex, int grade = 0, int[] upgradesIDs = null, int rendererType = 0)
        {
            ThrowIfNotLoaded();
            ModelProto model = new ModelProto
            {
                Name = id.ToString(),
                PrefabPath = prefabPath,
                ID = id,
                RendererType = rendererType
            };

            AddModelToItemProto(model, proto, descFields, buildIndex, grade, upgradesIDs);

            LDBTool.PreAddProto(ProtoType.Model, model);
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
            ThrowIfNotLoaded();
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
            ThrowIfNotLoaded();
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
            ThrowIfNotLoaded();
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
            ThrowIfNotLoaded();
            if (type >= recipeTypeLists.Count) throw new ArgumentException($"Recipe Type: {type} is not registered!");

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
            ThrowIfNotLoaded();
            RecipeProto proto = NewRecipeProto(id, type, time, input, inCounts, output, outCounts, description, techID, gridIndex);

            LDBTool.PreAddProto(ProtoType.Recipe, proto);
            recipes.Add(id, proto);

            return proto;
        }

        private static RecipeProto NewRecipeProto(int id, ERecipeType type, int time, int[] input, int[] inCounts, int[] output, int[] outCounts,
            string description,
            int techID, int gridIndex)
        {
            ThrowIfNotLoaded();
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
            int[] outCounts, string description, int techID = 0, int gridIndex = 0)
        {
            ThrowIfNotLoaded();
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
            ThrowIfNotLoaded();
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
        public static void RegisterString(string key, string enTrans, string cnTrans = "", string frTrans = "")
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

            LDBTool.PreAddProto(ProtoType.String, proto);
        }
    }
}