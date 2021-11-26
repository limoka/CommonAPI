namespace CommonAPI
{
    public interface IItemPickerExtension
    {
        bool OnBoxMouseDown(UIItemPicker picker);
        void TestMouseIndex(UIItemPicker picker);
        void Open(UIItemPicker picker);
        void Close(UIItemPicker picker);
        void OnPopup(UIItemPicker picker);
        void PostPopup(UIItemPicker picker);
    }

    public interface ShowLocked
    {
        
    }
}