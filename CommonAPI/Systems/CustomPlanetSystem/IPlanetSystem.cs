using System.Collections.Generic;
using System.IO;

namespace CommonAPI.Systems
{
    /// <summary>
    /// Defines a system, which has one instance per planet.
    /// </summary>
    public interface IPlanetSystem : ISerializeState
    {
        void Init(PlanetFactory factory);
    }
}