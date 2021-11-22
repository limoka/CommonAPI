using System;
using System.IO;

namespace CommonAPI
{
    public interface IPropertySerializer
    {
        void Export(object obj, BinaryWriter w);
        object Import(BinaryReader r);
        Type GetTargetType();
    }

}