﻿using System;
using System.Reflection;
using Coop.Mod.DebugUtil;
using JetBrains.Annotations;
using NLog;
using RailgunNet.Logic;
using RailgunNet.System.Types;
using RemoteAction;
using Sync.Behaviour;
using TaleWorlds.CampaignSystem;
using TaleWorlds.ObjectSystem;

namespace Coop.Mod.Persistence.Party
{
    /// <summary>
    ///     Railgun: Mobile party implementation for clients. One instance for each mobile party
    ///     that is registered in the Railgun room.
    /// </summary>
    public class MobilePartyEntityClient : RailEntityClient<MobilePartyState>, IMovementHandler
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [NotNull] private readonly IEnvironmentClient m_Environment;
        [CanBeNull] private MobileParty m_Instance;

        public MobilePartyEntityClient([NotNull] IEnvironmentClient environment)
        {
            m_Environment = environment;
        }

        public Tick Tick => Room?.Tick ?? Tick.INVALID;

        /// <summary>
        ///     Handler to issue a move command for this party to the server.
        /// </summary>
        /// <param name="args">MovementData</param>
        /// <exception cref="ArgumentException"></exception>
        public void RequestMovement([NotNull] MovementData data)
        {
            if (data == null)
            {
                throw new ArgumentException(nameof(data));
            }

            Logger.Trace("[{tick}] Request move entity {id} to '{position}'.", Room.Tick, Id, data);
            Room.RaiseEvent<EventPartyMoveTo>(
                e =>
                {
                    e.EntityId = Id;
                    e.Movement = new MovementState
                    {
                        DefaultBehavior = data.DefaultBehaviour,
                        Position = data.TargetPosition,
                        TargetPartyIndex = data.TargetParty?.Id ?? MovementState.InvalidIndex,
                        SettlementIndex = data.TargetSettlement?.Id ?? MovementState.InvalidIndex
                    };
                });
        }

        /// <summary>
        ///     Handler to apply a received move command for this party.
        /// </summary>
        /// <exception cref="Exception"></exception>
        private void UpdateLocalMovement()
        {
            if (State.PartyId == MobilePartyState.InvalidPartyId)
            {
                throw new Exception("Invalid party id!");
            }

            MobileParty party = m_Environment.GetMobilePartyByIndex(State.PartyId);
            if (party == null) return;
            MovementData data = GetLatest();
            Logger.Trace(
                "[{tick}] Received move entity {id} ({party}) to {position}.",
                Room.Tick,
                Id,
                party,
                data);
            m_Environment.SetAuthoritative(party, data);
            Replay.ReplayRecording?.Invoke(Id, party, data);
        }

        public MovementData GetLatest()
        {
            return new MovementData
            {
                DefaultBehaviour = State.Movement.DefaultBehavior,
                TargetPosition = State.Movement.Position,
                TargetParty = State.Movement.TargetPartyIndex != MovementState.InvalidIndex
                    ? MBObjectManager.Instance.GetObject(State.Movement.TargetPartyIndex) as
                        MobileParty
                    : null,
                TargetSettlement = State.Movement.SettlementIndex != MovementState.InvalidIndex
                    ? MBObjectManager.Instance.GetObject(
                        State.Movement.SettlementIndex) as Settlement
                    : null
            };
        }

        /// <summary>
        ///     Called when the controller of this party changes.
        /// </summary>
        protected override void OnControllerChanged()
        {
            if (Controller != null)
            {
                // We control the party now.
                RegisterAsController();
            }
            else
            {
                UnregisterAsController();
            }
        }

        /// <summary>
        ///     Handler to be called when the control of this party changes to or from any player.
        /// </summary>
        private void OnPlayerControlledChanged()
        {
            m_Environment.SetIsPlayerControlled(State.PartyId, State.IsPlayerControlled);
        }

        /// <summary>
        ///     Called when this party is added to the Railgun room.
        /// </summary>
        protected override void OnAdded()
        {
            State.OnMovementChanged += UpdateLocalMovement;
            State.OnPlayerControlledChanged += OnPlayerControlledChanged;
        }

        /// <summary>
        ///     Called when this party is removed from the Railgun room.
        /// </summary>
        protected override void OnRemoved()
        {
            State.OnPlayerControlledChanged -= OnPlayerControlledChanged;
            State.OnMovementChanged -= UpdateLocalMovement;
        }

        /// <summary>
        ///     Registers handlers to intercept issued movement commands to this party and send
        ///     them to the server. Should only be called for parties that are controlled by
        ///     this client.
        /// </summary>
        /// <exception cref="Exception"></exception>
        private void RegisterAsController()
        {
            if (m_Instance == null && Controller != null)
            {
                m_Instance = m_Environment.GetMobilePartyByIndex(State.PartyId);
                if (m_Instance == null)
                {
                    throw new Exception($"Mobile party id {State.PartyId} not found.");
                }
                m_Environment.PartySync.RegisterLocalHandler(m_Instance, this);
            }
        }

        /// <summary>
        ///     Unregisters all handlers.
        /// </summary>
        private void UnregisterAsController()
        {
            m_Environment.PartySync.Unregister(this);
            m_Instance = null;
        }

        public override string ToString()
        {
            return $"Party {State.PartyId} ({Id}): {m_Instance}";
        }
    }
}
