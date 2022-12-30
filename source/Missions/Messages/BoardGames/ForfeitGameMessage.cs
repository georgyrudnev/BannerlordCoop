﻿using ProtoBuf;
using System;

namespace Missions.Messages.BoardGames
{
    [ProtoContract]
    public readonly struct ForfeitGameMessage
    {
        public ForfeitGameMessage(Guid gameId)
        {
            GameId = gameId;
        }

        [ProtoMember(1)]
        public Guid GameId { get; }
    }
}
