# Planet Extension

PlanetExtension is a module that makes implementing planet based logic easier. An example would be Component Extension module, Power System, or Cargo Traffic. Such a system always has one instance per planet, with or without update methods.

## Usage
Make sure to add `[CommonAPISubmoduleDependency(nameof(PlanetExtensionSystem))]` to your plugin attributes. This will load the submodule.

First create a new class with this template
```cs
public class MyPlanetExtension : IPlanetExtension
{
    // Do not modify this code. This is template to help with dynamic ID assignment
    public static readonly string systemID = $"{MyMod.ID}:MyPlanetExtension";
    
    private static int _cachedId;
    public static int cachedId
    {
        get
        {
            if (_cachedId == 0)
                _cachedId = PlanetExtensionSystem.registry.GetUniqueId(systemID);
            
            return _cachedId;
        }
    }
  
    private PlanetFactory factory;

    // This method will get called when instance of your extension will get created
    // factory is PlanetFactory your extension belongs to.
    public void Init(PlanetFactory factory)
    {
        this.factory = factory;
        // ...
    }

    // Read your saved state here
    // All of your state is stored in .moddsv file near main save file
    public void Import(BinaryReader r)
    {
        int ver = r.ReadInt32();
        // ...
    }


    // Free and destory used objects here
    public void Free()
    {
        // ...
    }

    // Save your state here
    // All of your state is stored in .moddsv file near main save file
    public void Export(BinaryWriter w)
    {
        w.Write(0);
        // ...
    }
}
```

After creating your class make sure to register it in your plugin's awake call:

```cs
PlanetExtensionSystem.registry.Register($"{MyMod.ID}:MyPlanetExtension", typeof(MyPlanetExtension));
```

### Acessing data

To access data of a particular instance of your extension you can use this extension method:
```cs
MyPlanetExtension extension = factory.GetSystem<MyPlanetExtension>(MyPlanetExtension.cachedId);
```

### EntityData Properties

When using Planet Extnesion system you likely might want to store some data in EntityData class in your own properties. This can be achieved by using `EntityDataExtensions`. <br>
First regsiter your property. You must specify it's type and name here:
```cs
// Place on top of convinient class
public const string MY_PROPERTY_NAME = MyMod.MODID + ":MyProperty";

// In your awake call
EntityDataExtensions.DefineProperty(MY_PROPERTY_NAME, IntPropertySerializer.instance);
```
Type is defined by providing valid serializer. By default `int` and `int[]` are supported. To support any other type implement your type serializer as follows:
```cs
public class MyTypePropertySerializer : IPropertySerializer
{
    // Instance field
    public static readonly MyTypePropertySerializer instance = new MyTypePropertySerializer();
    
    // Save all data into BinaryWriter here
    public void Export(object obj, BinaryWriter w)
    {
        // You can cast object to your type
        MyType array = (MyType)obj;
        
        // ...
    }

    // Construct new instance your type and read it's data from BinaryReader
    public object Import(BinaryReader r)
    {
        MyType inst = new MyType();
        
        // ...

        return inst;
    }
    
    // Define type that this serializer supports
    public Type GetTargetType() => typeof(MyType);
}
```

After defining your property, you can set and read it on any EntityData:
```cs
entityData.SetProperty(MY_PROPERTY_NAME, data);

if (entityData.HasProperty(MY_PROPERTY_NAME)){
    int id = entityData.GetProperty<int>(MY_PROPERTY_NAME);
    // ...
}
```

### Optional Interfaces
Your class can listen to updates by implementing a number of interfaces such as: `IUpdate`, `IPreUpdate`, `IPostUpdate`, `IPowerUpdate` or their multithread variants.<br>
Add this code to your extension class. `data`, `dataSize` and `shared` here represent your internal data structures:

```cs
// Single thread update call
public void Update()
{
    for (int i = 1; i < dataSize; i++)
    {
        // ...
    }
}

// Multithread thread update call
// Togther with this call you get info about current thread
// Make sure that your code here is thread safe and does not try to interact with other threads or their data.
public void UpdateMultithread(int usedThreadCount, int currentThreadIdx, int minimumCount)
{
    // Use this method to split your data array into chunks to be processed. 
    if (WorkerThreadExecutor.CalculateMissionIndex(1, dataSize - 1, usedThreadCount, currentThreadIdx, minimumCount, out int start, out int end))
    {
        for (int i = start; i < end; i++)
        {
            // If you are accessing shadered data make sure to lock it before using.
            lock (shared)
            {
                // ...
            }
        }
    }
}
```


Your class can listen to entity construction or destruction by implementing `IComponentStateListener` interface.
Add this code to your extension class:

```cs
// Called when a new entity has been added. 
// If your system needs to react to that, you can do that here
public void OnLogicComponentsAdd(int entityId, PrefabDesc desc, int prebuildId)
{
    // You can use this Extension method to access and store data in PrefabDesc
    int typeId = desc.GetProperty<int>(ComponentDesc.FIELD_NAME);

    // ...
}

// Called after a new entity has been added. 
// If your system needs to react to that, you can do that here
public void OnPostlogicComponentsAdd(int entityId, PrefabDesc desc, int prebuildId)
{
    // ...
}

// Called when a entity has been dismantled
// If your system needs to react to that, you can do that here
public void OnLogicComponentsRemove(int id)
{
    // ...
}
```
