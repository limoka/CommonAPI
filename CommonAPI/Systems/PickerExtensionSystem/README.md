# Picker Extension System
Picker Extension system is a submodule that allows to extend behavior of Item and Recipe picker UI. The extension allows to filter which items/recipes will be shown to the player.

### Usage example:
Show only buildings to which Assembler Mk.I can be upgraded
```cs
int itemId = 2303; //Assembling machine Mk.I
Vector2 pos =  new Vector2(-300, 238); //Roughly center of the screen
UIItemPickerExtension.Popup(pos, proto =>
{
    // Do something
}, proto => proto.Upgrades.Contains(itemId));
```

Show only recipes which can be crafted by hand
```cs
Vector2 pos =  new Vector2(-300, 238); //Roughly center of the screen
UIRecipePickerExtension.Popup(pos, proto =>
{
    // Do something
}, proto => proto.Handcraft);
```

There is also an advanced usage for ItemPickerExtension which allows to define a class which acts like addon to UIItemPicker. To do so implement `IItemPickerExtension` interface. Contact me if you want an example.
