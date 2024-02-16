﻿using GameInterface.Services.Armies.Audit;
using GameInterface.Services.MobileParties.Audit;
using GameInterface.Services.ObjectManager;
using HarmonyLib;
using Helpers;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using static TaleWorlds.Library.CommandLineFunctionality;

namespace GameInterface.Services.MobileParties.Commands;

internal static class MobilePartyDebugCommand
{
    [CommandLineArgumentFunction("info", "coop.debug.mobileparty")]
    public static string Info(List<string> args)
    {
        if(args.Count < 1)
        {
            return "Usage: coop.debug.mobileparty.info <PartyStringID>";
        }

        MobileParty mobileParty = Campaign.Current.CampaignObjectManager.Find<MobileParty>(args[0]);

        if(mobileParty == null )
        {
            return string.Format("ID: '{0}' not found", args[0]);
        }

        Hero owner = mobileParty.Owner;
        var _lastCalculatedSpeed = AccessTools.Field(typeof(MobileParty), "_lastCalculatedSpeed").GetValue(mobileParty);
        var explanations = mobileParty.SpeedExplained.GetExplanations();

        var stringBuilder = new StringBuilder();
        
        stringBuilder.AppendLine($"MobileParty info for: {owner}");
        stringBuilder.AppendLine($"StringID: {mobileParty.StringId}");
        stringBuilder.AppendLine($"Speed: {mobileParty.Speed}");
        stringBuilder.AppendLine($"DefaultInventoryCapacityModel: {mobileParty.InventoryCapacity}");
        stringBuilder.AppendLine($"Weight Carried: {mobileParty.TotalWeightCarried}");
        stringBuilder.AppendLine($"LastCalculated Speed: {_lastCalculatedSpeed}");
        stringBuilder.AppendLine($"Player Skills: ");
        foreach (SkillObject skill in Skills.All)
        {
            int skillValue = owner.GetSkillValue(skill);
            stringBuilder.AppendLine($"{skill.StringId}: {skillValue}");
        }
        stringBuilder.AppendLine($"Explanations: {explanations}");

        return stringBuilder.ToString();
    }

    // coop.debug.mobileparty.createParty lord_1_1 town_V1
    [CommandLineArgumentFunction("createParty", "coop.debug.mobileparty")]
    public static string CreateNewParty(List<string> args)
    {
        if (ModInformation.IsClient)
        {
            return "Create party is only to be called on the server";
        }

        if (args.Count != 2)
        {
            return "Usage: coop.debug.mobileParty.createParty <Hero.StringId> <Settlment.StringId>";
        }

        if (ContainerProvider.TryResolve<IObjectManager>(out var objectManager) == false)
        {
            return $"Unable to get {nameof(IObjectManager)}";
        }

        string heroStringId = args[0];
        string settlementId = args[1];

        if (objectManager.TryGetObject<Hero>(heroStringId, out var hero) == false)
        {
            return $"Unable to get {typeof(Hero)} with id: {heroStringId}";
        }

        if (objectManager.TryGetObject<Settlement>(settlementId, out var settlement) == false)
        {
            return $"Unable to get {typeof(Settlement)} with id: {settlementId}";
        }

        var newParty = MobilePartyHelper.SpawnLordParty(hero, settlement);

        return $"Created new {nameof(MobileParty)} with string id: {newParty.StringId}";
    }

    // coop.debug.mobileParty.destroyParty tbd
    [CommandLineArgumentFunction("destroyParty", "coop.debug.mobileparty")]
    public static string DestroyParty(List<string> args)
    {
        if (ModInformation.IsClient)
        {
            return "Create party is only to be called on the server";
        }

        if (args.Count != 1)
        {
            return "Usage: coop.debug.mobileParty.destroyParty <MobileParty.StringId>";
        }

        if (ContainerProvider.TryResolve<IObjectManager>(out var objectManager) == false)
        {
            return $"Unable to get {nameof(IObjectManager)}";
        }

        string partyId = args[0];

        if (objectManager.TryGetObject<MobileParty>(partyId, out var party) == false)
        {
            return $"Unable to get {typeof(MobileParty)} with id: {partyId}";
        }

        party.RemoveParty();

        return $"Destroyed {nameof(MobileParty)} with string id: {partyId}";
    }

    // coop.debug.mobileParty.audit
    [CommandLineArgumentFunction("audit", "coop.debug.mobileparty")]
    public static string AuditParties(List<string> args)
    {
        if (ContainerProvider.TryResolve<MobilePartyAuditor>(out var auditor) == false)
        {
            return $"Unable to get {nameof(MobilePartyAuditor)}";
        }

        return auditor.Audit();
    }
}
