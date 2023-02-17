﻿using Common.Network;
using HarmonyLib;
using Missions.Services.Agents.Messages;
using Missions.Services.Network;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;

namespace Missions.Services.Arena.Patches
{
    [HarmonyPatch(typeof(Mission), "RegisterBlow")]
    /// <summary>
    /// Intercept agent damage and determine if a network call is needed
    /// </summary>
    public class AgentDamagePatch
    {
        static bool Prefix(Agent attacker, Agent victim, GameEntity realHitEntity, Blow b, ref AttackCollisionData collisionData, in MissionWeapon attackerWeapon, ref CombatLogData combatLogData)
        {
            // first, check if the attacker exists in the agent to ID groud, if not, no networking is needed (not a network agent)
            if (!NetworkAgentRegistry.Instance.AgentToId.TryGetValue(attacker, out Guid attackerId)) return true;

            // next, check if the attacker is one of ours, if not, no networking is needed (not our agent dealing damage)
            if (!NetworkAgentRegistry.Instance.ControlledAgents.ContainsKey(attackerId)) return true;

            AgentDamageData _agentDamageData;

            // get the victim GUI
            NetworkAgentRegistry.Instance.AgentToId.TryGetValue(victim, out Guid victimId);

            // construct a agent damage data
            _agentDamageData = new AgentDamageData(attackerId, victimId, collisionData, b);

            // publish the event
            NetworkMessageBroker.Instance.PublishNetworkEvent(_agentDamageData);

            return true;
        }
    }

    [HarmonyPatch(typeof(OrderController), "SimulateDestinationFrames")]
    public class OrderControllerPatch
    {
        static bool Prefix(out List<WorldPosition> simulationAgentFrames, float minDistance = 3f)
        {
            simulationAgentFrames = new List<WorldPosition>();
            return false;
        }
    }
}
