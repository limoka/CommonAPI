using System.IO;

namespace CommonAPI
{
    /// <summary>
    /// Interface for objects that can be contained an a <see cref="Pool{T}"/>
    /// </summary>
    public interface IPoolable : ISerializeState
    {
        int GetId();
        void SetId(int id);
    }
}