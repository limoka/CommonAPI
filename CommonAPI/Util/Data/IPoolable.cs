using System.IO;

namespace CommonAPI
{
    public interface IPoolable : ISerializeState
    {
        int GetId();
        void SetId(int id);
    }
}