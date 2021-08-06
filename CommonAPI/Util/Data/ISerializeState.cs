using System.IO;

namespace CommonAPI
{
    public interface ISerializeState
    {
        void Free();

        void Export(BinaryWriter w);

        void Import(BinaryReader r);
    }
}