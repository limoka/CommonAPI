using System;
using System.Collections.Generic;
using System.Linq;

namespace CommonAPI
{
    public class NetworksSystem : BaseSubmodule
    {
        public static List<NetworkHandler> handlers = new List<NetworkHandler>();

        internal static NetworksSystem Instance => CommonAPIPlugin.GetModuleInstance<NetworksSystem>();
        
        
        internal override void Load()
        {
            AddHandler(new PowerNetworkHandler());
        }
        
        public static void AddHandler(NetworkHandler handler)
        {
            handlers.Add(handler);
        }

        public static bool IsConnectedToNetwork(PlanetFactory factory, int objId)
        {
            Instance.ThrowIfNotLoaded();
            try
            {
                NetworkHandler handler = GetNetworkHandler(factory, objId);
                return handler != null;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        public static bool IsConnectedToSameNetwork(PlanetFactory factory, int firstId, int secondId)
        {
            Instance.ThrowIfNotLoaded();
            try
            {
                NetworkHandler handler = GetCommonNetwork(factory, firstId, secondId);
                return handler != null;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        public static NetworkHandler GetNetworkHandler(PlanetFactory factory, int objId)
        {
            Instance.ThrowIfNotLoaded();
            if (objId == 0) return null;

            int protoId = objId > 0 ? factory.entityPool[objId].protoId : factory.prebuildPool[-objId].protoId;
            ItemProto itemProto = LDB.items.Select(protoId);

            if (itemProto == null) return null;
            try
            {
                return handlers.First(handler => handler.IsRelatedTo(itemProto)).Prepare(factory);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        public static NetworkHandler GetCommonNetwork(PlanetFactory factory, int firstId, int secondId)
        {
            Instance.ThrowIfNotLoaded();
            if (firstId == 0 || secondId == 0) return null;

            int firstProtoId = firstId > 0 ? factory.entityPool[firstId].protoId : factory.prebuildPool[-firstId].protoId;
            int secondProtoId = secondId > 0 ? factory.entityPool[secondId].protoId : factory.prebuildPool[-secondId].protoId;
            ItemProto firstItemProto = LDB.items.Select(firstProtoId);
            ItemProto secondItemProto = LDB.items.Select(secondProtoId);

            if (firstItemProto == null || secondItemProto == null) return null;

            try
            {
                return handlers.First(handler => handler.IsRelatedTo(firstItemProto) && handler.IsRelatedTo(secondItemProto)).Prepare(factory);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }
    }
}