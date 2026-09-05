using UnityEngine;

namespace MadIslandModCore
{
    /// <summary>
    /// Interfaz que implementa cualquier mod para integrarse con MadIslandModManager.
    /// Si el Mod Manager esta presente, mostrara el mod en su lista y delegara OnDrawUI.
    /// Si el Mod Manager no esta presente, el mod puede ejecutarse en modo independiente (standalone).
    /// </summary>
    public interface IModModule
    {
        string ModId { get; }
        string ModName { get; }
        string ModDescription { get; }
        string ModVersion { get; }

        /// <summary>
        /// Se llama cuando el mod esta seleccionado en el Mod Manager para dibujar su contenido en pantalla.
        /// </summary>
        void OnDrawUI(Rect area);
    }
}
