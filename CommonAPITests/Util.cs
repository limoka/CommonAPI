using System;
using System.IO;

namespace CommonAPITests
{
    public static class Util
    {
        public static void GetSerializationSetup(Action<BinaryWriter> write, Action<BinaryReader> read)
        {
            MemoryStream stream = new();
            BinaryWriter writer = new(stream);

            write(writer);
            
            stream.Position = 0;
            BinaryReader reader = new(stream);

            read(reader);
        }
    }
}