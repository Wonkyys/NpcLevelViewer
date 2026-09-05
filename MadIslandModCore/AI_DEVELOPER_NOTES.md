# 🤖 Mad Island - AI & Developer Architecture Notes (Cheat Sheet)

> **Nota para IAs y Desarrolladores:**  
> Este documento resume toda la ingeniería inversa, descompilación, estructura de clases, singletons, restricciones del compilador y convenciones descubiertas en **Mad Island**.  
> **Consúltalo antes de crear nuevos mods para evitar escaneos y descompilaciones innecesarias de `Assembly-CSharp.dll`.**

---

## 1. Entorno de Ejecución y Compilación

* **Motor del Juego:** Unity (Arquitectura Mono x64, Windows).
* **Gestor de Mods:** BepInEx 5.4.x (x64) ubicado en `BepInEx/`.
* **Herramienta de Compilación Oficial:** `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe`
* **⚠️ RESTRICCIÓN CRÍTICA DE C# (C# 5.0):**  
  El compilador nativo de Windows `.NET 4.0/4.8` solo soporta sintaxis de **C# 5**.
  * ❌ **NO usar** interpolación de cadenas `$"texto {var}"` ➔ Usar `string.Format("texto {0}", var)` o `+`.
  * ❌ **NO usar** operador nulo condicional `obj?.Propiedad` ➔ Usar `if (obj != null) { ... }`.
  * ❌ **NO usar** `nameof(Simbolo)`.
  * ❌ **NO usar** miembros de cuerpo de expresión `int Prop => 5;` ➔ Usar `{ get { return 5; } }`.
  * ❌ **NO usar** variables out en línea `int.TryParse(s, out int x);` ➔ Declarar `int x; int.TryParse(s, out x);`.

### Ensamblados de Referencia Necesarios
* **Unity:**
  * `Mad Island_Data/Managed/UnityEngine.dll`
  * `Mad Island_Data/Managed/UnityEngine.CoreModule.dll`
  * `Mad Island_Data/Managed/UnityEngine.IMGUIModule.dll` (Para `OnGUI`, `GUI.Window`, `GUILayout`)
  * `Mad Island_Data/Managed/UnityEngine.InputLegacyModule.dll` (Para `Input.GetKeyDown`, `KeyCode`)
  * `Mad Island_Data/Managed/UnityEngine.PhysicsModule.dll`
  * `Mad Island_Data/Managed/UnityEngine.TextRenderingModule.dll`
  * `Mad Island_Data/Managed/UnityEngine.UI.dll` (Para `UnityEngine.UI.Image`)
  * `Mad Island_Data/Managed/UnityEngine.UIModule.dll`
* **Juego:**
  * `Mad Island_Data/Managed/Assembly-CSharp.dll` (Clases nativas del juego)
* **BepInEx:**
  * `BepInEx/core/BepInEx.dll`
  * `BepInEx/core/0Harmony.dll`
* **Framework Core:**
  * `BepInEx/plugins/MadIslandModCore/MadIslandModCore.dll`

---

## 2. Acceso a los Gestores del Juego (`GameManagers`)

El objeto raíz de control en la escena es `GameObject.Find("Managers")`, cuyo componente principal es `MainManager.instance`.  
En `MadIslandModCore`, puedes acceder directamente mediante la clase estática `GameManagers`:

```csharp
using MadIslandModCore;

MainManager mn = GameManagers.Managers; // o MainManager.instance
SaveManager save = GameManagers.Save;
StoryManager story = GameManagers.Story;
GameManager game = GameManagers.Game;
UIManager ui = GameManagers.UI;
SoundManager sound = GameManagers.Sound;
FieldManager field = GameManagers.Field;
NPCManager npc = GameManagers.Npc;
ItemManager items = GameManagers.Items;
```

---

## 3. Sistema de NPCs y Aliados (`CommonStates`)

Tanto el jugador como todos los NPCs utilizan la clase `CommonStates`.

### Campos Críticos de `CommonStates`:
* `friend` (int):
  * `0` = Neutral / Salvaje / Enemigo.
  * `1` o `2` = Aliado reclutado / Amigo.
* `dead` (int): `0` = Vivo, `1` = Muerto.
* `life` (float) y `maxLife` (float): Salud actual y máxima.
* `statusPoint` (int): **Puntos dorados** disponibles para mejorar al NPC.
* `upLife` (int): Inversiones en **Vitalidad/Salud** (+100 maxLife por punto).
* `upAttack` (int): Inversiones en **Fuerza/Ataque** (+10 ataque por punto).
* `upFaint` (int): Inversiones en **Velocidad/Recuperación** (+10 velocidad por punto).
* **Nivel del NPC:** Calculado en runtime:
  `int nivel = 1 + cs.upLife + cs.upAttack + cs.upFaint;`

