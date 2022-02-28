# Component Extension module

ComponentExtension is a module that makes adding new machine components easy. It also handles adding UI to these machines.

## Usage

First make sure that you have requested this module to be loaded as shown [here](https://github.com/kremnev8/CommonAPI#usage-example)
Then make your component class as follows
```cs
public class ComponentName : FactoryComponent
{
    // Do not modify this code. This is template to help with dynamic ID assignment
    public static readonly string componentID = $"{MyMod.MODID}:ComponentName";
    private static int _cachedId;
    public static int cachedId
    {
        get
        {
            if (_cachedId == 0)
                _cachedId = ComponentExtension.componentRegistry.GetUniqueId(componentID);
            
            return _cachedId;
        }
    }

    // This is where you get your updates. power is a value from 0 to 1 of how much power machine recieved.
    // The value you return here is then passed to `UpdateAnimation`
    public override int InternalUpdate(float power, PlanetFactory factory)
    {
        if (power < 0.1f) return 0;
        
        return 1;
    }
    
    // Here you can update machine animations. You get the update result from update method, and power state.
    // This code just loops simple animation.
    public override void UpdateAnimation(ref AnimData data, int updateResult, float power)
    {
        float num = data.prepare_length + data.working_length - 0.001f;

        data.time += ComponentTypePool.DT * power * 0.5f;
        
        if (data.time > num) {
            data.time = data.prepare_length;
        }
        
    }
    
    // Determine if machine should consume full power, other wise idle.
    public override void UpdatePowerState(ref PowerConsumerComponent component)
    {
        component.SetRequiredEnergy(true);
    }
}
```
You can see other methods you can use [here](https://github.com/kremnev8/CommonAPI/blob/master/CommonAPI/Systems/ComponentSystem/FactoryComponent.cs)
After making your class make sure to register it in your plugin code:
```cs
ComponentExtension.componentRegistry.Register(ComponentName.componentID, typeof(ComponentName));
```
Now your component is ready to be used. Attach a `ComponentDesc` class to your machine prefab and specify STRING component ID.

## Other capabilities
### Copy and Paste
You can make your machine support copy and paste by implementing [`ICopyPasteSettings`](https://github.com/kremnev8/CommonAPI/blob/master/CommonAPI/Systems/ComponentSystem/ICopyPasteSettings.cs). Then your component data can be copied and pasted using < and >. This should also help with blueprints.

### Custom Machine UI
When using ComponentExtension system you might also need a UI for your new machine. To make adding it easy you can use [`CustomMachineWindow`](https://github.com/kremnev8/CommonAPI/blob/master/CommonAPI/Systems/ComponentSystem/UI/CustomMachineWindow.cs)

Create new machine class as follows:
```cs
public class UIMyMachineWindow : CustomMachineWindow
{
    public ComponentName myComponent;

    public override bool _OnInit()
    {
        // Init your UI here
        return true;
    }

    public override void _OnClose()
    {
       // Called when UI is closed
    }

    // This method is called when player opened another machine UI, or just opened the UI
    // When you get the call `component` field should already contain your component
    protected override void OnMachineChanged()
    {
        myComponent = (ComponentName) component;

        // Update the ui accordingly
    }


    public override void _OnUpdate()
    {
        // Called every tick
    }
    
    // Should player inventory be closed when this UI is open
    public override bool DoClosePlayerInventory()
    {
        return true;
    }

    // Should other windows get closed
    public override bool DoCloseOtherWindows()
    {
        return true;
    }

    // When player clicks on a machine this method determines whether your UI should open.
    public override bool ShouldOpen(int componentId, int protoId)
    {
        return componentId == ComponentName.cachedId;
    }
}
```

Now create a prefab in Unity Editor which contains your UI elements. Make sure your prefab root has your window class attached to it.
Last step here is to register the prefab. All you need to provide is path to UI prefab. Used class will be inferred from the prefab.
```cs
CustomMachineUISystem.RegisterWindow("assets/SignalNetworks/ui/machineui/modem-window");
```

### How to get my component data ?

To fetch your component data you will need a reference to `PlanetFactory` class:
```cs
int customId = factory.entityPool[id].customId;
int customType = factory.entityPool[id].customType;
if (customId > 0)
{
    if (customType == ComponentName.cachedId)
    {
        FactoryComponent component = ComponentExtension.GetComponent(factory, customType, customId);
        // do stuff
    }
}
```
Here `customType` is ID of your component TYPE. `customId` is your component type INSTANCE ID (Like `assemblerId`).


