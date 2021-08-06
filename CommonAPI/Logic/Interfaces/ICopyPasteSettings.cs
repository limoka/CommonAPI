using System.IO;

namespace CommonAPI
{
    public interface ICopyPasteSettings
    {
        void WriteCopyData(BinaryWriter w);
        bool CanPasteSettings(FactoryComponent originalObject, BinaryReader r);
        void PasteSettings(int sourceType, BinaryReader r);
        string GetCopyMessage();
        string GetPasteMessage();
    }
}