using System.Collections.Generic;
using System.IO;

namespace CommonAPI.Systems
{
    /// <summary>
    /// Defines a extension, which has one instance per planet.
    /// </summary>
    public interface IPlanetExtension : ISerializeState
    {
        void Init(PlanetFactory factory);
    }
}