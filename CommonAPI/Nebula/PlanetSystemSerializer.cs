using System.IO;
using NebulaAPI;

namespace CommonAPI
{
    public class PlanetSystemSerializer : IModData<PlanetFactory>
    {
        public void Export(PlanetFactory inst, BinaryWriter w)
        {
            for (int i = 1; i < PlanetSystemManager.registry.data.Count; i++)
            {
                PlanetSystemStorage system = PlanetSystemManager.systems[i];
                system.GetSystem(inst).Export(w);
            }
        }

        public void Import(PlanetFactory inst, BinaryReader r)
        {
            PlanetSystemManager.InitNewPlanet(inst.planet);
            for (int i = 1; i < PlanetSystemManager.registry.data.Count; i++)
            {
                PlanetSystemStorage system = PlanetSystemManager.systems[i];
                system.GetSystem(inst).Import(r);
            }
        }
    }
}