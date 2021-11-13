using CommonAPI.Systems;
using NebulaAPI;

namespace CommonAPI.Nebula
{
    public class PlanetSystemLoadRequest
    {
        public int planetID { get; set; }

        public PlanetSystemLoadRequest() { }
        public PlanetSystemLoadRequest(int planetID)
        {
            this.planetID = planetID;
        }
    }
    
    [RegisterPacketProcessor]
    public class PlanetSystemLoadRequestProcessor : BasePacketProcessor<PlanetSystemLoadRequest>
    {
        public override void ProcessPacket(PlanetSystemLoadRequest packet, INebulaConnection conn)
        {
            if (IsClient) return;

            PlanetData planet = GameMain.galaxy.PlanetById(packet.planetID);
            PlanetFactory factory = GameMain.data.GetOrCreateFactory(planet);

            using IWriterProvider p = NebulaModAPI.GetBinaryWriter();
            
            for (int i = 1; i < CustomPlanetSystem.registry.data.Count; i++)
            {
                PlanetSystemStorage system = CustomPlanetSystem.systems[i];
                system.GetSystem(factory).Export(p.BinaryWriter);
            }
            conn.SendPacket(new PlanetSystemData(packet.planetID, p.CloseAndGetBytes()));
        }
    }
}