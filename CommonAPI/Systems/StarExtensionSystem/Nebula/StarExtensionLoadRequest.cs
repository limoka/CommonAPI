using CommonAPI.Systems;
using NebulaAPI;
using NebulaAPI.Interfaces;
using NebulaAPI.Networking;
using NebulaAPI.Packets;

namespace CommonAPI.Nebula
{
    public class StarExtensionLoadRequest
    {
        public int starIndex { get; set; }

        public StarExtensionLoadRequest() { }
        public StarExtensionLoadRequest(int starIndex)
        {
            this.starIndex = starIndex;
        }
    }
    
    [RegisterPacketProcessor]
    public class StarExtensionLoadRequestProcessor : BasePacketProcessor<StarExtensionLoadRequest>
    {
        public override void ProcessPacket(StarExtensionLoadRequest packet, INebulaConnection conn)
        {
            if (IsClient) return;

            StarData star = GameMain.galaxy.StarById(packet.starIndex + 1);
            StarExtensionSystem.InitNewStar(star);

            using IWriterProvider p = NebulaModAPI.GetBinaryWriter();
            
            for (int i = 1; i < StarExtensionSystem.registry.data.Count; i++)
            {
                StarExtensionStorage extension = StarExtensionSystem.extensions[i];
                extension.GetSystem(star).Export(p.BinaryWriter);
            }
            conn.SendPacket(new StarExtensionData(packet.starIndex, p.CloseAndGetBytes()));
        }
    }
}