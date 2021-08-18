using System.IO;
using NebulaAPI;

namespace CommonAPI
{
    public class CustomFactorySerializer : IModData<PlanetFactory>
    {
        public void Export(PlanetFactory inst, BinaryWriter w)
        {
            for (int i = 1; i < CustomFactory.systemRegistry.data.Count; i++)
            {
                FactorySystemStorage system = CustomFactory.systems[i];
                system.GetSystem(inst).Export(w);
            }
        }

        public void Import(PlanetFactory inst, BinaryReader r)
        {
            CustomFactory.InitNewPlanet(inst.planet);
            for (int i = 1; i < CustomFactory.systemRegistry.data.Count; i++)
            {
                FactorySystemStorage system = CustomFactory.systems[i];
                system.GetSystem(inst).Import(r);
            }
        }
    }
}