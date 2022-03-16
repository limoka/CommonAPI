# CommonAPI
A modding library for Dyson Sphere Program. Provides multiple features to make adding custom content to DSP easier.


# List of features
- Easily register new items, recipes and more using [ProtoRegistry](https://github.com/kremnev8/CommonAPI/tree/master/CommonAPI/Systems/ProtoRegistrySystem) system
- Create new buildings with custom behavior and custom UI using [ComponentSystem](https://github.com/kremnev8/CommonAPI/tree/master/CommonAPI/Systems/ComponentSystem)
- Register new recipe types. This allows to create new machine types without writing almost any code. 
- Register and use custom tabs.
- Register new KeyBinds that players can rebind
- Easily create new systems that exist in each Planet or Star. An example of such system is [ComponentSystem](https://github.com/kremnev8/CommonAPI/tree/master/CommonAPI/Systems/ComponentSystem)
- Picker Extension tool allows to extend behavior of Item and Recipe pickers. For example use any filter defined by a function.
- Support translation for at least for English, Chinese and Franch by using String Protos

Full list of modules and other utilities can be found [here](https://github.com/kremnev8/CommonAPI/wiki).
More will come in the future. If want write your own module and add it to the list you can open a Pull Request. Contrubitions are welcome.

# Installation
### With Mod Manager

Simply open the mod manager (if you don't have it install it [here](https://dsp.thunderstore.io/package/ebkr/r2modman/)), select **CommonAPI by CommonAPI**, then **Download**.

If prompted to download with dependencies, select `Yes`.
Then just click **Start modded**, and the game will run with the mod installed.

### Manually
Install BepInEx from [here](https://dsp.thunderstore.io/package/xiaoye97/BepInEx/)<br/>
Install LDBTool from [here](https://dsp.thunderstore.io/package/xiaoye97/LDBTool/)<br/>
Install DSPModSave from [here](https://dsp.thunderstore.io/package/CommonAPI/DSPModSave/)<br/>

Unzip folder `patchers` into `Dyson Sphere Program/BepInEx/patchers/CommonAPI/` (Create folder named `CommonAPI`)<br/>
Unzip folder `plugins` into `Dyson Sphere Program/BepInEx/plugins/CommonAPI/`. (Create folder named `CommonAPI`)<br/>

## Feedback and Bug Report
Feel free to contact me via Discord (Kremnev8#3756) for any feedback, bug-reports or suggestions.

# How develop mods using CommonAPI
All features are written as self-contained modules (Inspired by [R2API](https://github.com/risk-of-thunder/R2API)). By default NO modules are loaded. To use a module at the top of your BepInEx plugin class add an attribute `CommonAPISubmoduleDependency`. That will ensure that specified modules are loaded. Make sure you don't ask to load modules that you are not using.

## How to setup development environment
1. Download and install [CommonAPI](https://dsp.thunderstore.io/package/CommonAPI/CommonAPI/) and its dependencies
2. Create development environment. You can find how to do that [here](https://docs.bepinex.dev/master/articles/dev_guide/plugin_tutorial/index.html#sidetoggle)
3. Add LDBTool, DSPModSave and CommonAPI assemblies to your references. You can use NuGet to get them. You can find them by typing `DysonSphereProgram.Modding` into nuget package search.
4. You also likely will need a Unity Project. You can find instructions on setting that up [here](https://github.com/kremnev8/DSP-Mods/wiki/Setting-up-development-environment)

### Usage Example
```csharp
[BepInPlugin(GUID, NAME, VERSION)]

[BepInDependency(CommonAPIPlugin.GUID)]
[CommonAPISubmoduleDependency(nameof(ProtoRegistry), nameof(CustomDescSystem))]
public class MyPlugin : BaseUnityPlugin
{
    public const string MODID = "myplugin";
    public const string GUID = "org.myname.plugin." + MODID;
    public const string NAME = "My Plugin";
    
    void Awake()
    {
        //Make use of modules here
    }
}
```

This library is still under development.

## Changelog
### v1.4.6
- Fix dynamic KeyBind ID assignment and migration being broken. Playes might lose some of previously rebound keybinds.
### v1.4.5
- Fix issues adding multiple techologies with the same pretech
- KeyBinds now dynamically assign ID's. To all modders using Custom KeyBinds: please stop assigning ID's when calling `RegisterKeyBind()`
- FactoryComponent now has a method `UpdateNeeds()` that allows to set entityNeeds. 
### v1.4.4
- Add extension methods for customId and customType fields on EntityData class
### v1.4.3
- Fixed `GetTabId` being impossible to call
- Improved appearance of mod created tabs
### v1.4.2
- Fix NRE in UISingalTip
### v1.4.1
- Added UIWindowResize class, made by Raptor
- Added ability to specify iconPath and name for recipes manually
### v1.4.0
- Fix lava ocean type being displayed as missing item
- Allow submodules have dependencies
- Add AssemblerRecipeSystem
- Refactor PickerExtensionSystem
- Allow adding Signal Proto using ProtoRegistry
### v1.3.4
- Fix missing items appearing instead of no item id 0
### v1.3.3
- Fix missing items being broken. Also make it possible to delete them
### v1.3.2
- Change StartModLoad function behavior
### v1.3.1
- Now Machines added by mods will be automatically removed from save game if mod is uninstalled.
- Corrected Game version CommonAPI is built for.
### v1.3.0
- Add ability to register Audio using ProtoRegistry
- Updated LDBTool to 2.0.1. Please make sure you are using 2.0.0 or higher.
### v1.2.2
- Added plugin catergories on Thunderstore page.
### v1.2.1
- Added ability to load modules manually. Useful for testing with ScriptEngine.
### v1.2.0
- Migrated to CommonAPI-DSPModSave package.
### v1.1.0
- Renamed CustomPlanetSystem to PlanetExtensionSystem
- Renamed CustomStarSystem to StarExtensionSystem
- Add show locked item and recipes feature to PickerExtensionModule
- Improved Icon Generator
### v1.0.1
- Fix issues selecting recipes in Assembler UI
### v1.0.0
- Initial Release
