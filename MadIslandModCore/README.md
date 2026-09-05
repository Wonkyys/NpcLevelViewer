# 📦 Mad Island - ModCore Framework (v1.0.0)

[Español](#español) | [English](#english)

---

## Español

Framework base, API central y gestor de interoperabilidad para el desarrollo y ejecución de mods en **Mad Island** utilizando **BepInEx 5**.

Permite que múltiples mods interactúen de forma limpia y segura con el motor de juego sin colisiones, proveyendo acceso centralizado a los gestores nativos de Unity (`Managers`), un registro dinámico de módulos (`ModRegistry`) y soporte para interfaces unificadas.

### 🌟 Características Principales
* **Requisito Base:** Mods como **Mad Island Mod Manager**, **Aliados & Stats (NPC Level Viewer)** y **Reiniciador de Misiones (Quest Reset)** dependen de esta librería compartida para registrarse y comunicarse.
* **Acceso Seguro a la Partida y Escena (`GameManagers`):** Atajos estáticos a `StoryManager`, `SaveManager`, `UIManager`, `SoundManager`, etc., evitando búsquedas repetitivas de GameObjects y caídas de FPS.
* **Registro Dinámico de Módulos (`ModRegistry`):** Permite a cualquier mod implementar `IModModule`. Si el usuario tiene instalado **MadIslandModManager**, el mod se integrará al botón unificado `[ MODS ]`. Si no lo tiene, funcionará en modo independiente.
* **Guía Arquitectónica para Desarrolladores e IAs:** Incluye [`AI_DEVELOPER_NOTES.md`](AI_DEVELOPER_NOTES.md) con la ingeniería inversa completa del juego (restricciones de C# 5, Unity Mono, estructura de eventos y guardados XML).

### 🚀 Instalación
1. Asegúrate de tener instalado **BepInEx 5.4.x (x64)** en la raíz de tu juego.
2. Coloca la carpeta `MadIslandModCore` dentro de:
   ```text
   Mad Island/BepInEx/plugins/
   ```
*(Nota: Por sí solo este mod no muestra menús visuales; actúa como motor para los demás mods).*

### 💻 Guía para Desarrolladores: Crear un Mod Compatible
```csharp
using UnityEngine;
using MadIslandModCore;

namespace MiNuevoMod
{
    public class MiModUI : MonoBehaviour, IModModule
    {
        public string ModId { get { return "mi_mod"; } }
        public string ModName { get { return "⚡ Mi Nuevo Mod"; } }
        public string ModDescription { get { return "Descripción de mi mod."; } }
        public string ModVersion { get { return "1.0.0"; } }

        private void Start() { ModRegistry.Register(this); }
        private void OnDestroy() { ModRegistry.Unregister(this); }

        // Renderizado dentro de MadIslandModManager
        public void OnDrawUI(Rect area)
        {
            GUILayout.Label("¡Hola desde Mi Nuevo Mod!");
        }

        // Renderizado independiente si NO hay Mod Manager instalado
        private void OnGUI()
        {
            if (ModRegistry.IsModManagerActive) return;

            if (GUI.Button(new Rect(Screen.width - 80, 100, 70, 25), "[MiMod]"))
            {
                // Abrir ventana standalone
            }
        }
    }
}
```

---

## English

Core framework, central API, and interoperability manager for developing and running mods in **Mad Island** using **BepInEx 5**.

Enables multiple mods to interact cleanly and safely with the game engine without collisions, providing centralized access to Unity native managers (`Managers`), dynamic module registration (`ModRegistry`), and unified UI support.

### 🌟 Key Features
* **Core Prerequisite:** Mods like **Mad Island Mod Manager**, **Allies & Stats (NPC Level Viewer)**, and **Quest Reset** depend on this shared library to register and communicate.
* **Safe Game & Scene Access (`GameManagers`):** Strongly-typed shortcuts to `StoryManager`, `SaveManager`, `UIManager`, `SoundManager`, etc., cached to avoid frame drops.
* **Dynamic Module Registry (`ModRegistry`):** Any mod implementing `IModModule` is automatically detected. If **MadIslandModManager** is installed, it joins the `[ MODS ]` hub; otherwise, it operates in standalone mode.
* **Architectural Guide for Devs & AIs:** Includes [`AI_DEVELOPER_NOTES.md`](AI_DEVELOPER_NOTES.md) containing the full reverse engineering of the game (C# 5 syntax limits, Unity Mono, event hierarchies, and XML save formats).

### 🚀 Installation
1. Ensure **BepInEx 5.4.x (x64)** is installed in your game's root directory.
2. Place the `MadIslandModCore` folder into:
   ```text
   Mad Island/BepInEx/plugins/
   ```
*(Note: On its own, this framework does not display any visible UI; it serves as the underlying engine for other mods).*

### 💻 Developer Guide: Creating a Compatible Mod
Implement `IModModule`, register with `ModRegistry.Register(this)`, and define `OnDrawUI(Rect area)`. Refer to the C# code snippet above or inspect [`AI_DEVELOPER_NOTES.md`](AI_DEVELOPER_NOTES.md) for deeper details.

---

## 🛠️ Compilación / Build
```powershell
./build.ps1
```

## 📄 Licencia / License
MIT License.
