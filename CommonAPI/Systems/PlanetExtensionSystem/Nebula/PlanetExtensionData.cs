using CommonAPI.Systems;
using NebulaAPI;
using NebulaAPI.Networking;
using NebulaAPI.Packets;

namespace CommonAPI.Nebula
{
    public class PlanetExtensionData
    {
        public int planetId { get; set; }
        public byte[] binaryData { get; set; }

        public PlanetExtensionData() { }
        public PlanetExtensionData(int id, byte[] data)
        {
            planetId = id;
            binaryData = data;
        }
    }
    
    [RegisterPacketProcessor]
    public class PlanetExtensionDataProcessor : BasePacketProcessor<PlanetExtensionData>
    {
        public override void ProcessPacket(PlanetExtensionData packet, INebulaConnection conn)
        {
            if (IsHost) return;

            PlanetExtensionSystem.pendingData.Add(packet.planetId, packet.binaryData);
        }
    }
}