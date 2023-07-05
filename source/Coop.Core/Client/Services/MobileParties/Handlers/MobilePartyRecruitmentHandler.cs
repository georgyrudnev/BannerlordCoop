﻿using Common.Messaging;
using Common.Network;
using Coop.Core.Client.Services.MobileParties.Messages;
using Coop.Core.Server.Services.MobileParties.Messages;
using Coop.Core.Server.Services.MobileParties.Packets;
using GameInterface.Services.MobileParties.Messages;
using GameInterface.Services.MobileParties.Messages.Behavior;
using System;
using System.Collections.Generic;
using System.Text;

namespace Coop.Core.Client.Services.MobileParties.Handlers
{
    public class MobilePartyRecruitmentHandler : IHandler
    {
        private readonly IMessageBroker messageBroker;
        private readonly INetwork network;

        public MobilePartyRecruitmentHandler(IMessageBroker messageBroker, INetwork network)
        {
            this.messageBroker = messageBroker;
            this.network = network;
            messageBroker.Subscribe<OnUnitRecruited>(Handle);
            messageBroker.Subscribe<NetworkUnitRecruited>(Handle);
        }

        public void Dispose()
        {
            messageBroker.Unsubscribe<OnUnitRecruited>(Handle);
            messageBroker.Unsubscribe<NetworkUnitRecruited>(Handle);
        }

        private void Handle(MessagePayload<NetworkUnitRecruited> obj)
        {
            var payload = obj.What;

            messageBroker.Publish(this, new UnitRecruitGranted(payload.CharacterId, payload.Amount, payload.PartyId));
        }

        internal void Handle(MessagePayload<OnUnitRecruited> obj)
        {
            var payload = obj.What;

            network.SendAll(new NetworkRecruitRequest(payload.CharacterId, payload.Amount, payload.PartyId));
        }
    }
}
