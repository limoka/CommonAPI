using System;
using CommonAPI.Patches;
using CommonAPI.Systems.ModLocalization;

namespace CommonAPI.Systems
{
    public class PickerExtensionsSystem : BaseSubmodule
    {
        internal static PickerExtensionsSystem Instance => CommonAPIPlugin.GetModuleInstance<PickerExtensionsSystem>();
        internal override Type[] Dependencies => new[] { typeof(LocalizationModule) };


        internal override void SetHooks()
        {
            CommonAPIPlugin.harmony.PatchAll(typeof(UIItemPicker_Patch));
            CommonAPIPlugin.harmony.PatchAll(typeof(UIRecipePicker_Patch));
            CommonAPIPlugin.harmony.PatchAll(typeof(UISignalPicker_Patch));
        }
        
        internal override void PostLoad()
        {
            if (ProtoRegistry.Instance.Loaded)
            {
                LocalizationModule.RegisterTranslation("SIGNAL-401", "Signal Information");
                LocalizationModule.RegisterTranslation("SIGNAL-402", "Signal Warning");
                LocalizationModule.RegisterTranslation("SIGNAL-403", "Signal Critical warning");
                LocalizationModule.RegisterTranslation("SIGNAL-404", "Signal Error");
                LocalizationModule.RegisterTranslation("SIGNAL-405", "Signal Settings");

                LocalizationModule.RegisterTranslation("SIGNAL-501", "Signal Missing power connection");
                LocalizationModule.RegisterTranslation("SIGNAL-502", "Signal Not Enough Power");
                LocalizationModule.RegisterTranslation("SIGNAL-503", "Signal Lightning");
                LocalizationModule.RegisterTranslation("SIGNAL-504", "Signal Set Recipe");
                LocalizationModule.RegisterTranslation("SIGNAL-506", "Signal Product stacking");
                LocalizationModule.RegisterTranslation("SIGNAL-507", "Signal Vein depleting");
                LocalizationModule.RegisterTranslation("SIGNAL-508", "Signal No fuel");
                LocalizationModule.RegisterTranslation("SIGNAL-509", "Signal Can't do");
                LocalizationModule.RegisterTranslation("SIGNAL-510", "Signal Missing connection");

                for (int i = 0; i < 10; i++)
                {
                    LocalizationModule.RegisterTranslation($"SIGNAL-60{i}", $"Signal {i}");
                }
                
                LocalizationModule.RegisterTranslation("setCountManually", "Select value");
                LocalizationModule.RegisterTranslation("CountLabel", "Value");
                LocalizationModule.RegisterTranslation("ConfirmButtonLabel", "Confirm");

                ProtoRegistry.onLoadingFinished += () =>
                {
                    for (int i = 4; i <= 6; i++)
                    {
                        for (int j = 0; j <= 10; j++)
                        {
                            SignalProto proto = LDB.signals.Select(i*100+j);
                            proto?.Preload();
                        }
                    }
                };
            }
        }
    }
}