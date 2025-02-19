﻿using NLog;
using ProtoBuf;
using Sandbox;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuantumHangar.HangarMarket
{
    public class ClientCommunication
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public const ushort NetworkId = 2934;

        public ClientCommunication()
        {
            MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(NetworkId, ClientMessageReceived);
        }

        private void ClientMessageReceived(ushort arg1, byte[] arg2, ulong arg3, bool arg4)
        {
            try
            {
                var receivedMessage = MyAPIGateway.Utilities.SerializeFromBinary<Message>(arg2);
                switch (receivedMessage.Type)
                {
                    case Message.MessageType.MarketOffersUpdate:
                        ReplyAllOffers(arg3);
                        break;


                    case Message.MessageType.GridDefinitionPreview:
                        SetGridPreview(receivedMessage.Definition);
                        break;


                    case Message.MessageType.BuySelectedGrid:
                        PurchaseGrid(receivedMessage.BuyRequest, arg3);
                        break;


                    default:
                        Log.Warn($"Unkown message type! Is this mod up to date?");
                        return;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Exception occured on server message deserialization! {ex.ToString()}");
            }
        }


        private static void ReplyAllOffers(ulong steamId)
        {
            try
            {
                var message = new Message(Message.MessageType.MarketOffersUpdate)
                {
                    MarketOffers = HangarMarketController.MarketOffers.Values.ToList()
                };
                //Message.MarketOffers.AddRange(Hangar.Config.PublicMarketOffers);


                MyAPIGateway.Multiplayer.SendMessageTo(NetworkId, MyAPIGateway.Utilities.SerializeToBinary(message),
                    steamId);
                //Log.Warn("Sending all market offers back to client!");
            }
            catch (Exception ex)
            {
                Log.Error($"Exception occured on server message serialization! {ex}");
            }
        }

        public void UpdateAllOffers()
        {
            try
            {
                if (!Hangar.ServerRunning || !MySession.Static.Ready || MySandboxGame.IsPaused)
                    return;

                var message = new Message(Message.MessageType.MarketOffersUpdate)
                {
                    MarketOffers = HangarMarketController.MarketOffers.Values.ToList()
                };


                //Message.MarketOffers.AddRange(Hangar.Config.PublicMarketOffers);
                message.MarketOffers ??= new List<MarketListing>();
                //Log.Fatal("Market offers is null");
                MyAPIGateway.Multiplayer.SendMessageToOthers(NetworkId,
                    MyAPIGateway.Utilities.SerializeToBinary(message));
                //Log.Warn("Sending all market offers back to client!");
            }
            catch (Exception ex)
            {
                Log.Error($"Exception occured on server message serialization! {ex}");
            }
        }


        private void SetGridPreview(GridDefinition definition)
        {
            //Need to have a cooldown on this shit
            Log.Warn($"Client requested to set grid preview of {definition.GridName}!");


            //Get grid.
            HangarMarketController.SetGridPreview(definition.ProjectorEntityId, definition.OwnerSteamId,
                definition.GridName);
        }

        private static void PurchaseGrid(BuyGridRequest offer, ulong buyerSteamId)
        {
            HangarMarketController.PurchaseGridOffer(buyerSteamId, offer.OwnerSteamId, offer.GridName);
        }


        public void Close()
        {
            MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(NetworkId, ClientMessageReceived);
        }
    }

    [ProtoContract]
    public class GridDefinition
    {
        //Items we send to server. (We will have server set the projection)
        [ProtoMember(1)] public string GridName;
        [ProtoMember(3)] public ulong OwnerSteamId;
        [ProtoMember(4)] public long ProjectorEntityId;


        public GridDefinition()
        {
        }
    }

    [ProtoContract]
    public class BuyGridRequest
    {
        [ProtoMember(1)] public string GridName;
        [ProtoMember(3)] public ulong OwnerSteamId;
        [ProtoMember(4)] public ulong BuyerSteamId;

        public BuyGridRequest()
        {
        }
    }

    [ProtoContract]
    public class Message
    {
        public enum MessageType
        {
            MarketOffersUpdate,
            GridDefinitionPreview,
            BuySelectedGrid
        }


        [ProtoMember(10)] public MessageType Type;


        [ProtoMember(20)] public List<MarketListing> MarketOffers;
        [ProtoMember(30)] public GridDefinition Definition;
        [ProtoMember(40)] public BuyGridRequest BuyRequest;


        public Message()
        {
        }

        public Message(MessageType type)
        {
            this.Type = type;
        }
    }
}