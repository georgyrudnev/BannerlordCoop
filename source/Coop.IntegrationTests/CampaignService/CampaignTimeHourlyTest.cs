﻿using Coop.IntegrationTests.Environment;
using GameInterface.Services.Settlements.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem;
using GameInterface.Services.CampaignService.Messages;
using Coop.Core.Server.Services.Settlements.Messages;
using Coop.IntegrationTests.Environment.Instance;
using Coop.Core.Server.Services.CampaignServices.Messages;

namespace Coop.IntegrationTests.CampaignService;
public class CampaignTimeHourlyTest
{
    internal TestEnvironment TestEnvironment { get; } = new TestEnvironment();
    [Fact]
    public void CampaignTimeHourlyTest_Publishes_Server_ToClients()
    {
        long numTicks = 30101L;
        long deltaTime = 13211L;

        var triggerMessage = new CampaignTimeChanged(numTicks, deltaTime);

        var server = TestEnvironment.Server;

        // Act
        server.SimulateMessage(this, triggerMessage);

        // Assert
        // Verify the server sends a single message to it's game interface
        Assert.Equal(1, server.NetworkSentMessages.GetMessageCount<NetworkCampaignTimeChanged>());

        // Verify the all clients send a single message to their game interfaces
        foreach (EnvironmentInstance client in TestEnvironment.Clients)
        {
            Assert.Equal(1, client.InternalMessages.GetMessageCount<ChangeCampaignTime>());
        }
    }
}
