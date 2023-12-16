# Submodules
## What is a submodule
Each folder on this page defines a submodule. Submodule is a building block of Common API. Each submodule has a single responsibility. For example ProtoRegistrySystem allows to register new Proto objects into game's LDB to add new content.

A submodule is not loaded unless another mod requests that. This is needed to ensure that CommonAPI does not break in an event where one of submodules gets broken due to game update. If that happens the player only needs to uninstall all mods that depend on the broken module, and other mods will continue to work. Because of this it is preferable to split unrelated functions into different submodules.

## Using submodules in your mods
To use any submodule you have to declare that in your plugin class. Use `CommonAPISubmoduleDependency` to declare all used submodules. make sure to only request submodules your code actually uses. Also don't forget to declare a dependency on CommonAPI plugin.
```cs
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

## Creating Submodules
To create a new submodule you need to create a new folder with the name of the module. In it create a new class with the same name. Here is a template of submodule class:
```cs
public static class SubmoduleName : BaseSubmodule
{

    public static void SomeAPIMethod()
    {
        // Ensure that you call this method in ALL interface methods
        // This ensures that if your module is not loaded, a error will be thrown
        Instance.ThrowIfNotLoaded();
    }
    
    internal static SubmoduleName Instance => CommonAPIPlugin.GetModuleInstance<SubmoduleName>();
  
    // To declare submodule dependency use this property
    internal override Type[] Dependencies => new[] { typeof(LocalizationModule) };
  
    internal override void SetHooks()
    {
        // Register all patches needed for this submodule here
    }

    internal override void load()
    {
        // Other actions not related to patches can be done here
    }
    
    internal override void PostLoad()
    {
        // This method will be called after all modules are loaded
        // Here you can use other modules functions.
    }
}

```
