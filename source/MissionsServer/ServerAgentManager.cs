﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace MissionsServer
{
    public class ServerAgentManager
    {
        private ConcurrentDictionary<(int,int), string> clientInfoToAgentIDMap = new ConcurrentDictionary<(int, int), string>();
        private ConcurrentDictionary<string, (int, int)> agentIDToClientInfoMap = new ConcurrentDictionary<string, (int, int)>();

        private ServerAgentManager() { }
        private static ServerAgentManager instance;
        public static ServerAgentManager Instance()
        {
            if(instance == null)
            {
                instance = new ServerAgentManager();
            }
            return instance;
        }
        public string GetAgentID(int clientId, int agentIndex)
        {
            if(clientInfoToAgentIDMap.ContainsKey((clientId, agentIndex)))
            {
                return clientInfoToAgentIDMap[(clientId, agentIndex)];
            }
            string agentID = Guid.NewGuid().ToString();
            clientInfoToAgentIDMap[(clientId, agentIndex)] = agentID;
            agentIDToClientInfoMap[agentID] = (clientId, agentIndex);
            return agentID;
        }

        public (int, int) GetClientInfo(string agentId)
        {
            return agentIDToClientInfoMap[agentId];
        }

        public void RemoveAgent(string agentId)
        {
            (int, int) clientInfo = this.GetClientInfo(agentId);
            this.agentIDToClientInfoMap.TryRemove(agentId, out _);
            this.clientInfoToAgentIDMap.TryRemove(clientInfo, out _);
        }
    }
}