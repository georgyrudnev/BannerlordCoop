﻿using Common.Util;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem;

namespace E2E.Tests.Util.ObjectBuilders;
internal class ClanBuilder : IObjectBuilder
{
    public object Build()
    {
        var clan = new Clan();
        var defaultTemplate = ObjectHelper.SkipConstructor<PartyTemplateObject>();
        clan._defaultPartyTemplate = defaultTemplate;

        clan.StringId = Guid.NewGuid().ToString();

        return clan;
    }
}
