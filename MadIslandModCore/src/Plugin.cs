using System;
using BepInEx;
using UnityEngine;

namespace MadIslandModCore
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class ModCorePlugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.leonardo.madisland.modcore";
        public const string PLUGIN_NAME = "Mad Island Modding Core";
        public const string PLUGIN_VERSION = "1.0.0";

        public static ModCorePlugin Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
            Logger.LogInfo("Mad Island Modding Core Framework v1.0.0 cargado con exito.");
        }
    }
}
