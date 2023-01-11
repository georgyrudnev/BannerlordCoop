﻿using Common.Logging;
using Common.Messaging;
using HarmonyLib;
using IntroServer.Config;
using Missions.Services.Network;
using Missions.Services.Network.Surrogates;
using ProtoBuf.Meta;
using SandBox;
using SandBox.Missions.MissionLogics;
using SandBox.Missions.MissionLogics.Arena;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Source.Missions;
using TaleWorlds.MountAndBlade.Source.Missions.Handlers;
using TaleWorlds.SaveSystem.Load;

namespace Missions
{
    public class ArenaTestGameManager : SandBoxGameManager, IMissionGameManager
    {
        static ArenaTestGameManager()
        {
            RuntimeTypeModel.Default.SetSurrogate<Vec3, Vec3Surrogate>();
            RuntimeTypeModel.Default.SetSurrogate<Vec2, Vec2Surrogate>();
        }

        private static readonly ILogger Logger = LogManager.GetLogger<ArenaTestGameManager>();
        private readonly Harmony harmony = new Harmony("Coop.MissonTestMod");
        private LiteNetP2PClient m_Client;
        private bool missionLoaded;

        public ArenaTestGameManager(LoadResult loadedGameResult) : base(loadedGameResult)
        {
            harmony.PatchAll();
        }

        ~ArenaTestGameManager()
        {
        }

        public void StartGameInArena()
        {
            NetworkConfiguration config = new NetworkConfiguration();

            m_Client = new LiteNetP2PClient(config, MessageBroker.Instance);

            if (m_Client.ConnectToP2PServer())
            {
                StartNewGame(this);
            }
            else
            {
                Logger.Error("Server Unreachable");
            }
        }

        public override void OnLoadFinished()
        {
            base.OnLoadFinished();

            if (!missionLoaded && Mission.Current != null && Mission.Current.IsLoadingFinished)
            {
                missionLoaded = true;
                StartArenaFight();
            }

            //get the settlement first
            Settlement settlement = Settlement.Find("town_ES3");

            CharacterObject characterObject = CharacterObject.PlayerCharacter;
            LocationEncounter locationEncounter = new TownEncounter(settlement);

            // create an encounter of the town with the player
            EncounterManager.StartSettlementEncounter(MobileParty.MainParty, settlement);

            //Set our encounter to the created encounter
            PlayerEncounter.LocationEncounter = locationEncounter;

            Location locationWithId = settlement.LocationComplex.GetLocationWithId("arena");

            int upgradeLevel = settlement.Town?.GetWallLevel() ?? 1;
            Location tavern = LocationComplex.Current.GetLocationWithId("tavern");
            string scene = tavern.GetSceneName(upgradeLevel);
            //Mission mission = SandBoxMissions.OpenIndoorMission(scene, tavern);
            //mission.AddMissionBehavior(new MissionNetworkBehavior(m_Client, MessageBroker.Instance));

            //PlayerEncounter.EnterSettlement();

            Location center = settlement.LocationComplex.GetLocationWithId("center");

            //return arena scenae name of current town
            //int upgradeLevel = settlement.IsTown ? settlement.Town.GetWallLevel() : 1;

            //Open a new arena mission with the scene; commented out because we are not doing Arena testing right now
            string civilianUpgradeLevelTag = Campaign.Current.Models.LocationModel.GetCivilianUpgradeLevelTag(upgradeLevel);
            Mission currentMission = MissionState.OpenNew("ArenaDuelMission", SandBoxMissions.CreateSandBoxMissionInitializerRecord(locationWithId.GetSceneName(upgradeLevel), "", false), (Mission mission) => new MissionBehavior[]
               {
                                         new MissionOptionsComponent(),
                                         //new ArenaDuelMissionController(CharacterObject.PlayerCharacter, false, false, null, 1), //this was the default controller that spawned the player and 1 opponent. Not very useful
                                         new MissionFacialAnimationHandler(),
                                         new MissionDebugHandler(),
                                         new MissionAgentPanicHandler(),
                                         new AgentCommonAILogic(),
                                         new AgentHumanAILogic(),
                                         new ArenaAgentStateDeciderLogic(),
                                         new VisualTrackerMissionBehavior(),
                                         new CampaignMissionComponent(),
                                         new MissionNetworkComponent(),
                                         new EquipmentControllerLeaveLogic(),
                                         new MissionAgentHandler(locationWithId, null),
                                         new MissionNetworkBehavior(m_Client, MessageBroker.Instance),
               }, true, true);

            MouseManager.ShowCursor(false);

        }

