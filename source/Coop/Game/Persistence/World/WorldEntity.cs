﻿using System;
using RailgunNet.Logic;
using RailgunNet.Logic.State;

namespace Coop.Game.Persistence.World
{
    public class WorldEntityClient : RailEntityClient<RailStateGeneric<WorldState>>
    {
        private readonly IEnvironment m_Environment;
        public WorldEntityClient(IEnvironment environment)
        {
            m_Environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        protected override void UpdateProxy()
        {
            m_Environment.TimeControlMode = State.Data.TimeControlMode;
        }
    }

    public class WorldEntityServer : RailEntityServer<RailStateGeneric<WorldState>>
    {
        private readonly IEnvironment m_Environment;
        public WorldEntityServer(IEnvironment environment)
        {
            m_Environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }
        protected override void UpdateAuthoritative()
        {
            State.Data.TimeControlMode = m_Environment.TimeControlMode;
        }
    }
}
