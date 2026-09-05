using System;
using System.Collections.Generic;
using UnityEngine;

namespace MadIslandModCore
{
    /// <summary>
    /// API para inspección y modificación segura de la partida guardada (Saves y Quests).
    /// </summary>
    public static class SaveAPI
    {
        public static SaveManager.SaveEntry CurrentSaveEntry
        {
            get
            {
                var save = GameManagers.Save;
                return save != null ? save.saveEntry : null;
            }
        }

        public static bool IsSaveLoaded
        {
            get
            {
                var entry = CurrentSaveEntry;
                return entry != null && entry.baseSave != null;
            }
        }

        /// <summary>
        /// Obtiene todas las misiones registradas en la partida actual.
        /// </summary>
        public static List<SaveManager.QuestSave> GetAllQuests()
        {
            var result = new List<SaveManager.QuestSave>();
            var entry = CurrentSaveEntry;
            if (entry != null && entry.quests != null)
            {
                result.AddRange(entry.quests);
            }
            return result;
        }

        /// <summary>
        /// Obtiene el progreso actual de una misión (0 = no iniciada, 1 = en curso, 2 = completada, etc.).
        /// </summary>
        public static int GetQuestProgress(string questKey)
        {
            if (string.IsNullOrEmpty(questKey)) return -1;
            var entry = CurrentSaveEntry;
            if (entry != null && entry.quests != null)
            {
                for (int i = 0; i < entry.quests.Length; i++)
                {
                    if (entry.quests[i].questKey == questKey)
                    {
                        return entry.quests[i].progress;
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// Modifica el estado/progreso de una misión en vivo usando la función nativa del juego.
        /// Ejemplo: SetQuestProgress("kana_sex", 0) para reiniciar la misión.
        /// </summary>
        public static bool SetQuestProgress(string questKey, int newProgress)
        {
            if (string.IsNullOrEmpty(questKey)) return false;

            // 1. Usar el método oficial del juego si StoryManager está activo
            try
            {
                var story = GameManagers.Story;
                if (story != null)
                {
                    story.ProgressChange(questKey, newProgress);
                    return true;
                }
            }
            catch { }

            // 2. Modificación directa en la estructura de guardado en memoria
            var entry = CurrentSaveEntry;
            if (entry != null && entry.quests != null)
            {
                for (int i = 0; i < entry.quests.Length; i++)
                {
                    if (entry.quests[i].questKey == questKey)
                    {
                        entry.quests[i].progress = newProgress;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Obtiene la lista de personajes guardados en el archivo de guardado.
        /// </summary>
        public static List<SaveManager.CharaSave> GetSavedNpcs()
        {
            var result = new List<SaveManager.CharaSave>();
            var entry = CurrentSaveEntry;
            if (entry != null && entry.npcSave != null)
            {
                result.AddRange(entry.npcSave);
            }
            return result;
        }
    }
}
