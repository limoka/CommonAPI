using PowerNetworkStructures;
using UnityEngine;

namespace CommonAPI
{
    public static class NodeExtensions
    {
        public static Vector3 GetPoint(this Node node)
        {
            return new Vector3(node.x, node.y, node.z);
        }
    }
}