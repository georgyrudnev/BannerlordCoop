﻿using Common;
using Common.Extensions;
using Common.Messaging;
using Common.Util;
using GameInterface.Policies;
using GameInterface.Services.Clans.Messages;
using GameInterface.Services.GameDebug.Patches;
using HarmonyLib;
using System;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;

namespace GameInterface.Services.Clans.Patches
{
    [HarmonyPatch(typeof(DestroyClanAction), "ApplyInternal")]
    public class ClanDestroyPatch
    {
        private static readonly AllowedInstance<Clan> AllowedInstance = new AllowedInstance<Clan>();

        static bool Prefix(Clan destroyedClan, int details)
        {
            if (AllowedInstance.IsAllowed(destroyedClan)) return true;

            if (CallOriginalPolicy.IsOriginalAllowed()) return true;

            if (ModInformation.IsClient && destroyedClan != Clan.PlayerClan) return false;

            CallStackValidator.Validate(destroyedClan, AllowedInstance);

            MessageBroker.Instance.Publish(destroyedClan, new ClanDestroyed(destroyedClan.StringId, details));

            return false;
        }

        public static void RunOriginalDestroyClan(Clan clan, int details)
        {
            using (AllowedInstance)
            {
                AllowedInstance.Instance = clan;

                GameLoopRunner.RunOnMainThread(() =>
                {
                    DestroyClanAction.ApplyInternal(clan, (DestroyClanAction.DestroyClanActionDetails)details);
                }, true);
            }
        }
    }
}
