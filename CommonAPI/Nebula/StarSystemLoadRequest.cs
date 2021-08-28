using NebulaAPI;

namespace CommonAPI
{
    public class StarSystemLoadRequest
    {
        public int starIndex { get; set; }

        public StarSystemLoadRequest() { }
        public StarSystemLoadRequest(int starIndex)
        {
            this.starIndex = starIndex;
        }
    }
    
    [RegisterPacketProcessor]
    public class StarSystemLoadRequestProcessor : BasePacketProcessor<StarSystemLoadRequest>
    {
        public override void ProcessPacket(StarSystemLoadRequest packet, INebulaConnection conn)
        {
            if (IsClient) return;

            StarData star = GameMain.galaxy.StarById(packet.starIndex + 1);
            StarSystemManager.InitNewStar(star);

            using IWriterProvider p = NebulaModAPI.GetBinaryWriter();
            
            for (int i = 1; i < StarSystemManager.registry.data.Count; i++)
            {
                StarSystemStorage system = StarSystemManager.systems[i];
                system.GetSystem(star).Export(p.BinaryWriter);
            }
            conn.SendPacket(new StarSystemData(packet.starIndex, p.CloseAndGetBytes()));
        }
    }
}