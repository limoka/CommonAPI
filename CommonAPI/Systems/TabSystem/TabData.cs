using JetBrains.Annotations;

namespace CommonAPI.Systems
{
    [UsedImplicitly]
    public class TabData
    {
        public int tabIndex;
        public readonly string tabName;
        public readonly string tabIconPath;
        
        public TabData(string tabName, string tabIconPath)
        {
            this.tabName = tabName;
            this.tabIconPath = tabIconPath;
        }
    }
}