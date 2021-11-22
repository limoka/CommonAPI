using HarmonyLib;

namespace CommonAPI
{
    /// <summary>
    /// Allows to load directly into a save once the game has loaded. <br/>
    /// To use add <c>loadSave &lt;Save File Name&gt;</c> argument when launching the game
    /// </summary>
    [HarmonyPatch]
    public static class LoadSaveOnLoad
    {
        internal static string saveName;
        internal static bool isValid => saveName != null && !saveName.Equals("");
        
        internal static string GetArg(string name)
        {
            var args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == name && args.Length > i + 1)
                {
                    return args[i + 1];
                }
            }
            return null;
        }

        internal static void Init()
        {
            saveName = GetArg("loadSave");
            if (isValid)
            {
                DSPGame.LoadFile = saveName;
                CommonAPIPlugin.logger.LogInfo($"Loading save {saveName} by default!");
            }
        }
        
        internal static void LoadSave()
        {
            if (!isValid || !GameSave.SaveExist(saveName)) return;
            
            DSPGame.StartGame(saveName);
        }
    }
}