﻿using Common.Logging;
using Common.Network;
using Common.PacketHandlers;
using Missions.Services.Agents.Handlers;
using Missions.Services.Agents.Messages;
using Serilog;
using System;
using TaleWorlds.MountAndBlade;

namespace Missions.Services.Network
{
    public class CoopMissionNetworkBehavior : MissionBehavior, IDisposable
    {
        private static readonly ILogger Logger = LogManager.GetLogger<CoopMissionNetworkBehavior>();

        public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;
        private readonly LiteNetP2PClient client;

        private readonly INetworkMessageBroker networkMessageBroker;
        private readonly INetworkAgentRegistry agentRegistry;

        private readonly IDisposable[] disposables;

        public CoopMissionNetworkBehavior(
            LiteNetP2PClient client,
            INetworkMessageBroker messageBroker,
            INetworkAgentRegistry agentRegistry,
            AgentMovementHandler movementHandler,
            EventPacketHandler eventPacketHandler)
        {
            this.client = client;
            networkMessageBroker = messageBroker;
            this.agentRegistry = agentRegistry;

            disposables = new IDisposable[]
            {
                client,
                movementHandler,
                eventPacketHandler
            };
        }

        ~CoopMissionNetworkBehavior() => Dispose();

        public void Dispose()
        {
            agentRegistry.Clear();

            foreach (var disposable in disposables)
            {
                disposable.Dispose();
            }
        }

        protected override void OnEndMission()
        {
            base.OnEndMission();
            MBGameManager.EndGame();
            Dispose();
        }

        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
            Dispose();
        }

        public override void OnRenderingStarted()
        {
            string sceneName = Mission.SceneName;
            client.NatPunch(sceneName);
        }

        public override void OnAgentDeleted(Agent affectedAgent)
        {
            networkMessageBroker.Publish(this, new AgentDeleted(affectedAgent));

            base.OnAgentDeleted(affectedAgent);
        }
    }
}
