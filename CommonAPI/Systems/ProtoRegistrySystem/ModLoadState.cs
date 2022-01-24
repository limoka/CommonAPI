using System;

namespace CommonAPI.Systems
{
    public class ModLoadState : IDisposable
    {
        public string modGUID;

        public ModLoadState(string modGUID)
        {
            this.modGUID = modGUID;
        }
        
        public void Dispose()
        {
            ProtoRegistry.currentMod = "";
            CommonAPIPlugin.logger.LogInfo($"Mod {modGUID} loading phase is over.");
        }
    }
}