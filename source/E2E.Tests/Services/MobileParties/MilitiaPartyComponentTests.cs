﻿using E2E.Tests.Environment;
using E2E.Tests.Util;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Settlements;
using Xunit.Abstractions;

namespace E2E.Tests.Services.MobileParties;
public class MilitiaPartyComponentTests : IDisposable
{
    E2ETestEnvironment TestEnvironement { get; }
    public MilitiaPartyComponentTests(ITestOutputHelper output)
    {
        TestEnvironement = new E2ETestEnvironment(output);
    }

    public void Dispose()
    {
        TestEnvironement.Dispose();
    }

    [Fact]
    public void ServerCreateParty_SyncAllClients()
    {
        // Arrange
        var server = TestEnvironement.Server;

        var settlement = GameObjectCreator.CreateInitializedObject<Settlement>();

        // Act
        string? partyId = null;

        server.Call(() =>
        {
            var newParty = MilitiaPartyComponent.CreateMilitiaParty("TestId", settlement);
            partyId = newParty.StringId;
        });


        // Assert
        Assert.NotNull(partyId);

        foreach (var client in TestEnvironement.Clients)
        {
            Assert.True(client.ObjectManager.TryGetObject<MobileParty>(partyId, out var newParty));
            Assert.NotNull(newParty.PartyComponent);
        }
    }

    [Fact]
    public void ClientCreateParty_DoesNothing()
    {
        // Arrange
        var server = TestEnvironement.Server;
        var client1 = TestEnvironement.Clients.First();

        var settlement = GameObjectCreator.CreateInitializedObject<Settlement>();

        // Act
        string partyId = "TestId";
        client1.Call(() =>
        {
            MilitiaPartyComponent.CreateMilitiaParty("TestId", settlement);
        });

        // Assert
        foreach (var client in TestEnvironement.Clients)
        {
            Assert.False(client.ObjectManager.TryGetObject<MobileParty>(partyId, out var _));
        }
    }
}
