using System;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace NpcLevelViewer
{
    [BepInPlugin("com.leo.madisland.npclevelviewer", "NPC Level & Stat Viewer", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; private set; }

        private ConfigEntry<KeyCode> configToggleKey;
        private ConfigEntry<bool> configSpanishLabels;
        private ConfigEntry<int> configBulkMultiplier;

        private void Awake()
        {
            Instance = this;

            configToggleKey = Config.Bind("General", "ToggleKey", KeyCode.F5, "Tecla para abrir/cerrar la lista de NPCs (F5 por defecto)");
            configSpanishLabels = Config.Bind("General", "SpanishLabels", true, "Usar letras H-F-V (Vitalidad, Fuerza, Velocidad) o H-A-S (Health, Attack, Speed)");
            configBulkMultiplier = Config.Bind("General", "DefaultBulkMultiplier", 1, "Multiplicador de puntos por click por defecto (1, 10, 100)");

            // Create UI GameObject
            var uiObj = new GameObject("NpcStatUI_Host");
            DontDestroyOnLoad(uiObj);
            var ui = uiObj.AddComponent<NpcStatUI>();
            ui.ToggleKey = configToggleKey.Value;
            ui.SpanishLabels = configSpanishLabels.Value;
            ui.BulkMultiplier = configBulkMultiplier.Value;

            Logger.LogInfo("NPC Level & Stat Viewer mod cargado correctamente! Presiona F5 para abrir.");
        }
    }
}
