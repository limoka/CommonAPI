# Custom KeyBind system
Custom KeyBind system allows to register new keybinds that: 
- Can be rebinded by players using options menu
- Have a short localized description
- Have conflict groups which define which keybinds can't share same keys
- Can be easily addressed in code without using static variables

### Example usage
Make sure to add `[CommonAPISubmoduleDependency(nameof(CustomKeyBindSystem))]` to your plugin attributes. This will load the submodule.

First you need to define default key user has to press. This is done using CombineKey class:
```cs
CombineKey key = new CombineKey(int _keyCode, byte _modifier, ECombineKeyAction _action, bool _noneKey)
```
Then you can call RegisterKeyBind method to register the keybind. 
```cs
CustomKeyBindSystem.RegisterKeyBind<HoldKeyBind>(new BuiltinKey
{
    key = key, // Default KeyBind
    conflictGroup = 2052, // Conflict group is a bitfield. Each bit corresponds to a key group 
    name = "ForceBPPlace", // Name of your KeyBind. 
    canOverride = true // Can player rebind this KeyBind?
});
```
When registering new KeyBind apart from parameters you also have to specify KeyBind Type. By default there are three types:
- PressKeyBind
- HoldKeyBind
- ReleasekeyBind

You can also define your own KeyBind type by creating new class extending PressKeyBind class

You also need to register the localized string for the KeyBind. To do that use ProtoRegistry system. Do note that there is `KEY` appended to the localized string. This is always the case.
```cs
ProtoRegistry.RegisterString("KEYForceBPPlace", "Force Blueprint placement", "强制蓝图放置");
```
Finally you can use your KeyBind anywhere in your code to get current state of that key
```cs
CustomKeyBindSystem.GetKeyBind("ForceBPPlace").keyValue
```
## KeyBind conflict group descriptions
As stated above `conflictGroup` is a bitfield, where each bit corresponds to a key group. No KeyBinds that share the same group can have the same Key bound to them. Vanilla game has already made use of some of these. I have listed here my assumed use for each group:
Bit(In decimal) | Use
---- | ----
1    | Player movement KeyBinds
2    | UI
4    | Build mode key #1
8    | Build mode key #2
16   | Build mode key #3
32   | Inventory keys
64   | Camera control keys #1
128  | Camera control keys #1
256  | Player Flying
512  | Player Sailing
1024 | No sure
2048 | Is this key on a keyboard
4096 | Is this key on a mouse
Other| Not Used
