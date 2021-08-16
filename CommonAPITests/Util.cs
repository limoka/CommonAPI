using System;
using System.IO;

namespace CommonAPITests
{
    public static class Util
    {
        public static void GetSerializationSetup(Action<BinaryWriter> write, Action<BinaryReader> read)
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);

            write(writer);
            
            stream.Position = 0;
            BinaryReader reader = new BinaryReader(stream);

            read(reader);
        }
    }
}