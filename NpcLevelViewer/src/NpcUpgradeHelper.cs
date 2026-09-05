using System;
using System.Collections.Generic;
using UnityEngine;

namespace NpcLevelViewer
{
    public static class NpcUpgradeHelper
    {
        // Stat Indices:
        // 0 = Health / Life (H / Vitalidad)
        // 1 = Attack / Strength (F / A / Fuerza)
        // 2 = Speed / Agility (V / S / Velocidad)

        private static ManagersScript cachedMn;

        public static ManagersScript GetManagers()
        {
            if (cachedMn == null)
            {
                var go = GameObject.Find("Managers");
                if (go != null)
                {
                    cachedMn = go.GetComponent<ManagersScript>();
                }
                if (cachedMn == null)
                {
                    cachedMn = UnityEngine.Object.FindObjectOfType<ManagersScript>();
                }
            }
            return cachedMn;
        }

        public static int GetStatCost(CommonStates common, int statIndex)
        {
            if (common == null || common.status == null || statIndex < 0 || statIndex >= common.status.Length)
                return 1;

            int currentVal = common.status[statIndex];
            return (currentVal / 10) + 1;
        }

        public static bool CanUpgrade(CommonStates common, int statIndex)
        {
            if (common == null || common.status == null || statIndex < 0 || statIndex >= common.status.Length)
                return false;

            // Check speed limit
            if (statIndex == 2 && common.speedLimit > 0f && common.speed >= common.speedLimit)
                return false;

            int cost = GetStatCost(common, statIndex);
            return common.statusPoint >= cost;
        }

        public static bool CanUpgradeAny(CommonStates common)
        {
            if (common == null || common.statusPoint <= 0)
                return false;

            return CanUpgrade(common, 0) || CanUpgrade(common, 1) || CanUpgrade(common, 2);
        }

        public static int UpgradeStat(CommonStates common, int statIndex, int requestedTimes)
        {
            if (common == null || common.status == null || statIndex < 0 || statIndex >= common.status.Length)
                return 0;

            if (requestedTimes <= 0)
                requestedTimes = 1;

            int timesUpgraded = 0;

            var mn = GetManagers();

            // Get StatsData for life and attack bonuses per point
            StatsData statsData = null;
            try
            {
                if (mn != null && mn.npcMN != null)
                {
                    statsData = mn.npcMN.GetStatsData(common.npcID);
                }
            }
            catch { }

            float upLife = (statsData != null && statsData.upLife > 0f) ? statsData.upLife : 2f;
            float upAttack = (statsData != null && statsData.upAttack > 0f) ? statsData.upAttack : 1f;

            for (int i = 0; i < requestedTimes; i++)
            {
                if (!CanUpgrade(common, statIndex))
                    break;

                int cost = GetStatCost(common, statIndex);
                if (common.statusPoint < cost)
                    break;

                common.statusPoint -= cost;
                common.status[statIndex]++;

                if (statIndex == 0) // HP / Life
                {
                    common.maxLife += upLife;
                    common.life += upLife;
                }
                else if (statIndex == 1) // ATK / Strength
                {
                    common.attack += upAttack;
                }
                else if (statIndex == 2) // SPD / Agility
                {
                    common.speed += 0.005f;
                }

                timesUpgraded++;
            }

            if (timesUpgraded > 0)
            {
                try
                {
                    if (mn != null)
                    {
                        if (mn.uiMN != null)
                        {
                            mn.uiMN.StatusChange(common);
                        }
                        if (mn.sound != null)
                        {
                            mn.sound.GoSoundButton(0);
                        }
                    }
                }
                catch { }
            }

            return timesUpgraded;
        }

        public static List<CommonStates> GetFriendNpcs()
        {
            var result = new List<CommonStates>();
            var seen = new HashSet<CommonStates>();

            var mn = GetManagers();

            // 1. Primary: Master friend list from NPCManager
            try
            {
                if (mn != null && mn.npcMN != null && mn.npcMN.friendList != null)
                {
                    for (int i = 0; i < mn.npcMN.friendList.Count; i++)
                    {
                        var friend = mn.npcMN.friendList[i];
                        if (friend != null && friend.pMove == null && seen.Add(friend))
                        {
                            result.Add(friend);
                        }
                    }
                }
            }
            catch { }

            // 2. Secondary fallback: only if friendList was empty, check active scene objects
            if (result.Count == 0)
            {
                try
                {
                    var allCommons = UnityEngine.Object.FindObjectsOfType<CommonStates>();
                    for (int i = 0; i < allCommons.Length; i++)
                    {
                        var cs = allCommons[i];
                        if (cs != null && cs.pMove == null && IsValidFriend(cs) && seen.Add(cs))
                        {
                            result.Add(cs);
                        }
                    }
                }
                catch { }
            }

            // Sort: NPCs with available points first, then by points descending, then by name
            result.Sort(delegate(CommonStates a, CommonStates b)
            {
                bool aCan = CanUpgradeAny(a);
                bool bCan = CanUpgradeAny(b);
                if (aCan != bCan) return aCan ? -1 : 1;

                if (a.statusPoint != b.statusPoint)
                    return b.statusPoint.CompareTo(a.statusPoint);

                string nameA = GetNpcDisplayName(a);
                string nameB = GetNpcDisplayName(b);
                return string.Compare(nameA, nameB, StringComparison.OrdinalIgnoreCase);
            });

            return result;
        }

        public static bool IsValidFriend(CommonStates cs)
        {
            if (cs == null) return false;
            // Ignore player character
            if (cs.pMove != null) return false;

            // Check if friendID is assigned
            if (cs.friendID > 0) return true;

            // Check Employ status
            if (cs.employ == CommonStates.Employ.Friend ||
                cs.employ == CommonStates.Employ.Favorite ||
                cs.employ == CommonStates.Employ.Prisoner)
            {
                return true;
            }

            try
            {
                if (cs.Employed(false)) return true;
            }
            catch { }

            // Check if they have status points
            if (cs.statusPoint > 0 && cs.status != null && cs.status.Length >= 3)
                return true;

            return false;
        }

        public static string GetNpcDisplayName(CommonStates cs)
        {
            if (cs == null) return "Unknown";
            if (!string.IsNullOrEmpty(cs.charaName))
                return cs.charaName;

            try
            {
                var mn = GetManagers();
                if (mn != null && mn.npcMN != null)
                {
                    var data = mn.npcMN.GetStatsData(cs.npcID);
                    if (data != null && !string.IsNullOrEmpty(data.charaName))
                        return data.charaName;
                }
            }
            catch { }

            if (cs.friendID > 0)
                return string.Format("Friend #{0}", cs.friendID);

            return string.Format("NPC #{0}", cs.npcID);
        }
    }
}
