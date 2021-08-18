using System;
using System.Collections.Generic;
using System.Linq;
using PowerNetworkStructures;
using UnityEngine;

namespace CommonAPI
{
    public class PowerNetworkHandler : NetworkHandler
    {
        private PowerSystem system;

        public override NetworkHandler Prepare(PlanetFactory currentFactory)
        {
            if (factory == currentFactory) return this;
            
            base.Prepare(currentFactory);
            system = currentFactory.powerSystem;
            return this;
        }

        protected override void SetNodeNetwork(Node node, INetwork network)
        {
            system.nodePool[node.id].networkId = network.GetId();
        }

        public override INetwork GetNetwork(int nodeId)
        {
            int netId = system.nodePool[nodeId].networkId;
            if (netId == 0) throw new ArgumentException("Zero Network id");

            return new PowerNetworkWrapper(system.netPool[netId]);
        }

        protected override void RemoveNetwork(INetwork network)
        {
            system.RemoveNetwork(network.GetId());
        }

        protected override bool CheckConnectionConditions(Node first, Node second, ref string message)
        {
            float connDistance = first.connDistance2;
            float connDistance2 = second.connDistance2;
            if (connDistance < connDistance2)
            {
                connDistance = connDistance2;
            }
            
            float dot = (first.GetPoint() - second.GetPoint()).sqrMagnitude;
            message = "距离太远";
            return dot <= connDistance;
        }

        public override string GetComponentType()
        {
            return CommonAPIPlugin.GUID + ":PowerNetworkHandler";
        }

        public override bool IsRelatedTo(ItemProto proto)
        {
            return proto.prefabDesc.isPowerNode;
        }

        public override int GetNodeId(EntityData entity, Vector3 pos, Func<Node, bool> filter = null)
        {
            if (filter == null || filter(GetNodeWithId(entity.powerNodeId)))
            {
                return entity.powerNodeId;
            }

            return 0;
        }
        
        public override NodeBounds GetNodeBounds(PrefabDesc prefab, int nodeId)
        {
            NodeBounds bounds = new NodeBounds
            {
                center = prefab.selectCenter, 
                size = prefab.selectSize,
                nodePoint = prefab.powerPoint
            };
            
            return bounds;
        }

        protected override void HandleNetworkMerge(INetwork first, INetwork second)
        {
            PowerNetwork firstNetwork = ((PowerNetworkWrapper) first).data;
            PowerNetwork secondNetwork = ((PowerNetworkWrapper) second).data;
            
            foreach (int id in secondNetwork.consumers)
            {
                system.consumerPool[id].networkId = first.GetId();
            }

            foreach (int id in secondNetwork.generators)
            {
                system.genPool[id].networkId = first.GetId();
            }

            foreach (int id in secondNetwork.accumulators)
            {
                system.accPool[id].networkId = first.GetId();
            }

            foreach (int id in secondNetwork.exchangers)
            {
                system.excPool[id].networkId = first.GetId();
            }
            
            Algorithms.ListSortedMerge(firstNetwork.consumers, secondNetwork.consumers);
            Algorithms.ListSortedMerge(firstNetwork.generators, secondNetwork.generators);
            Algorithms.ListSortedMerge(firstNetwork.accumulators, secondNetwork.accumulators);
            Algorithms.ListSortedMerge(firstNetwork.exchangers, secondNetwork.exchangers);
            
        }

        protected override void HandleNodeRemoval(INetwork network, Node node)
        {
            PowerNetwork testNetwork = ((PowerNetworkWrapper) network).data;

            foreach (int item in node.consumers)
            {
                testNetwork.consumers.Remove(item);
            }

            if (node.genId > 0)
            {
                testNetwork.generators.Remove(node.genId);
            }

            if (node.accId > 0)
            {
                testNetwork.accumulators.Remove(node.accId);
            }

            if (node.excId > 0)
            {
                testNetwork.exchangers.Remove(node.excId);
            }
        }


        protected override INetwork CreateNewNetworkWith(List<Node> nodes)
        {
            int newNetId = system.NewNetwork();
            PowerNetwork newNetwork = system.netPool[newNetId];
            Algorithms.ListSortedMerge(newNetwork.nodes, nodes);
            foreach (Node node in nodes)
            {
                system.nodePool[node.id].networkId = newNetId;
                Algorithms.ListSortedMerge(newNetwork.consumers, node.consumers);
                foreach (int num4 in node.consumers)
                {
                    system.consumerPool[num4].networkId = newNetId;
                }

                if (node.genId > 0)
                {
                    Algorithms.ListSortedAdd(newNetwork.generators, node.genId);
                    system.genPool[node.genId].networkId = newNetId;
                }

                if (node.accId > 0)
                {
                    Algorithms.ListSortedAdd(newNetwork.accumulators, node.accId);
                    system.accPool[node.accId].networkId = newNetId;
                }

                if (node.excId > 0)
                {
                    Algorithms.ListSortedAdd(newNetwork.exchangers, node.excId);
                    system.excPool[node.excId].networkId = newNetId;
                }
            }

            return new PowerNetworkWrapper(newNetwork);
        }

        public override void UpdateVisualComponents(Node target)
        {
            system.line_arragement_for_remove_node(target);
            system.line_arragement_for_add_node(target);
            factory.planet.factoryModel.RefreshPowerNodes();
        }
    }
}