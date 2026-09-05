using System;
using UnityEngine;

namespace MadIslandModCore
{
    /// <summary>
    /// Acceso centralizado y seguro a todos los subsistemas y managers del motor de Mad Island.
    /// </summary>
    public static class GameManagers
    {
        private static ManagersScript cachedMn;

        public static ManagersScript Instance
        {
            get
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
        }

        public static ManagersScript Managers
        {
            get { return Instance; }
        }

        public static bool IsAvailable
        {
            get { return Instance != null; }
        }

        public static SaveManager Save
        {
            get { return Instance != null ? Instance.save : null; }
        }

        public static StoryManager Story
        {
            get { return Instance != null ? Instance.story : null; }
        }

        public static NPCManager Npc
        {
            get { return Instance != null ? Instance.npcMN : null; }
        }

        public static ItemManager Items
        {
            get { return Instance != null ? Instance.itemMN : null; }
        }

        public static GameManager Game
        {
            get { return Instance != null ? Instance.gameMN : null; }
        }

        public static UIManager UI
        {
            get { return Instance != null ? Instance.uiMN : null; }
        }

        public static SoundManager Sound
        {
            get { return Instance != null ? Instance.sound : null; }
        }

        public static InventoryManager Inventory
        {
            get { return Instance != null ? Instance.inventory : null; }
        }

        public static EventManager Event
        {
            get { return Instance != null ? Instance.eventMN : null; }
        }
    }
}
