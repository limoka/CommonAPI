using System.Collections.Generic;
using PowerNetworkStructures;

namespace CommonAPI
{
    public interface INetwork
    {
        int GetId();

        List<Node> GetNodes();
    }
}