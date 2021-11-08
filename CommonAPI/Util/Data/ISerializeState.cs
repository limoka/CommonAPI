using System.IO;

namespace CommonAPI
{
    /// <summary>
    /// Interface used to save and load class data to Binary Reader/Writer
    /// </summary>
    public interface ISerializeState
    {
        void Free();

        void Export(BinaryWriter w);

        void Import(BinaryReader r);
    }
}