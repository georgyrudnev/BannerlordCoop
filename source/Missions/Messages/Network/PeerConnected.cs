﻿using LiteNetLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Missions.Messages.Network
{
    public readonly struct PeerConnected
    {
        public NetPeer Peer { get; }

        public PeerConnected(NetPeer peer)
        {
            Peer = peer;
        }
    }
}