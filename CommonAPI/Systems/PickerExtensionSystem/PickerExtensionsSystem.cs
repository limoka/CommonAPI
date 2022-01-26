using System;
using CommonAPI.Patches;

namespace CommonAPI.Systems
{
    [CommonAPISubmodule]
    public static class PickerExtensionsSystem
    {
        /// <summary>
        /// Return true if the submodule is loaded.
        /// </summary>
        public static bool Loaded {
            get => _loaded;
            internal set => _loaded = value;
        }

        private static bool _loaded;


        [CommonAPISubmoduleInit(Stage = InitStage.SetHooks)]
        internal static void SetHooks()
        {
            CommonAPIPlugin.harmony.PatchAll(typeof(UIItemPicker_Patch));
            CommonAPIPlugin.harmony.PatchAll(typeof(UIRecipePicker_Patch));
            CommonAPIPlugin.harmony.PatchAll(typeof(UISignalPicker_Patch));
        }

        [CommonAPISubmoduleInit(Stage = InitStage.PostLoad)]
        internal static void PostLoad()
        {
            if (ProtoRegistry.Loaded)
            {
                ProtoRegistry.RegisterString("SIGNAL-401", "Signal Information");
                ProtoRegistry.RegisterString("SIGNAL-402", "Signal Warning");
                ProtoRegistry.RegisterString("SIGNAL-403", "Signal Critical warning");
                ProtoRegistry.RegisterString("SIGNAL-404", "Signal Error");
                ProtoRegistry.RegisterString("SIGNAL-405", "Signal Settings");

                ProtoRegistry.RegisterString("SIGNAL-501", "Signal Missing power connection");
                ProtoRegistry.RegisterString("SIGNAL-502", "Signal Not Enough Power");
                ProtoRegistry.RegisterString("SIGNAL-503", "Signal Lightning");
                ProtoRegistry.RegisterString("SIGNAL-504", "Signal Set Recipe");
                ProtoRegistry.RegisterString("SIGNAL-506", "Signal Product stacking");
                ProtoRegistry.RegisterString("SIGNAL-507", "Signal Vein depleting");
                ProtoRegistry.RegisterString("SIGNAL-508", "Signal No fuel");
                ProtoRegistry.RegisterString("SIGNAL-509", "Signal Can't do");
                ProtoRegistry.RegisterString("SIGNAL-510", "Signal Missing connection");

                for (int i = 0; i < 10; i++)
                {
                    ProtoRegistry.RegisterString($"SIGNAL-60{i}", $"Signal {i}");
                }
            }
        }

        internal static void ThrowIfNotLoaded()
        {
            if (!Loaded)
            {
                throw new InvalidOperationException(
                    $"{nameof(PickerExtensionsSystem)} is not loaded. Please use [{nameof(CommonAPISubmoduleDependency)}(nameof({nameof(PickerExtensionsSystem)})]");
            }
        }
    }
}