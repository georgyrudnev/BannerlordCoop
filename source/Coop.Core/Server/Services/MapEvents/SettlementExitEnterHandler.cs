﻿using Common.Logging;
using Common.Messaging;
using Common.Network;
using Coop.Core.Client.Services.MapEvents.Messages;
using GameInterface.Services.MapEvents;
using LiteNetLib;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Coop.Core.Server.Services.MapEvents
{
    /// <summary>
    /// Handles changes to parties for settlement entry and exit.
    /// </summary>
    public class SettlementExitEnterHandler : IHandler
    {
        private readonly IMessageBroker messageBroker;
        private readonly INetwork network;
        private readonly ILogger Logger = LogManager.GetLogger<SettlementExitEnterHandler>();

        public SettlementExitEnterHandler(IMessageBroker messageBroker, INetwork network)
        {
            this.messageBroker = messageBroker;
            this.network = network;
            messageBroker.Subscribe<SettlementEnterRequest>(Handle);
        }

        public void Dispose()
        {
            messageBroker.Unsubscribe<SettlementEnterRequest>(Handle);
        }

        private void Handle(MessagePayload<SettlementEnterRequest> obj)
        {
            PartyEnteredSettlement partyEnteredSettlement = new PartyEnteredSettlement(obj.What.StringId, obj.What.PartyId);

            network.SendAllBut(obj.Who as NetPeer, partyEnteredSettlement);

            messageBroker.Publish(this, partyEnteredSettlement);
        }
    }
}
