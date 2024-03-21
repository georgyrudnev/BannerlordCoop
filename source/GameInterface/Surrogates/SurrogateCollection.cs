﻿using ProtoBuf.Meta;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace GameInterface.Surrogates;

/// <summary>
/// Collection of ProtoBuf surrogates
/// </summary>
public interface ISurrogateCollection { }

/// <inheritdoc cref="ISurrogateCollection"/>
internal class SurrogateCollection : ISurrogateCollection
{
    public SurrogateCollection()
    {
        RuntimeTypeModel.Default.Add(typeof(Vec2), false).SetSurrogate(typeof(Vec2Surrogate));
        RuntimeTypeModel.Default.Add(typeof(Vec3), false).SetSurrogate(typeof(Vec3Surrogate));
        RuntimeTypeModel.Default.Add(typeof(Army), false).SetSurrogate(typeof(ArmySurrogate));
        RuntimeTypeModel.Default.Add(typeof(PartyBase), false).SetSurrogate(typeof(PartyBaseSurrogate));
        RuntimeTypeModel.Default.Add(typeof(TextObject), false).SetSurrogate(typeof(TextObjectSurrogate));
        RuntimeTypeModel.Default.Add(typeof(CultureObject), false).SetSurrogate(typeof(BasicCultureObjectSurrogate));
        RuntimeTypeModel.Default.Add(typeof(ItemCategory), false).SetSurrogate(typeof(ItemCategorySurrogate));

        RuntimeTypeModel.Default.Add(typeof(ItemComponent), false).SetSurrogate(typeof(ItemComponentSurrogate));
        RuntimeTypeModel.Default.Add(typeof(ItemModifierGroup), false).SetSurrogate(typeof(ItemModifierGroupSurrogate));
        RuntimeTypeModel.Default.Add(typeof(WeaponDesign), false).SetSurrogate(typeof(WeaponDesignSurrogate));
    }
}