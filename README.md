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

Unzip folder `patchers` into `Dyson Sphere Program/BepInEx/patchers/CommonAPI/` (Create folder named `CommonAPI`)<br/>
Unzip folder `plugins` into `Dyson Sphere Program/BepInEx/plugins/CommonAPI/`. (Create folder named `CommonAPI`)<br/>

## Feedback and Bug Report
Feel free to contact me via Discord (Kremnev8#3756) for any feedback, bug-reports or suggestions.

# How develop mods using CommonAPI
All features are written as self-contained modules (Inspired by [R2API](https://github.com/risk-of-thunder/R2API)). By default NO modules are loaded. To use a module at the top of your BepInEx plugin class add an attribute `CommonAPISubmoduleDependency`. That will ensure that specified modules are loaded. Make sure you don't ask to load modules that you are not using.

## How to setup development environment
1. Download and install [CommonAPI](https://dsp.thunderstore.io/package/CommonAPI/CommonAPI/) and its dependencies
3. Create development environment. You can find how to do that [here](https://docs.bepinex.dev/master/articles/dev_guide/plugin_tutorial/index.html#sidetoggle)
4. Add LDBTool and CommonAPI assemblies to your references.
5. You also likely will need a Unity Project. You can find instructions on setting that up [here](https://github.com/kremnev8/DSP-Mods/wiki/Setting-up-development-environment)

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

This library is still under development. Used by many of my mods.
