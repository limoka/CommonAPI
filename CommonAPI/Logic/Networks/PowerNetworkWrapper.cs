using System.Collections.Generic;
using PowerNetworkStructures;

namespace CommonAPI
{
    public class PowerNetworkWrapper : INetwork
    {
        public PowerNetwork data;

        public PowerNetworkWrapper(PowerNetwork network)
        {
            data = network;
        }

        public int GetId()
        {
            return data.id;
        }

        public List<Node> GetNodes()
        {
            return data.nodes;
        }
    }
}