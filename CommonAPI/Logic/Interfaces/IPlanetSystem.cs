using System.Collections.Generic;
using System.IO;

namespace CommonAPI
{
    /// <summary>
    /// Defines a system, which has one instance per planet.
    /// </summary>
    public interface IPlanetSystem : ISerializeState
    {
        void Init(PlanetFactory factory);
    }
}