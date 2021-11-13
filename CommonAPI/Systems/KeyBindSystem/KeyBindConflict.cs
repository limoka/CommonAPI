namespace CommonAPI.Systems
{
    public class KeyBindConflict
    {
        // Conflict group bit usages by game's keybindings
        // Each bit defines a group where two keybinds cannot have same keys
        // Bits after 4096 are not used
        public const int MOVEMENT = 1;
        public const int UI = 2;
        public const int BUILD_MODE_1 = 4;
        public const int BUILD_MODE_2 = 8;
        public const int BUILD_MODE_3 = 16;
        public const int INVENTORY = 32;
        public const int CAMERA_1 = 64;
        public const int CAMERA_2 = 128;
        public const int FLYING = 256;
        public const int SAILING = 512;
        public const int EXTRA = 1024;
        
        //Defines whether keybind uses keyboard or mouse
        public const int KEYBOARD_KEYBIND = 2048;
        public const int MOUSE_KEYBIND = 4096;
    }
}