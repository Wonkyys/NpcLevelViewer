using System;
using System.Collections.Generic;
using UnityEngine;

namespace MadIslandModCore
{
    /// <summary>
    /// Registro central de mods para Mad Island.
    /// Permite a cualquier mod registrarse para ser listado en el Mod Manager.
    /// </summary>
    public static class ModRegistry
    {
        private static readonly List<IModModule> _modules = new List<IModModule>();

        /// <summary>
        /// Lista de todos los mods registrados actualmente en el sistema.
        /// </summary>
        public static List<IModModule> Modules
        {
            get { return _modules; }
        }

        /// <summary>
        /// Indica si el Mod Manager Hub principal esta activo en el juego.
        /// Los mods usan esto para saber si deben ocultar sus botones flotantes independientes.
        /// </summary>
        public static bool IsModManagerActive { get; set; }

        /// <summary>
        /// Registra un modulo/mod en el gestor.
        /// </summary>
        public static void Register(IModModule module)
        {
            if (module == null) return;
            for (int i = 0; i < _modules.Count; i++)
            {
                if (_modules[i].ModId == module.ModId)
                {
                    _modules[i] = module;
                    return;
                }
            }
            _modules.Add(module);
            Debug.Log(string.Format("[MadIslandModCore] Mod registrado: {0} ({1})", module.ModName, module.ModVersion));
        }

        /// <summary>
        /// Desregistra un modulo si se deshabilita.
        /// </summary>
        public static void Unregister(IModModule module)
        {
            if (module != null)
                _modules.Remove(module);
        }
    }
}
