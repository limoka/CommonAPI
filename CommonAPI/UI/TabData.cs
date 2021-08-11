using JetBrains.Annotations;

namespace CommonAPI
{
    [UsedImplicitly]
    public class TabData
    {
        public static InstanceRegistry<TabData> tabsRegistry = new InstanceRegistry<TabData>(3);
        
        public string tabName;
        public string tabIconPath;
        
        public TabData(string tabName, string tabIconPath)
        {
            this.tabName = tabName;
            this.tabIconPath = tabIconPath;
        }
    }
}