﻿using Common.Serialization;
using GameInterface.Serialization;
using GameInterface.Serialization.External;
using ProtoBuf;
using TaleWorlds.Core;

namespace Missions.Services.Network.Surrogates
{
    /// <summary>
    /// Surrogate for the ItemModifier Class
    /// </summary>
    [ProtoContract(SkipConstructor = true)]
    public class ItemModifierSurrogate
    {
        [ProtoMember(1)]
        public byte[] data { get; }

        public ItemModifierSurrogate(ItemModifier obj)
        {
            if (obj == null) return;

            if (ContainerProvider.TryResolve(out IBinaryPackageFactory packageFactory) == false) return;

            IBinaryPackage package = packageFactory.GetBinaryPackage(obj);

            data = BinaryFormatterSerializer.Serialize(package);
        }

        private ItemModifier Deserialize()
        {
            if (data == null) return default;

            if (ContainerProvider.TryResolve(out IBinaryPackageFactory packageFactory) == false) return default;

            var package = BinaryFormatterSerializer.Deserialize<ItemModifierBinaryPackage>(data);

            return package.Unpack<ItemModifier>(packageFactory);
        }

        public static implicit operator ItemModifierSurrogate(ItemModifier obj)
        {
            return new ItemModifierSurrogate(obj);
        }

        public static implicit operator ItemModifier(ItemModifierSurrogate surrogate)
        {
            return surrogate.Deserialize();
        }
    }
}