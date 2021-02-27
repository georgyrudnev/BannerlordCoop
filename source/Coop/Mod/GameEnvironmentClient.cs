﻿using System;
using System.Collections.Generic;
using System.Linq;
using Coop.Mod.Patch;
using Coop.Mod.Patch.MobilePartyPatches;
using Coop.Mod.Persistence.Party;
using RemoteAction;
using Sync.Store;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace Coop.Mod
{
    internal class GameEnvironmentClient : IEnvironmentClient
    {
        public GameEnvironmentClient()
        {
            TimeSynchronization.GetAuthoritativeTime += () => AuthoritativeTime;
        }

        public HashSet<MobileParty> PlayerControlledMainParties { get; } =
            new HashSet<MobileParty>();

        public void SetAuthoritative(MobileParty party, MovementData data)
        {
            CampaignMapMovement.Sync.SetAuthoritative(party, data);
        }
        public void SetAuthoritative(MobileParty party, Vec2 position)
        {
            CampaignMapMovement.Sync.SetAuthoritative(party, position);
        }

        public CampaignTime AuthoritativeTime { get; set; } = CampaignTime.Never;

        public void SetIsPlayerControlled(MBGUID guid, bool isPlayerControlled)
        {
            MobileParty party = GetMobilePartyById(guid);

            if(party == null)
            {
                return;
            }

            if (isPlayerControlled)
            {
                PlayerControlledMainParties.Add(party);
            }
            else
            {
                PlayerControlledMainParties.Remove(party);
            }
        }

        public IEnumerable<MobileParty> PlayerMainParties => PlayerControlledMainParties;
        public MobilePartySync PartySync { get; } = CampaignMapMovement.Sync;

        public RemoteStore Store =>
            CoopClient.Instance.SyncedObjectStore ??
            throw new InvalidOperationException("Client not initialized.");

        #region Game state access
        public MobileParty GetMobilePartyById(MBGUID guid)
        {
            return MobileParty.All.SingleOrDefault(p => p.Id == guid);
        }
        #endregion
    }
}
