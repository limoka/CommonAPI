using System.Collections.Generic;
using System.IO;

namespace CommonAPI
{
    /// <summary>
    /// Defines a factory system, which has one instance per planet.
    /// </summary>
    public interface IFactorySystem : ISerializeState
    {
        void Init(PlanetFactory factory);

        void OnLogicComponentsAdd(int entityId, PrefabDesc desc, int prebuildId);
        
        void OnPostlogicComponentsAdd(int entityId, PrefabDesc desc, int prebuildId);
        void OnLogicComponentsRemove(int entityId);
    }
}