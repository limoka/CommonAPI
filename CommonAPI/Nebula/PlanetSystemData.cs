using NebulaAPI;

namespace CommonAPI
{
    public class PlanetSystemData
    {
        public int planetId { get; set; }
        public byte[] binaryData { get; set; }

        public PlanetSystemData() { }
        public PlanetSystemData(int id, byte[] data)
        {
            planetId = id;
            binaryData = data;
        }
    }
    
    [RegisterPacketProcessor]
    public class PlanetSystemDataProcessor : BasePacketProcessor<PlanetSystemData>
    {
        public override void ProcessPacket(PlanetSystemData packet, INebulaConnection conn)
        {
            if (IsHost) return;

            PlanetSystemManager.pendingData.Add(packet.planetId, packet.binaryData);
        }
    }
}