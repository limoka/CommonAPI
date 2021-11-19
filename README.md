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

More will come in the future. If want write your own module and add it to the list you can open a Pull Request. Contrubitions are welcome.


All features are written as self-contained modules (Inspired by [R2API](https://github.com/risk-of-thunder/R2API)). To use a module at the top of your BepInEx plugin class add an attribute `CommonAPISubmoduleDependency`.
# Example
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
For more info on setting up dev environment and examples check [this repo](https://github.com/kremnev8/DSP-Mods)
