using CommonAPI.Systems;
using NebulaAPI;

namespace CommonAPI.Nebula
{
    public class StarExtensionData
    {
        public int starIndex { get; set; }
        public byte[] binaryData { get; set; }

        public StarExtensionData() { }
        public StarExtensionData(int starIndex, byte[] data)
        {
            this.starIndex = starIndex;
            binaryData = data;
        }
    }
    
    [RegisterPacketProcessor]
    public class StarExtensionDataProcessor : BasePacketProcessor<StarExtensionData>
    {
        public override void ProcessPacket(StarExtensionData packet, INebulaConnection conn)
        {
            if (IsHost) return;

            StarData star = GameMain.galaxy.StarById(packet.starIndex + 1);
            using IReaderProvider p = NebulaModAPI.GetBinaryReader(packet.binaryData);

            for (int i = 1; i < StarExtensionSystem.registry.data.Count; i++)
            {
                StarExtensionStorage extension = StarExtensionSystem.extensions[i];
                extension.GetSystem(star).Import(p.BinaryReader);
            }
        }
    }
}