### Método Seguro de Mejora:
Para subir un stat:
1. Validar que `cs.statusPoint >= cantidad`.
2. Restar `cs.statusPoint -= cantidad`.
3. Sumar al stat (`cs.upLife += cantidad`, etc.).
4. Si es vida: `cs.maxLife += cantidad * 100f; cs.life = cs.maxLife;`.
5. Forzar actualización de estadísticas del juego: llamar a `cs.SetStatus();` si existe.

---

## 4. Sistema de Misiones y Eventos (Quests & Triggers)

El juego cuenta con **38 misiones** registradas (`Boss_*`, `Main_*`, `Sub_*`).

### Jerarquía de Escena `Events`:
Todos los triggers y spawners de misiones cuelgan de `GameObject.Find("Events")`:
* Carpetas hijas nombradas como la misión: `Boss_Spider`, `Boss_Gen`, `Main_Gate`, etc.
* Bloques de victoria (que se desactivan al ganar): `Block_00`, `Block_01`, `fence_01`.
* Bloques de entrada (que deben reactivarse): `cave01_block_00`, `fence_00`, `Key/stones`.
* Componentes reactivables:
  * `EnemySpawner`: Activar con `spawner.gameObject.SetActive(true); spawner.active = true;`.
  * `CommonStates`: Restaurar con `dead = 0; life = maxLife; gameObject.SetActive(true);`.
  * `EventStarter`: Componente que inicia cinemáticas/diálogos y maneja el ícono en el mapa (`mapFade`).

### Procedimiento Completo de Reinicio de Misión:
1. **Memoria de Historia:** `story.ProgressChange(questKey, 0);` y `story.QuestDiaryChange(questKey, 0);`.
2. **Entrada de Guardado:** `save.saveEntry.quests` -> buscar `questKey` y poner `progress = 0`.
3. **Archivos XML:** Modificar `Mad Island_Data/StreamingAssets/XML/SaveData*.xml` usando XPath `//QuestSave[questKey='...']/progress = 0`.
4. **Reactivar Triggers:** `story.ActiveEventTrigger(folder, trigger, true);`.
5. **Reactivar `EventStarter`:** Encontrar `EventStarter` en `Events` y llamar `es.gameObject.SetActive(true);` y restaurar `es.mapFade.color = Color.white;`.
6. **Limpiar UI:** `ui.DeleteQuestByName(questKey);`.
7. **Audio:** `sound.GoSoundButton(0);`.

---

## 5. Arquitectura de Módulos (`MadIslandModCore`)

Para que un mod se integre automáticamente al catálogo del **Mod Manager** y soporte **Modo Dual**, debe implementar `IModModule`:

```csharp
public class MiModUI : MonoBehaviour, IModModule
{
    public string ModId { get { return "mi_mod"; } }
    public string ModName { get { return "Mi Nuevo Mod"; } }
    public string ModDescription { get { return "Descripción breve del mod."; } }
    public string ModVersion { get { return "1.0.0"; } }

    private void Start()
    {
        // Se registra en el Core
        ModRegistry.Register(this);
    }

    private void OnDestroy()
    {
        ModRegistry.Unregister(this);
    }

    // Dibujado dentro de la ventana de MadIslandModManager
    public void OnDrawUI(Rect area)
    {
        GUILayout.Label("Contenido del Mod");
    }

    // Dibujado independiente si el usuario NO tiene instalado el ModManager
    private void OnGUI()
    {
        if (ModRegistry.IsModManagerActive) return; // Si el manager está activo, ocultar botón standalone

        if (GUI.Button(new Rect(Screen.width - 80, 70, 70, 25), "[MiMod]"))
        {
            // abrir ventana standalone
        }
    }
}
```

---

## 6. Rendimiento y Buenas Prácticas IMGUI

* Cachear siempre los `GUIStyle` en un método `InitStyles()` ejecutado una sola vez; nunca crear `new GUIStyle(...)` en cada frame dentro de `OnGUI()`.
* Usar `GUILayout.BeginScrollView` con un `Vector2` para listados largos (NPCs, misiones, inventario).
* Al hacer ventanas modales flotantes en IMGUI, colocar un overlay semitransparente con `GUI.Box(new Rect(0, 0, Screen.width, Screen.height), ...)` para bloquear clics en la interfaz trasera.
