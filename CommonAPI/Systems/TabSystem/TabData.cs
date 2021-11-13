using JetBrains.Annotations;

namespace CommonAPI.Systems
{
    [UsedImplicitly]
    public class TabData
    {
        public string tabName;
        public string tabIconPath;
        
        public TabData(string tabName, string tabIconPath)
        {
            this.tabName = tabName;
            this.tabIconPath = tabIconPath;
        }
    }
}