using ProtoBuf;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Vintagestory.API.Server;

namespace VsQuest
{
    public class QuestPersistenceManager
    {
        private readonly ConcurrentDictionary<string, List<ActiveQuest>> playerQuests;
        private readonly ICoreServerAPI sapi;

        public QuestPersistenceManager(ICoreServerAPI sapi)
        {
            this.sapi = sapi;
            this.playerQuests = new ConcurrentDictionary<string, List<ActiveQuest>>();
        }

        public List<ActiveQuest> GetPlayerQuests(string playerUID)
        {
            return playerQuests.GetOrAdd(playerUID, (val) => LoadPlayerQuests(val));
        }

        public void SavePlayerQuests(string playerUID, List<ActiveQuest> activeQuests)
        {
            sapi.WorldManager.SaveGame.StoreData<List<ActiveQuest>>(String.Format("quests-{0}", playerUID), activeQuests);
        }

        private List<ActiveQuest> LoadPlayerQuests(string playerUID)
        {
            try
            {
                return sapi.WorldManager.SaveGame.GetData<List<ActiveQuest>>(String.Format("quests-{0}", playerUID), new List<ActiveQuest>());
            }
            catch (ProtoException)
            {
                sapi.Logger.Error("Could not load quests for player with id {0}, corrupted quests will be deleted.", playerUID);
                return new List<ActiveQuest>();
            }
        }

        public void SaveAllPlayerQuests()
        {
            foreach (var player in playerQuests)
            {
                SavePlayerQuests(player.Key, player.Value);
            }
        }

        public void UnloadPlayerQuests(string playerUID)
        {
            if (playerQuests.TryGetValue(playerUID, out var activeQuests))
            {
                SavePlayerQuests(playerUID, activeQuests);
                playerQuests.TryRemove(playerUID, out _);
            }
        }
    }
}
