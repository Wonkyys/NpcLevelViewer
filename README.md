# 👥 Mad Island - Aliados & Stats (NPC Level Viewer v1.0.0)

[Español](#español) | [English](#english)

---

## Español

Un mod completo para la gestión, inspección y mejora de estadísticas de tus NPCs aliados en **Mad Island**.

Permite monitorear el nivel real de cada aliado, sus puntos de atributo dorados disponibles y asignar mejoras de forma masiva o individual con atajos rápidos de teclado.

### ✨ Características Principales
* **📊 Monitoreo Detallado:** Consulta vida actual/máxima, nivel dinámico y puntos de atributo dorados (`statusPoint`) de cada aliado.
* **⚡ Asignación Rápida de Mejoras:**
  * **❤️ Vitalidad / Salud (H):** +100 puntos de vida máxima por punto.
  * **⚔️ Fuerza / Ataque (F o A):** +10 puntos de daño de ataque por punto.
  * **👟 Velocidad / Agilidad (V o S):** +10 puntos de velocidad y recuperación por punto.
* **🔢 Multiplicadores por Lote:** Botones de **x1**, **x10** o **x100** por clic. O mantén presionado **Shift** (x10) o **Ctrl** (x100) mientras usas la tecla de atajo.
* **🔍 Filtros y Búsqueda:** Filtra por aliados que tienen puntos disponibles o busca por nombre.

### 🔄 Modo Dual (Independiente o Integrado)
* **Con MadIslandModManager:** Se oculta el botón individual; el mod se integra limpiamente en el menú **`[ MODS ]`**.
* **Sin ModManager (Standalone):** Muestra su propio botón flotante **`[NPC]`** en la esquina superior derecha y se puede alternar con la tecla **`F5`**.

### ❓ ¿Por qué se incluye `MadIslandModCore` en la descarga?
Es la librería base obligatoria para acceder a las estructuras de datos nativas de los NPCs (`CommonStates`). Viene incluida para que la instalación sea 100% *Plug & Play*.

### 📥 Instalación
1. Instala **BepInEx 5.4.x (x64)**.
2. Descarga la última versión desde **Releases**.
3. Copia las carpetas `NpcLevelViewer` y `MadIslandModCore` en:
   ```text
   Mad Island/BepInEx/plugins/
   ```

---

## English

A comprehensive ally management and stat inspection mod for **Mad Island**.

Monitor real ally levels, golden attribute points, and perform instant stat upgrades individually or in bulk via hotkeys.

### ✨ Key Features
* **📊 In-Depth Ally Stats:** View current/max HP, dynamic level, and available golden attribute points (`statusPoint`) for every recruited companion.
* **⚡ Quick Stat Upgrades:**
  * **❤️ Vitality / Health (H):** +100 max HP per point.
  * **⚔️ Strength / Attack (F or A):** +10 attack power per point.
  * **👟 Speed / Agility (V or S):** +10 speed and recovery per point.
* **🔢 Bulk Multipliers:** On-screen **x1**, **x10**, **x100** buttons. Or hold **Shift** (x10) or **Ctrl** (x100) while pressing the shortcut key over an ally card.
* **🔍 Filters & Search:** Instantly search by name or filter NPCs with unspent attribute points.

### 🔄 Dual-Mode Support
* **With MadIslandModManager:** Standalone button is hidden; the UI embeds into the central **`[ MODS ]`** hub.
* **Without ModManager (Standalone):** Shows a dedicated floating **`[NPC]`** button and toggles with the **`F5`** key.

### ❓ Why is `MadIslandModCore` included in the release?
It is the required core framework allowing safe runtime access to NPC data (`CommonStates`). Bundled for effortless *Plug & Play* installation.

### 📥 Installation
1. Install **BepInEx 5.4.x (x64)**.
2. Download the release from **Releases**.
3. Copy both `NpcLevelViewer` and `MadIslandModCore` folders into:
   ```text
   Mad Island/BepInEx/plugins/
   ```

---

## 🛠️ Compilación / Build
```powershell
./build.ps1
```

## 📄 Licencia / License
MIT License.
