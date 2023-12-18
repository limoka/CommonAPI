### v1.6.1
- Fix errors when adding custom protos
- Fix warnings about Common API not being built for 0.10.28.20729

### v1.6.0
- Updated to work with game version 0.10.28.20729 or higher
- Added Localization Module to support custom localizations.
- ProtoRegistry methods related to StringProto are deprecated in favor of Localization Module
- Internal restructure of the submodule system

<details>
<summary>Changelog</summary>

### v1.5.7
- Fix modded items not appearing on the production graphs.
### v1.5.6
- Remove game exe name targeting
### v1.5.5
- Update for Dyson Sphere Program update
### v1.5.4
- Fixed errors when loading a save with one or more mods that add buildings (For example Better Machines) removed
### v1.5.3
- Fix errors when model index is much bigger than maximum used by game.
### v1.5.2
- Added UINumberPickerExtension for picking signal together with a value.
### v1.5.1
- Testing release, no changes
### v1.5.0
- Updated to work with game version 0.9.25.11985 or higher
### v1.4.9
- @Raptor: Prevent creation of delegates every tick using alternate logic for Pool, which should improve performance of Various Facility.
### v1.4.8
- Add checks to container export. Any mod issues should be logged and contained.
- Fix Registry exporting data of empty items
### v1.4.7
- Add public method to make other mods compatibility easier. Internal refactor.
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
</details>