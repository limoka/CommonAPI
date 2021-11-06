using System;
using System.Collections.Generic;
using System.Linq;
using PowerNetworkStructures;
using UnityEngine;

namespace CommonAPI
{
    public struct NodeBounds
    {
        public Vector3 center;
        public Vector3 size;
        public Vector3 nodePoint;
    }
    
    public abstract class NetworkHandler
    {
        protected static List<int> _tmp_ints = new List<int>();
        protected static List<Node> _tmp_nodes = new List<Node>();
        
        protected PlanetFactory factory;
        
        public virtual NetworkHandler Prepare(PlanetFactory currentFactory)
        {
            factory = currentFactory;
            return this;
        }

        public abstract INetwork GetNetwork(int nodeId);
        protected abstract void RemoveNetwork(INetwork network);
        protected abstract void HandleNetworkMerge(INetwork first, INetwork second);
        protected abstract void SetNodeNetwork(Node node, INetwork network);
        protected abstract void HandleNodeRemoval(INetwork network, Node node);
        protected abstract INetwork CreateNewNetworkWith(List<Node> nodes);
        public abstract void UpdateVisualComponents(Node target);
        protected abstract bool CheckConnectionConditions(Node first, Node second, ref string errorMessage);
         
        public abstract string GetComponentType();

        public abstract bool IsRelatedTo(ItemProto proto);

        public abstract int GetNodeId(EntityData entity, Vector3 pos, Func<Node, bool> filter = null);
        public abstract NodeBounds GetNodeBounds(PrefabDesc prefab, int nodeId);
        
        public bool DisconnectAll(int nodeId)
        {
            INetwork network = GetNetwork(nodeId);
            Node targetNode = GetNodeWithId(nodeId);

            _tmp_nodes.Clear();
            foreach (Node conn in targetNode.conns)
            {
                _tmp_nodes.Add(conn);
                conn.conns.Remove(targetNode);
            }

            foreach (Node node in _tmp_nodes)
            {
                targetNode.conns.Remove(node);
            }

            _tmp_nodes.Clear();

            CreateNewNetworkWith(new List<Node>() {targetNode});

            CheckForDisconnectedNetworks(network);

            UpdateVisualComponents(targetNode);
            return true;
        }

        public void DisconnectNodes(Node first, Node other)
        {
            first.conns.Remove(other);
            other.conns.Remove(first);

            INetwork network = GetNetwork(first.id);
            CheckForDisconnectedNetworks(network);
        }

        public bool ConnectNodes(Node first, Node second, ref string errorMessage)
        {
            if (!CheckConnectionConditions(first, second, ref errorMessage)) return false;

            Algorithms.ListSortedAdd(first.conns, second);
            Algorithms.ListSortedAdd(second.conns, first);

            INetwork firstNet = GetNetwork(first.id);
            INetwork secondNet = GetNetwork(second.id);
            MergeNetworks(firstNet, secondNet);
            return true;
        }

        public Node GetNodeWithId(int nodeId)
        {
            INetwork network = GetNetwork(nodeId);
            try
            {
                return network.GetNodes().First(node => node.id == nodeId);
            }
            catch (InvalidOperationException)
            {
                throw new ArgumentException("Can't find network");
            }
        }

        public bool AreNodesConnected(Node firstNode, Node secondNode)
        {
            return firstNode.conns.Any(node => node.id == secondNode.id);
        }
        
        private void MergeNetworks(INetwork firstNetwork, INetwork secondNetwork)
        {
            if (firstNetwork == null || secondNetwork == null) return;
            if (firstNetwork.GetId() == secondNetwork.GetId()) return;

            foreach (Node node in secondNetwork.GetNodes())
            {
                SetNodeNetwork(node, firstNetwork);
            }
            Algorithms.ListSortedMerge(firstNetwork.GetNodes(), secondNetwork.GetNodes());
            
            HandleNetworkMerge(firstNetwork, secondNetwork);

            RemoveNetwork(secondNetwork);
        }

        public void CheckForDisconnectedNetworks(INetwork startNetwork)
        {
            INetwork testNetwork = startNetwork;
            int itterations = 0;
            do
            {
                List<Node> nodes = testNetwork.GetNodes();
                Algorithms.NodeDfs(nodes[0]);
                _tmp_nodes.Clear();
                for (int j = 0; j < nodes.Count; j++)
                {
                    Node node = nodes[j];
                    if (!node.flag)
                    {
                        _tmp_nodes.Add(node);
                        HandleNodeRemoval(testNetwork, node);

                        nodes.RemoveAt(j);
                        j--;
                    }
                }

                Algorithms.ClearNodeFlags(nodes);
                if (_tmp_nodes.Count == 0)
                {
                    break;
                }

                testNetwork = CreateNewNetworkWith(_tmp_nodes);
            } while (itterations++ < 24);

            _tmp_nodes.Clear();
        }
    }
}