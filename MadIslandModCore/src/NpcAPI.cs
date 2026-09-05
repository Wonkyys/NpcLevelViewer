using System;
using System.Collections.Generic;
using UnityEngine;

namespace MadIslandModCore
{
    /// <summary>
    /// API para manipulación, estadísticas y consulta de NPCs y personajes.
    /// </summary>
    public static class NpcAPI
    {
        public static List<CommonStates> GetAllFriends()
        {
            var result = new List<CommonStates>();
            var seen = new HashSet<CommonStates>();

            var mn = GameManagers.Instance;
            if (mn != null && mn.npcMN != null && mn.npcMN.friendList != null)
            {
                var list = mn.npcMN.friendList;
                for (int i = 0; i < list.Count; i++)
                {
                    var friend = list[i];
                    if (friend != null && friend.pMove == null && seen.Add(friend))
                    {
                        result.Add(friend);
                    }
                }
            }

            return result;
        }

        public static int GetStatCost(CommonStates common, int statIndex)
        {
            if (common == null || common.status == null || statIndex < 0 || statIndex >= common.status.Length)
                return 1;

            return (common.status[statIndex] / 10) + 1;
        }

        public static bool CanUpgradeStat(CommonStates common, int statIndex)
        {
            if (common == null || common.status == null || statIndex < 0 || statIndex >= common.status.Length)
                return false;

            if (statIndex == 2 && common.speedLimit > 0f && common.speed >= common.speedLimit)
                return false;

            int cost = GetStatCost(common, statIndex);
            return common.statusPoint >= cost;
        }

        public static int UpgradeStat(CommonStates common, int statIndex, int requestedTimes)
        {
            if (common == null || common.status == null || statIndex < 0 || statIndex >= common.status.Length)
                return 0;

            if (requestedTimes <= 0) requestedTimes = 1;
            int upgraded = 0;

            StatsData statsData = null;
            try
            {
                if (GameManagers.Npc != null)
                {
                    statsData = GameManagers.Npc.GetStatsData(common.npcID);
                }
            }
            catch { }

            float upLife = (statsData != null && statsData.upLife > 0f) ? statsData.upLife : 2f;
            float upAttack = (statsData != null && statsData.upAttack > 0f) ? statsData.upAttack : 1f;

            for (int i = 0; i < requestedTimes; i++)
            {
                if (!CanUpgradeStat(common, statIndex))
                    break;

                int cost = GetStatCost(common, statIndex);
                if (common.statusPoint < cost)
                    break;

                common.statusPoint -= cost;
                common.status[statIndex]++;

                if (statIndex == 0) // HP
                {
                    common.maxLife += upLife;
                    common.life += upLife;
                }
                else if (statIndex == 1) // ATK
                {
                    common.attack += upAttack;
                }
                else if (statIndex == 2) // SPD
                {
                    common.speed += 0.005f;
                }

                upgraded++;
            }

            if (upgraded > 0)
            {
                try
                {
                    if (GameManagers.UI != null)
                    {
                        GameManagers.UI.StatusChange(common);
                    }
                    if (GameManagers.Sound != null)
                    {
                        GameManagers.Sound.GoSoundButton(0);
                    }
                }
                catch { }
            }

            return upgraded;
        }

        public static string GetNpcDisplayName(CommonStates common)
        {
            if (common == null) return "Unknown";
            if (!string.IsNullOrEmpty(common.charaName)) return common.charaName;

            try
            {
                if (GameManagers.Npc != null)
                {
                    var data = GameManagers.Npc.GetStatsData(common.npcID);
                    if (data != null && !string.IsNullOrEmpty(data.charaName))
                        return data.charaName;
                }
            }
            catch { }

            if (common.friendID > 0)
                return string.Format("Friend #{0}", common.friendID);

            return string.Format("NPC #{0}", common.npcID);
        }

        public static GameObject SpawnNpc(int npcId, Vector3 position)
        {
            if (GameManagers.Npc == null) return null;
            try
            {
                var obj = GameManagers.Npc.Spawn(npcId);
                if (obj != null)
                {
                    obj.transform.position = position;
                }
                return obj;
            }
            catch
            {
                return null;
            }
        }
    }
}
