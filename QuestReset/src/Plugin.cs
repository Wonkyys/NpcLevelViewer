using System;
using BepInEx;
using UnityEngine;

namespace QuestReset
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.leonardo.madisland.questreset";
        public const string PLUGIN_NAME = "Mad Island Quest Reset";
        public const string PLUGIN_VERSION = "1.0.0";

        public static Plugin Instance { get; private set; }

        private void Awake()
        {
            Instance = this;

            var host = new GameObject("QuestResetUI_Host");
            DontDestroyOnLoad(host);
            host.AddComponent<QuestResetUI>();

            Logger.LogInfo("Mad Island Quest Reset v1.0.0 cargado correctamente.");
        }
    }
}