        // Spawn an agent based on its character object and frame. For now, Main agent character object is used
        // This should be the real character object in the future
        private static Agent SpawnAgent(CharacterObject character, MatrixFrame frame)
        {
            AgentBuildData agentBuildData = new AgentBuildData(character);
            agentBuildData.BodyProperties(character.GetBodyPropertiesMax());
            Mission mission = Mission.Current;
            agentBuildData = agentBuildData.Team(Mission.Current.PlayerAllyTeam).InitialPosition(frame.origin);
            Vec2 vec = frame.rotation.f.AsVec2;
            vec = vec.Normalized();
            Agent agent = mission.SpawnAgent(agentBuildData.InitialDirection(vec).NoHorses(true).Equipment(character.FirstBattleEquipment).TroopOrigin(new SimpleAgentOrigin(character, -1, null, default)), false, 0);
            agent.FadeIn();
            agent.Controller = Agent.ControllerType.None;
            return agent;
        }

        public static Agent AddPlayerToArena(bool isMain)
        {
            Mission.Current.PlayerTeam = Mission.Current.AttackerTeam;

            List<MatrixFrame> spawnFrames = (from e in Mission.Current.Scene.FindEntitiesWithTag("sp_arena")
                                             select e.GetGlobalFrame()).ToList();
            for (int i = 0; i < spawnFrames.Count; i++)
            {
                MatrixFrame value = spawnFrames[i];
                value.rotation.OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
                spawnFrames[i] = value;
            }
            //// get a random spawn point
            MatrixFrame randomElement = spawnFrames.GetRandomElement();
            ////remove the point so no overlap
            //_initialSpawnFrames.Remove(randomElement);
            ////find another spawn point
            //randomElement2 = randomElement;


            //// spawn an instance of the player (controlled by default)
            return SpawnAgent(CharacterObject.PlayerCharacter, randomElement);
        }

        // DEBUG METHOD: Starts an Arena fight
        public static void StartArenaFight()
        {
            //reset teams if any exists

            Mission.Current.ResetMission();

            //
            Mission.Current.Teams.Add(BattleSideEnum.Defender, Hero.MainHero.MapFaction.Color, Hero.MainHero.MapFaction.Color2, null, true, false, true);
            Mission.Current.Teams.Add(BattleSideEnum.Attacker, Hero.MainHero.MapFaction.Color2, Hero.MainHero.MapFaction.Color, null, true, false, true);

            //players is defender team
            Mission.Current.PlayerTeam = Mission.Current.DefenderTeam;


            //find areas of spawn

            List<MatrixFrame> spawnFrames = (from e in Mission.Current.Scene.FindEntitiesWithTag("sp_arena")
                                             select e.GetGlobalFrame()).ToList();
            for (int i = 0; i < spawnFrames.Count; i++)
            {
                MatrixFrame value = spawnFrames[i];
                value.rotation.OrthonormalizeAccordingToForwardAndKeepUpAsZAxis();
                spawnFrames[i] = value;
            }
            //// get a random spawn point
            MatrixFrame randomElement = spawnFrames.GetRandomElement();
            ////remove the point so no overlap
            //_initialSpawnFrames.Remove(randomElement);
            ////find another spawn point
            //randomElement2 = randomElement;


            //// spawn an instance of the player (controlled by default)
            SpawnAgent(CharacterObject.PlayerCharacter, randomElement);

        }

        public Agent SpawnAgent(Vec3 startingPos, CharacterObject character)
        {
            AgentBuildData agentBuildData = new AgentBuildData(character);
            agentBuildData.BodyProperties(character.GetBodyPropertiesMax());
            agentBuildData.InitialPosition(startingPos);
            agentBuildData.Team(Mission.Current.PlayerAllyTeam);
            agentBuildData.InitialDirection(Vec2.Forward);
            agentBuildData.NoHorses(true);
            agentBuildData.Equipment(character.FirstCivilianEquipment);
            agentBuildData.TroopOrigin(new SimpleAgentOrigin(character, -1, null, default));
            agentBuildData.Controller(Agent.ControllerType.None);

            Agent agent = default;
            GameLoopRunner.RunOnMainThread(() =>
            {
                agent = Mission.Current.SpawnAgent(agentBuildData);
                agent.FadeIn();
            });

            return agent;
        }

        public static string[] GetAllSpawnPoints(Scene scene)
        {
            List<GameEntity> entities = new List<GameEntity>();
            scene.GetEntities(ref entities);
            return entities.Where(entity => entity.Tags.Any(tag => tag.StartsWith("sp_"))).Select(entity => entity.Name).ToArray();
        }

        public override void OnGameEnd(Game game)
        {
            harmony.UnpatchAll();
            base.OnGameEnd(game);
        }
    }
}
