# 📜 Mad Island - Reiniciador de Misiones (Quest Reset v1.1.0)

[Español](#español) | [English](#english)

---

## Español

Una herramienta avanzada para reiniciar misiones en **Mad Island**, incorporando un sistema de **Arcos Narrativos Unificados** que previene inconsistencias y bugs al reiniciar historias entrelazadas, además de ofrecer acceso a las **38 sub-misiones individuales** del juego.

### 🌟 Novedad v1.1.0: Sistema de Arcos Narrativos Unificados
En Mad Island, muchas misiones están compuestas por múltiples elecciones, cinemáticas y personajes interconectados. Si se reinicia solo una parte de la historia (por ejemplo, reiniciar a Keigo sin reiniciar a Reika), el juego puede corromper las variables de afecto o dejar eventos a medio completar. 

Por ello, el mod ahora incluye dos modos de visualización:

1. **🌟 Arcos Narrativos Unificados (Recomendado y Seguro):**
   * **👥 Arco: Keigo & Reika (El Triángulo de la Playa):** Unifica `Main_Reika`, `Main_Reika1`, `Sub_Keigo` y `Sub_KeigoPendant`. Reinicia de forma conjunta toda la historia de Reika y Keigo desde su llegada a la orilla, protegiendo las cinemáticas de afecto, el rescate y el colgante.
   * **🥥 Arco: Takumi (El Náufrago Hambriento):** Unifica `Main_Takumi`, `Main_Takumi1` y `Main_TakumiFeed` (encuentro, búsqueda de provisiones y evento de alimentación).
   * **🌲 Saga: Árbol Sagrado (Rey Ent & Reina Ent):** Unifica `Main_Entking`, `Main_Entqueen` y `Main_Entking2`. Como la Fase 2 requiere vencer a la Reina, reiniciar la saga completa previene que el bosque quede bloqueado o los puentes rotos.
   * **⛩️ La Gran Puerta Ancestral (Main Gate):** Vuelve a sellar las puertas de la Gran Puerta, reactiva el pedestal de las piedras/cristales sagrados y desactiva el portal warp. *(Nota: Los 3 jefes que custodian los cristales son misiones independientes y se pueden reiniciar libremente en la sección de Jefes).*
   * **🔬 Arco: Laboratorio 2 y Rescate de Cassie:** Unifica `Sub_Labo2Door`, `Sub_Cassie` y `Boss_Daruman`.
   * **🎀 Arco: Rescate de Kana & Lulu:** Unifica `Sub_Kana` y `Sub_Lulu`.
   * **🌊 Arco: El Hombre de la Ola & Yona:** Unifica `Sub_NamiMan`, `Sub_NamiYona` y `Main_Yona`.
   * **Jefes y Secundarias Independientes:** Araña, General, Monstruo Planta, Guadaña, Cuello Largo, Asaltante, Doctor, Gorila, Cazador, Niño, Sirena, Gigante, Shino, Prisión, Santa, etc.

2. **📋 Lista de 38 Sub-Misiones (Avanzado):**
   * Vista granular para jugadores que deseen inspeccionar o ajustar cada clave interna de forma individual. Si una misión forma parte de una cadena, se muestra una insignia que indica a qué arco pertenece con su correspondiente advertencia.

### 🔄 Reactivación Real en el Mundo de Juego
* **Jefes y Spawners:** Revive a los enemigos (`CommonStates.dead = 0`, vida completa) y reactiva `EnemySpawner`.
* **Obstáculos:** Desactiva bloqueos de victoria y restablece puertas/bloques de entrada.
* **Íconos del Mapa:** Restaura los símbolos de misión en el mapa del juego reactivando `EventStarter` y su marcador visual (`mapFade`).
* **Sincronización Permanente:** Actualiza `StoryManager`, `SaveManager` y los archivos `SaveData*.xml` en disco.
* **Limpieza de HUD:** Remueve el aviso de misión completada con `UIManager.DeleteQuestByName()`.

### ⚠️ Advertencia y Modal de Confirmación
Al reiniciar cualquier arco o misión se despliega una ventana de confirmación detallando:
* Las claves internas específicas que serán reajustadas.
* Advertencia sobre la acción permanente en los guardados.
* Riesgo de duplicación de NPCs únicos o recompensas si se repiten misiones ya concluidas.

### 🔄 Modo Dual (Independiente o Integrado)
* **Con MadIslandModManager:** Se integra automáticamente dentro del menú unificado **`[ MODS ]`**.
* **Sin ModManager (Standalone):** Muestra un botón flotante **`[Misiones]`** en la esquina superior derecha.

---

## English

An advanced quest resetting tool for **Mad Island**, featuring **Unified Storyline Arcs** to prevent story bugs and state desynchronization when resetting intertwined character quests, alongside granular access to all **38 individual sub-quests**.

### 🌟 What's New in v1.1.0: Unified Storyline Arcs
Many quests in Mad Island feature branching choices, cutscenes, and connected NPCs. Resetting only one piece of an intertwined story (such as resetting Keigo while leaving Reika completed) can desync affection flags or break quest flow.

The mod now provides two distinct views:

1. **🌟 Unified Storyline Arcs (Safe & Recommended):**
   * **👥 Keigo & Reika Arc:** Combines `Main_Reika`, `Main_Reika1`, `Sub_Keigo`, and `Sub_KeigoPendant`. Atomically resets the shared storyline from their beach arrival, preserving affection cutscenes and the pendant event.
   * **🥥 Takumi Arc:** Combines `Main_Takumi`, `Main_Takumi1`, and `Main_TakumiFeed` (initial rescue, supplies, and feeding).
   * **🌲 Sacred Tree Saga (Entking & Entqueen):** Combines `Main_Entking`, `Main_Entqueen`, and `Main_Entking2`. Because Phase 2 strictly depends on defeating the Queen, resetting the entire saga avoids map softlocks or missing bridges.
   * **⛩️ The Great Gate (Main Gate):** Re-locks the Great Gate doors, restores the sacred stone/crystal pedestal, and closes the warp portal. *(Note: The 3 bosses guarding the crystals remain independent and can be re-fought via the Bosses section).*
   * **🔬 Lab 2 & Cassie Rescue:** Combines `Sub_Labo2Door`, `Sub_Cassie`, and `Boss_Daruman`.
   * **🎀 Kana & Lulu Rescue:** Combines `Sub_Kana` and `Sub_Lulu`.
   * **🌊 The Wave Man & Yona:** Combines `Sub_NamiMan`, `Sub_NamiYona`, and `Main_Yona`.
   * **Independent Bosses & Sidequests:** Cave Spider, Fort General, Planton, Scythe, Necks, Raider, Mad Doctor, Gorilla, Hunter, Lost Child, Mermaid, Giant, Shino, Prison, Santa, etc.

2. **📋 Full 38 Sub-Quest List (Advanced):**
   * Granular list for players wanting full control over each internal quest key, labeled with badges indicating which storyline each quest belongs to.

### 🔄 In-Game World Reactivation
* **Bosses & Spawners:** Restores boss health (`dead = 0`, full HP) and reactivates `EnemySpawner`.
* **Barriers:** Removes victory barriers and restores entrance gates/blocks.
* **Map Icons:** Restores map symbols via `EventStarter` and `mapFade`.
* **Permanent Save Sync:** Updates runtime memory and writes directly to disk (`SaveData*.xml`).

---

## 🛠️ Compilación / Build
```powershell
./build.ps1
```

## 📄 Licencia / License
MIT License.
