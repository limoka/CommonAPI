namespace CommonAPI
{
        
    public interface IItem
    {
        int GetItemId();
        int GetCount();
        int GetMaxStackSize();
        
    }
    
    public interface IStorage
    {
        int size { get; }
        bool changed { get; set; }

        IItem GetAt(int index);
        void SetAt(int index, IItem stack);
    }
}