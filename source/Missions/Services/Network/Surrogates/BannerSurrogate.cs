﻿using Common.Serialization;
using GameInterface.Serialization;
using GameInterface.Serialization.External;
using ProtoBuf;
using TaleWorlds.Core;

namespace Missions.Services.Network.Surrogates
{
    /// <summary>
    /// Surrogate for the Banner Class
    /// </summary>

    [ProtoContract(SkipConstructor = true)]
    public class BannerSurrogate
    {
        [ProtoMember(1)]
        public byte[] data { get; }

        public BannerSurrogate(Banner obj)
        {
            if (obj == null) return;

            if (ContainerProvider.TryResolve(out IBinaryPackageFactory packageFactory) == false) return;

            IBinaryPackage package = packageFactory.GetBinaryPackage(obj);

            data = BinaryFormatterSerializer.Serialize(package);
        }

        private Banner Deserialize()
        {
            if (data == null) return default;

            if (ContainerProvider.TryResolve(out IBinaryPackageFactory packageFactory) == false) return default;

            var package = BinaryFormatterSerializer.Deserialize<BannerBinaryPackage>(data);

            return package.Unpack<Banner>(packageFactory);
        }

        public static implicit operator BannerSurrogate(Banner obj)
        {
            return new BannerSurrogate(obj);
        }

        public static implicit operator Banner(BannerSurrogate surrogate)
        {
            return surrogate.Deserialize();
        }
    }
}