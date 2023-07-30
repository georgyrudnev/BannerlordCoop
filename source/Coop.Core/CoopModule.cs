﻿using Autofac;
using Common.Messaging;
using Common.Network;
using Common.PacketHandlers;
using Coop.Core.Client.Services.Heroes.Data;
using Coop.Core.Common.Configuration;
using Coop.Core.Surrogates;
using GameInterface;
using ProtoBuf.Meta;
using TaleWorlds.Library;

namespace Coop.Core
{
    internal class CoopModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            SurrogateCollection.AssignSurrogates();

            #region Network
            builder.RegisterType<NetworkConfiguration>().As<INetworkConfiguration>().OwnedByLifetimeScope();
            #endregion

            #region Communication
            builder.RegisterType<PacketManager>().As<IPacketManager>().SingleInstance();
            builder.RegisterType<EventPacketHandler>().AsSelf().SingleInstance().AutoActivate();
            builder.RegisterInstance(MessageBroker.Instance).As<IMessageBroker>().SingleInstance();
            #endregion

            builder.RegisterType<DeferredHeroRepository>().As<IDeferredHeroRepository>().InstancePerLifetimeScope();

            #region GameInterface
            builder.RegisterModule<GameInterfaceModule>();
            #endregion

            base.Load(builder);
        }
    }
}
