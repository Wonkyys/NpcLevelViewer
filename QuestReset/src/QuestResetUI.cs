using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MadIslandModCore;

namespace QuestReset
{
    public class QuestGroupDef
    {
        public string GroupId;
        public string Title;
        public string Subtitle;
        public string Category; // "Story", "Boss", "Sub"
        public string Description;
        public string WarningNote;
        public string[] QuestKeys;

        public QuestGroupDef(string id, string title, string sub, string cat, string desc, string warn, string[] keys)
        {
            GroupId = id;
            Title = title;
            Subtitle = sub;
            Category = cat;
            Description = desc;
            WarningNote = warn;
            QuestKeys = keys;
        }
    }

    public class QuestResetUI : MonoBehaviour, IModModule
    {
        public static QuestResetUI Instance { get; private set; }

        // ── IModModule Properties ──────────────────────────────────────────────────
        public string ModId { get { return "quest_reset"; } }
        public string ModName { get { return "📜 Reiniciador de Misiones (Quest Reset)"; } }
        public string ModDescription { get { return "Reinicia arcos narrativos unificados (Keigo & Reika, Ents, Gate) o las 38 misiones individuales con restauracion de triggers y mapa."; } }
        public string ModVersion { get { return "1.1.0"; } }

        public void OnDrawUI(Rect area)
        {
            InitStyles();
            DrawContent();

            if (showConfirmModal)
            {
                DrawConfirmModalOverlay();
            }
        }

        // ── Standalone Mode ────────────────────────────────────────────────────────
        public bool IsVisible = false;
        private Rect windowRect = new Rect(40f, 35f, 920f, 650f);
        private Vector2 scrollPos = Vector2.zero;
        private string searchQuery = "";
        private int filterCategory = 0; // 0=All, 1=Story, 2=Bosses, 3=Sub, 4=Completed/InProgress
        private int viewMode = 0;       // 0=Arcos Unificados (Recomendado), 1=Misiones Individuales (Avanzado)

        // Modal fields
        private bool showConfirmModal = false;
        private string pendingResetTitle = "";
        private string pendingResetWarn = "";
        private List<string> pendingResetKeys = new List<string>();
        private StoryManager pendingStory = null;
        private SaveManager pendingSave = null;
        private Rect confirmRect = new Rect(0, 0, 580f, 380f);

        // Feedback notice
        private string _lastResetTitle = "";
        private int _lastResetCount = 0;
        private bool _showResetNotice = false;
        private float _resetNoticeTime = 0f;

        // Custom GUI styles
        private GUIStyle styleWin;
        private GUIStyle styleHeader;
        private GUIStyle styleTip;
        private GUIStyle styleQuestName;
        private GUIStyle styleQuestStatus;
        private GUIStyle styleBtnReset;
        private GUIStyle styleBtnAlready;
        private GUIStyle styleModalTitle;
        private GUIStyle styleModalBody;
        private GUIStyle styleCardBox;
        private GUIStyle styleCardTitle;
        private GUIStyle styleCardSub;
        private GUIStyle styleCardDesc;
        private GUIStyle styleTag;
        private bool stylesReady = false;

        // ── Unified Quest Groups (Arcos Narrativos) ────────────────────────────────
        public static readonly QuestGroupDef[] AllGroups = new QuestGroupDef[]
        {
            // ── Arcos de Historia Principales (Unificados) ──
            new QuestGroupDef(
                "keigo_reika",
                "👥 Arco: Keigo & Reika (El Triangulo de la Playa)",
                "Misiones agrupadas: Main_Reika, Main_Reika1, Sub_Keigo, Sub_KeigoPendant",
                "Story",
                "Historia compartida entre Reika y Keigo desde su llegada a la playa. Incluye el primer encuentro, busqueda de provisiones, cinematica de afecto y el evento del colgante.",
                "⚠️ Ambos personajes estan entrelazados en la misma trama. Al reiniciarlos juntos se garantiza que sus cinematicas de afecto, rescate y opciones romanticas vuelvan limpiamente al inicio sin corromper flags de guardado.",
                new string[] { "Main_Reika", "Main_Reika1", "Sub_Keigo", "Sub_KeigoPendant" }
            ),
            new QuestGroupDef(
                "takumi",
                "🥥 Arco: Takumi (El Naufrago Hambriento)",
                "Misiones agrupadas: Main_Takumi, Main_Takumi1, Main_TakumiFeed",
                "Story",
                "Las 3 etapas consecutivas de Takumi: encuentro en la orilla herido, peticiones de provisiones y evento de alimentacion para desbloquear accesos.",
                "Reinicia las 3 fases de Takumi desde cero de forma sincronizada.",
                new string[] { "Main_Takumi", "Main_Takumi1", "Main_TakumiFeed" }
            ),
            new QuestGroupDef(
                "ent_tree",
                "🌲 Saga: Arbol Sagrado (Rey Ent & Reina Ent)",
                "Misiones agrupadas: Main_Entking, Main_Entqueen, Main_Entking2",
                "Story",
                "Gran saga del bosque sagrado: combate contra el Rey Ent (Fase 1), descenso al pantano contra la Reina Ent y batalla final en la Fase 2 con puente de corteza.",
                "⚠️ La Fase 2 del Rey Ent requiere la derrota previa de la Reina. Reiniciar la saga completa previene bloqueos de mapa, puentes rotos o cinemáticas colgadas.",
                new string[] { "Main_Entking", "Main_Entqueen", "Main_Entking2" }
            ),
            new QuestGroupDef(
                "main_gate",
                "⛩️ La Gran Puerta Ancestral (Main Gate)",
                "Mision principal: Main_Gate",
                "Story",
                "Cierra las puertas de la Gran Puerta, reactiva el pedestal de las piedras/cristales sagrados y desactiva el portal de teletransporte.",
                "La Gran Puerta volvera a cerrarse y requerira los cristales para abrirse. (Los 3 jefes que custodian los cristales se pueden reiniciar independientemente en la seccion de Jefes).",
                new string[] { "Main_Gate" }
            ),
            new QuestGroupDef(
                "cassie_labo",
                "🔬 Arco: Laboratorio 2 y Rescate de Cassie",
                "Misiones agrupadas: Sub_Labo2Door, Sub_Cassie, Boss_Daruman",
                "Story",
                "Apertura de la puerta del Laboratorio 2, rescate de la investigadora Cassie y combate contra la aberracion biológica Daruman.",
                "Reinicia el acceso sellado a las instalaciones, el rescate de Cassie y el combate con Daruman.",
                new string[] { "Sub_Labo2Door", "Sub_Cassie", "Boss_Daruman" }
            ),
            new QuestGroupDef(
                "kana_lulu",
                "🎀 Arco: Rescate de Kana & Lulu",
                "Misiones agrupadas: Sub_Kana, Sub_Lulu",
                "Sub",
                "Liberacion de Kana tras la batalla en el Fuerte del General y sus posteriores interacciones y eventos compartidos con Lulu.",
                "Reinicia las cinematicas de rescate e interaccion de ambas supervivientes.",
                new string[] { "Sub_Kana", "Sub_Lulu" }
            ),
            new QuestGroupDef(
                "nami_yona",
                "🌊 Arco: El Hombre de la Ola & Yona",
                "Misiones agrupadas: Sub_NamiMan, Sub_NamiYona, Main_Yona",
                "Sub",
                "Misiones costeras del naufrago misterioso (Nami) y los eventos y conversaciones de Yona en la isla.",
                "Restaura los triggers costeros y las conversaciones de Yona.",
                new string[] { "Sub_NamiMan", "Sub_NamiYona", "Main_Yona" }
            ),
            new QuestGroupDef(
                "main_bridge",
                "🌉 Mision: Construccion del Puente Principal",
                "Mision principal: Main_Bridge",
                "Story",
                "Construccion y paso a traves del gran puente hacia las tierras altas de la isla.",
                "Vuelve a requerir la reparacion/construccion del puente para cruzar.",
                new string[] { "Main_Bridge" }
            ),

            // ── Jefes Individuales de la Isla ──
            new QuestGroupDef("boss_spider",   "🕷️ Jefe: Arana de la Cueva", "Boss_Spider", "Boss", "Monstruo de las profundidades de la cueva que custodia uno de los cristales.", "Revive a la araña, restablece el spawner y reabre el evento.", new string[] { "Boss_Spider" }),
            new QuestGroupDef("boss_gen",      "⚔️ Jefe: General del Fuerte", "Boss_Gen", "Boss", "Comandante del fuerte militar en la isla.", "Revive al General, reactive el spawner de soldados y reabre la arena.", new string[] { "Boss_Gen" }),
            new QuestGroupDef("boss_planton",  "🌱 Jefe: Monstruo Planta (Planton)", "Boss_Planton", "Boss", "Enorme criatura vegetal carnivora en la selva.", "Revive al monstruo planta y restablece su area.", new string[] { "Boss_Planton" }),
            new QuestGroupDef("boss_scythe",   "💀 Jefe: Guadana Mortal (Scythe)", "Boss_Scythe", "Boss", "Espectro armado con una guadaña gigante.", "Revive al jefe guadaña y reactiva el trigger de batalla.", new string[] { "Boss_Scythe" }),
            new QuestGroupDef("boss_necks",    "🦕 Jefe: Cuello Largo (Necks)", "Boss_Necks", "Boss", "Bestia colosal de las marismas.", "Revive al cuello largo y reactiva su evento.", new string[] { "Boss_Necks" }),
            new QuestGroupDef("boss_rapesman", "🔥 Jefe: Asaltante de la Selva", "Boss_Rapesman", "Boss", "Lider de los bandidos salvajes en su campamento.", "Revive al asaltante y restablece el campamento.", new string[] { "Boss_Rapesman" }),
            new QuestGroupDef("boss_doctor",   "🧪 Jefe: Doctor Loco", "Boss_Doctor", "Boss", "Cientifico loco en la entrada del laboratorio.", "Revive al Doctor y restablece los bloques de contencion.", new string[] { "Boss_Doctor" }),
            new QuestGroupDef("boss_gorilla",  "🦍 Jefe: Gorila Gigante", "Boss_Gorilla", "Boss", "Gorila colosal de la cueva corrupta.", "Revive al gorila y reactiva sus spawners.", new string[] { "Boss_Gorilla" }),
            new QuestGroupDef("boss_hunter",   "🏹 Jefe: Cazador Furtivo", "Boss_Hunter", "Boss", "Cazador implacable en el bosque oriental.", "Revive al cazador y sus trampas.", new string[] { "Boss_Hunter" }),

            // ── Misiones Secundarias Individuales ──
            new QuestGroupDef("sub_little",    "👶 Rescate del Nino Perdido", "Sub_Little", "Sub", "Mision de busqueda y rescate del pequeno naufrago.", "Restaura la mision de rescate del nino.", new string[] { "Sub_Little" }),
            new QuestGroupDef("sub_ruinsdoor", "🏛️ Puerta Sellada de las Ruinas", "Sub_RuinsDoor", "Sub", "Mecanismo antiguo que sella las ruinas de piedra.", "Vuelve a cerrar la puerta de las ruinas.", new string[] { "Sub_RuinsDoor" }),
            new QuestGroupDef("sub_mermaid",   "🧜‍♀️ La Sirena Encallada", "Sub_Mermaid", "Sub", "Encuentro y asistencia a la sirena en la playa.", "Reactiva la cinematica y evento de la sirena.", new string[] { "Sub_Mermaid" }),
            new QuestGroupDef("sub_freebase",  "🚩 Liberacion de la Base Rebelde", "Sub_FreeBase", "Sub", "Toma del puesto militar y liberacion de la zona.", "Restablece el control enemigo en la base.", new string[] { "Sub_FreeBase" }),
            new QuestGroupDef("sub_giant",     "🗿 El Gigante Solitario", "Sub_Giant", "Sub", "El misterioso gigante que vigila las rocas.", "Reactiva el encuentro y las cinematicas del gigante.", new string[] { "Sub_Giant" }),
            new QuestGroupDef("sub_shino",     "🌸 Historia de Shino", "Sub_Shino", "Sub", "Cadena de dialogos y eventos personales con Shino.", "Reinicia los eventos de relacion con Shino.", new string[] { "Sub_Shino" }),
            new QuestGroupDef("sub_prison",    "⛓️ Prision Subterranea Corrupta", "Sub_Prison", "Sub", "Infiltracion en la prision de las profundidades.", "Vuelve a bloquear los accesos de la prision.", new string[] { "Sub_Prison" }),
            new QuestGroupDef("sub_santa",     "🎄 Evento Especial: Santa & Tonakai", "Sub_Santa", "Sub", "Evento tematico especial en la isla.", "Reactiva el evento festivo de Santa.", new string[] { "Sub_Santa" }),
            new QuestGroupDef("sub_death",     "⚠️ Encuentro Mortal", "Sub_Death", "Sub", "Cinematica y suceso de peligro extremo.", "Reactiva el evento de peligro.", new string[] { "Sub_Death" })
        };

        // ── Raw 38 Quests (For Advanced View) ──────────────────────────────────────
        private static readonly string[][] AllQuests = new string[][]
        {
            new string[] { "Boss_Spider",      "Jefe: Arana de la Cueva",           "Boss", "boss_spider" },
            new string[] { "Boss_Gen",         "Jefe: General del Fuerte",           "Boss", "boss_gen" },
            new string[] { "Boss_Planton",     "Jefe: Monstruo Planta",              "Boss", "boss_planton" },
            new string[] { "Boss_Scythe",      "Jefe: Guadana Mortal",               "Boss", "boss_scythe" },
            new string[] { "Boss_Necks",       "Jefe: Cuello Largo",                 "Boss", "boss_necks" },
            new string[] { "Boss_Rapesman",    "Jefe: Asaltante de la Selva",        "Boss", "boss_rapesman" },
            new string[] { "Boss_Doctor",      "Jefe: Doctor Loco",                  "Boss", "boss_doctor" },
            new string[] { "Boss_Daruman",     "Jefe: Daruman",                      "Boss", "cassie_labo" },
            new string[] { "Boss_Gorilla",     "Jefe: Gorila Gigante",               "Boss", "boss_gorilla" },
            new string[] { "Boss_Hunter",      "Jefe: Cazador Furtivo",              "Boss", "boss_hunter" },
            new string[] { "Main_Gate",        "Principal: Gran Puerta (Gate)",      "Main", "main_gate" },
            new string[] { "Main_Bridge",      "Principal: Puente Principal",        "Main", "main_bridge" },
            new string[] { "Main_Reika",       "Principal: Historia de Reika 1",     "Main", "keigo_reika" },
            new string[] { "Main_Reika1",      "Principal: Historia de Reika 2",     "Main", "keigo_reika" },
            new string[] { "Main_Takumi",      "Principal: Encuentro con Takumi",    "Main", "takumi" },
            new string[] { "Main_Takumi1",     "Principal: Mision de Takumi 1",      "Main", "takumi" },
            new string[] { "Main_TakumiFeed",  "Principal: Alimentar a Takumi",      "Main", "takumi" },
            new string[] { "Main_Entking",     "Principal: Rey Ent (Entking)",       "Main", "ent_tree" },
            new string[] { "Main_Entqueen",    "Principal: Reina Ent (Entqueen)",    "Main", "ent_tree" },
            new string[] { "Main_Entking2",    "Principal: Rey Ent Fase 2",          "Main", "ent_tree" },
            new string[] { "Main_Yona",        "Principal: Historia de Yona",        "Main", "nami_yona" },
            new string[] { "Sub_Little",       "Secundaria: Rescate del Nino",       "Sub",  "sub_little" },
            new string[] { "Sub_RuinsDoor",    "Secundaria: Puerta de las Ruinas",   "Sub",  "sub_ruinsdoor" },
            new string[] { "Sub_Mermaid",      "Secundaria: La Sirena",              "Sub",  "sub_mermaid" },
            new string[] { "Sub_Cassie",       "Secundaria: Historia de Cassie",     "Sub",  "cassie_labo" },
            new string[] { "Sub_FreeBase",     "Secundaria: Liberar Base",           "Sub",  "sub_freebase" },
            new string[] { "Sub_Giant",        "Secundaria: Gigante Solitario",      "Sub",  "sub_giant" },
            new string[] { "Sub_Labo2Door",    "Secundaria: Puerta Laboratorio 2",   "Sub",  "cassie_labo" },
            new string[] { "Sub_Keigo",        "Secundaria: Mision de Keigo",        "Sub",  "keigo_reika" },
            new string[] { "Sub_KeigoPendant", "Secundaria: Colgante de Keigo",      "Sub",  "keigo_reika" },
            new string[] { "Sub_Shino",        "Secundaria: Historia de Shino",      "Sub",  "sub_shino" },
            new string[] { "Sub_Prison",       "Secundaria: Prision Subterranea",    "Sub",  "sub_prison" },
            new string[] { "Sub_Santa",        "Secundaria: Evento Santa",           "Sub",  "sub_santa" },
            new string[] { "Sub_NamiMan",      "Secundaria: El Hombre de la Ola",    "Sub",  "nami_yona" },
            new string[] { "Sub_NamiYona",     "Secundaria: Ola y Yona",             "Sub",  "nami_yona" },
            new string[] { "Sub_Death",        "Secundaria: Encuentro Mortal",       "Sub",  "sub_death" },
            new string[] { "Sub_Kana",         "Secundaria: Historia de Kana",       "Sub",  "kana_lulu" },
            new string[] { "Sub_Lulu",         "Secundaria: Historia de Lulu",       "Sub",  "kana_lulu" }
        };

        // Exact folder and trigger names inside GameObject.Find("Events")
        private static readonly Dictionary<string, string[]> QuestTriggerMap = new Dictionary<string, string[]>()
        {
            { "Boss_Spider",      new string[] { "Boss_Spider", "event_boss_spider", "stage_cave01" } },
            { "Boss_Gen",         new string[] { "Boss_Gen", "event_boss_gen", "st_gen_boss" } },
            { "Boss_Planton",     new string[] { "Boss_Planton", "event_boss_planton", "" } },
            { "Boss_Scythe",      new string[] { "Boss_Scythe", "event_boss_scythe", "" } },
            { "Boss_Necks",       new string[] { "Boss_Necks", "event_boss_necks", "" } },
            { "Boss_Doctor",      new string[] { "Boss_Doctor", "event_doctor", "st_doctor" } },
            { "Boss_Daruman",     new string[] { "Boss_Daruman", "event_daruman", "stage_labo02" } },
            { "Boss_Gorilla",     new string[] { "Boss_Gorilla", "event_boss_gorilla", "" } },
            { "Boss_Hunter",      new string[] { "Boss_Hunter", "event_boss_hunter", "st_hunter" } },
            { "Boss_Rapesman",    new string[] { "Boss_Rapesman", "event_boss_rapesman", "" } },
            { "Main_Gate",        new string[] { "Main_Gate", "event_gate", "st_gate_01" } },
            { "Main_Bridge",      new string[] { "Main_Bridge", "event_bridge", "" } },
            { "Main_Reika",       new string[] { "Main_Reika", "event_main_reika", "" } },
            { "Main_Reika1",      new string[] { "Main_Reika1", "event_main_reika1", "" } },
            { "Main_Takumi",      new string[] { "Main_Takumi", "event_main_takumi", "" } },
            { "Main_Takumi1",     new string[] { "Main_Takumi1", "event_main_takumi1", "" } },
            { "Main_TakumiFeed",  new string[] { "Main_TakumiFeed", "event_main_takumifeed", "" } },
            { "Main_Entking",     new string[] { "Main_Entking", "event_boss_entking", "st_dark_03" } },
            { "Main_Entqueen",    new string[] { "Main_Entqueen", "event_entqueen", "st_entqueen_01" } },
            { "Main_Entking2",    new string[] { "Main_Entking2", "event_entking02", "" } },
            { "Main_Yona",        new string[] { "Main_Yona", "event_yona", "" } },
            { "Sub_Little",       new string[] { "Sub_Little", "event_little", "" } },
            { "Sub_RuinsDoor",    new string[] { "Sub_RuinsDoor", "event_ruinsdoor", "st_ruins_01" } },
            { "Sub_Mermaid",      new string[] { "Sub_Mermaid", "event_mermaid", "st_mermaid" } },
            { "Sub_Cassie",       new string[] { "Sub_Cassie", "event_cassie", "" } },
            { "Sub_FreeBase",     new string[] { "Sub_FreeBase", "event_freebase", "" } },
            { "Sub_Giant",        new string[] { "Sub_Giant", "event_giant", "" } },
            { "Sub_Labo2Door",    new string[] { "Sub_Labo2Door", "event_labo2door", "stage_labo01" } },
            { "Sub_Keigo",        new string[] { "Sub_Keigo", "event_keigo", "" } },
            { "Sub_KeigoPendant", new string[] { "Sub_Keigo1", "event_keigo1", "" } },
            { "Sub_Shino",        new string[] { "Sub_Shino", "event_shino", "" } },
            { "Sub_Prison",       new string[] { "Sub_Prison", "event_prison", "stage_cave_corrupt" } },
            { "Sub_Santa",        new string[] { "Sub_Santa", "event_santa", "" } },
            { "Sub_NamiMan",      new string[] { "Sub_Nami", "event_nami", "" } },
            { "Sub_NamiYona",     new string[] { "Sub_Nami", "event_namiyona", "" } },
            { "Sub_Death",        new string[] { "Sub_Death", "event_death", "" } },
            { "Sub_Kana",         new string[] { "Sub_Kana", "event_kana", "" } },
            { "Sub_Lulu",         new string[] { "Sub_Lulu", "event_lulu", "" } }
        };

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            ModRegistry.Register(this);
        }

        private void OnDestroy()
        {
            ModRegistry.Unregister(this);
        }

        private void OnGUI()
        {
            if (ModRegistry.IsModManagerActive) return;

            float bW = 80f, bH = 26f;
            Color prevBg = GUI.backgroundColor;
            GUI.backgroundColor = IsVisible ? new Color(0.2f, 0.7f, 0.3f) : new Color(0.75f, 0.35f, 0.15f);
            if (GUI.Button(new Rect(Screen.width - bW - 10f, 40f, bW, bH), "[Misiones]"))
            {
                IsVisible = !IsVisible;
            }
            GUI.backgroundColor = prevBg;

            if (!IsVisible) return;

            InitStyles();
            GUI.backgroundColor = new Color(0.10f, 0.12f, 0.16f, 0.97f);
            windowRect = GUI.Window(987201, windowRect, DrawStandaloneWindow, "Mad Island | Reiniciador de Misiones v1.1");

            if (showConfirmModal)
            {
                DrawConfirmModalOverlay();
            }
        }

        private void DrawStandaloneWindow(int id)
        {
            GUI.DragWindow(new Rect(0, 0, windowRect.width - 40f, 24f));

            if (GUI.Button(new Rect(windowRect.width - 32f, 4f, 26f, 20f), "X"))
            {
                IsVisible = false;
                return;
            }

            DrawContent();
        }

        private void InitStyles()
        {
            if (stylesReady) return;

            styleWin = new GUIStyle(GUI.skin.window);
            styleWin.fontSize = 13;

            styleHeader = new GUIStyle(GUI.skin.label);
            styleHeader.fontSize = 12;
            styleHeader.fontStyle = FontStyle.Bold;
            styleHeader.normal.textColor = Color.white;
            styleHeader.richText = true;

            styleTip = new GUIStyle(GUI.skin.label);
            styleTip.fontSize = 11;
            styleTip.normal.textColor = new Color(0.7f, 0.82f, 1f);
            styleTip.richText = true;

            styleQuestName = new GUIStyle(GUI.skin.label);
            styleQuestName.fontSize = 12;
            styleQuestName.normal.textColor = Color.white;
            styleQuestName.richText = true;
            styleQuestName.alignment = TextAnchor.MiddleLeft;

            styleQuestStatus = new GUIStyle(GUI.skin.label);
            styleQuestStatus.fontSize = 11;
            styleQuestStatus.richText = true;
            styleQuestStatus.alignment = TextAnchor.MiddleLeft;
            styleQuestStatus.normal.textColor = Color.white;

            styleBtnReset = new GUIStyle(GUI.skin.button);
            styleBtnReset.fontSize = 11;
            styleBtnReset.fontStyle = FontStyle.Bold;
            styleBtnReset.normal.textColor = Color.white;

            styleBtnAlready = new GUIStyle(GUI.skin.button);
            styleBtnAlready.fontSize = 11;
            styleBtnAlready.normal.textColor = new Color(0.55f, 0.55f, 0.55f);

            styleModalTitle = new GUIStyle(GUI.skin.label);
            styleModalTitle.fontSize = 13;
            styleModalTitle.fontStyle = FontStyle.Bold;
            styleModalTitle.normal.textColor = Color.white;
            styleModalTitle.richText = true;

            styleModalBody = new GUIStyle(GUI.skin.label);
            styleModalBody.fontSize = 11;
            styleModalBody.normal.textColor = new Color(0.92f, 0.92f, 0.92f);
            styleModalBody.richText = true;
            styleModalBody.wordWrap = true;

            styleCardBox = new GUIStyle(GUI.skin.box);
            styleCardBox.padding = new RectOffset(10, 10, 8, 8);

            styleCardTitle = new GUIStyle(GUI.skin.label);
            styleCardTitle.fontSize = 13;
            styleCardTitle.fontStyle = FontStyle.Bold;
            styleCardTitle.normal.textColor = new Color(1f, 0.88f, 0.35f);
            styleCardTitle.richText = true;

            styleCardSub = new GUIStyle(GUI.skin.label);
            styleCardSub.fontSize = 11;
            styleCardSub.normal.textColor = new Color(0.65f, 0.80f, 0.95f);
            styleCardSub.richText = true;

            styleCardDesc = new GUIStyle(GUI.skin.label);
            styleCardDesc.fontSize = 11;
            styleCardDesc.normal.textColor = new Color(0.85f, 0.85f, 0.85f);
            styleCardDesc.richText = true;
            styleCardDesc.wordWrap = true;

            styleTag = new GUIStyle(GUI.skin.label);
            styleTag.fontSize = 10;
            styleTag.fontStyle = FontStyle.Bold;
            styleTag.richText = true;

            stylesReady = true;
        }

        public void DrawContent()
        {
            var mn = GameManagers.Managers;
            var story = mn != null ? mn.story : null;
            var save  = mn != null ? mn.save  : null;

            GUILayout.BeginVertical();

            // Row 1: View Mode Switcher
            GUILayout.BeginHorizontal();
            {
                Color prevBtn = GUI.backgroundColor;
                GUI.backgroundColor = viewMode == 0 ? new Color(0.18f, 0.65f, 0.35f) : new Color(0.22f, 0.26f, 0.34f);
                if (GUILayout.Button("🌟 Arcos Narrativos Unificados (Seguro y Recomendado)", GUILayout.Height(28f)))
                {
                    viewMode = 0;
                }

                GUI.backgroundColor = viewMode == 1 ? new Color(0.18f, 0.65f, 0.35f) : new Color(0.22f, 0.26f, 0.34f);
                if (GUILayout.Button("📋 Lista de 38 Sub-Misiones (Avanzado)", GUILayout.Height(28f), GUILayout.Width(280f)))
                {
                    viewMode = 1;
                }
                GUI.backgroundColor = prevBtn;
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4f);

            // Row 2: Search & Category Filters
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("<b>Buscar:</b>", styleHeader, GUILayout.Width(50f));
                searchQuery = GUILayout.TextField(searchQuery, GUILayout.Width(140f));

                GUILayout.Space(10f);
                Color prevC = GUI.backgroundColor;

                GUI.backgroundColor = filterCategory == 0 ? new Color(0.25f, 0.60f, 0.35f) : new Color(0.22f, 0.26f, 0.34f);
                if (GUILayout.Button("Todos", GUILayout.Width(65f))) filterCategory = 0;

                GUI.backgroundColor = filterCategory == 1 ? new Color(0.25f, 0.60f, 0.35f) : new Color(0.22f, 0.26f, 0.34f);
                if (GUILayout.Button("Historia", GUILayout.Width(75f))) filterCategory = 1;

                GUI.backgroundColor = filterCategory == 2 ? new Color(0.25f, 0.60f, 0.35f) : new Color(0.22f, 0.26f, 0.34f);
                if (GUILayout.Button("Jefes", GUILayout.Width(65f))) filterCategory = 2;

                GUI.backgroundColor = filterCategory == 3 ? new Color(0.25f, 0.60f, 0.35f) : new Color(0.22f, 0.26f, 0.34f);
                if (GUILayout.Button("Secundarias", GUILayout.Width(95f))) filterCategory = 3;

                GUI.backgroundColor = filterCategory == 4 ? new Color(0.25f, 0.60f, 0.35f) : new Color(0.22f, 0.26f, 0.34f);
                if (GUILayout.Button("Solo Activas / Hechas", GUILayout.Width(170f))) filterCategory = 4;

                GUI.backgroundColor = prevC;
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6f);

            // Content scroll area
            scrollPos = GUILayout.BeginScrollView(scrollPos);

            if (viewMode == 0)
            {
                DrawUnifiedGroupsView(story, save);
            }
            else
            {
                DrawRawQuestsView(story, save);
            }

            GUILayout.EndScrollView();

            // Footer notification
            GUILayout.Space(4f);
            GUILayout.BeginHorizontal();
            if (_showResetNotice && (Time.time - _resetNoticeTime) < 10f)
            {
                Color prevBgQ = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0.15f, 0.65f, 0.25f);
                GUILayout.Label(
                    string.Format("✅ ¡'{0}' reiniciado con exito! ({1} misiones reajustadas a 0 en memoria y guardado XML).", _lastResetTitle, _lastResetCount),
                    styleTip, GUILayout.Height(24f));
                GUI.backgroundColor = prevBgQ;
            }
            else
            {
                _showResetNotice = false;
                GUILayout.Label("💡 Consejo: Los arcos unificados reinician cinematicas, decisiones y personajes simultaneamente para evitar inconsistencias en la partida.", styleTip);
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void DrawUnifiedGroupsView(StoryManager story, SaveManager save)
        {
            int shownCount = 0;

            for (int i = 0; i < AllGroups.Length; i++)
            {
                var grp = AllGroups[i];

                // Filters
                if (filterCategory == 1 && grp.Category != "Story") continue;
                if (filterCategory == 2 && grp.Category != "Boss") continue;
                if (filterCategory == 3 && grp.Category != "Sub") continue;

                // Calculate progress across all keys in group
                int completedKeys = 0;
                int inProgressKeys = 0;
                for (int k = 0; k < grp.QuestKeys.Length; k++)
                {
                    int p = GetQuestProgress(grp.QuestKeys[k], save, story);
                    if (p >= 99) completedKeys++;
                    else if (p > 0) inProgressKeys++;
                }
                bool isAnyActive = (completedKeys > 0 || inProgressKeys > 0);

                if (filterCategory == 4 && !isAnyActive) continue;

                // Search
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    bool match = grp.Title.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                 grp.Description.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                 grp.Subtitle.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0;
                    if (!match) continue;
                }

                shownCount++;

                Color origBg = GUI.backgroundColor;
                GUI.backgroundColor = isAnyActive ? new Color(0.16f, 0.22f, 0.32f, 0.94f) : new Color(0.13f, 0.15f, 0.19f, 0.88f);
                GUILayout.BeginVertical(styleCardBox);
                GUI.backgroundColor = origBg;

                GUILayout.BeginHorizontal();
                {
                    // Left: Title, Subtitle, Description
                    GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(string.Format("<b>{0}</b>", grp.Title), styleCardTitle);
                        GUILayout.FlexibleSpace();

                        string statusBadge;
                        if (!isAnyActive)
                            statusBadge = "<color=#888888>○ No iniciada</color>";
                        else if (completedKeys == grp.QuestKeys.Length)
                            statusBadge = string.Format("<color=#55FF88>● Completada ({0}/{1})</color>", completedKeys, grp.QuestKeys.Length);
                        else
                            statusBadge = string.Format("<color=#FFD700>● En curso ({0}/{1} listas)</color>", completedKeys, grp.QuestKeys.Length);

                        GUILayout.Label(statusBadge, styleQuestStatus, GUILayout.Height(22f));
                        GUILayout.EndHorizontal();

                        GUILayout.Label(string.Format("<color=#77AADD>{0}</color>", grp.Subtitle), styleCardSub);
                        GUILayout.Space(2f);
                        GUILayout.Label(grp.Description, styleCardDesc);
                    }
                    GUILayout.EndVertical();

                    GUILayout.Space(12f);

                    // Right: Action button
                    Color prevBtn = GUI.backgroundColor;
                    if (!isAnyActive)
                    {
                        GUI.backgroundColor = new Color(0.32f, 0.32f, 0.35f);
                        GUILayout.Button("Ya en 0", styleBtnAlready, GUILayout.Width(125f), GUILayout.Height(44f));
                    }
                    else
                    {
                        GUI.backgroundColor = new Color(0.82f, 0.24f, 0.15f);
                        if (GUILayout.Button("🔄 Reiniciar Arco", styleBtnReset, GUILayout.Width(125f), GUILayout.Height(44f)))
                        {
                            OpenConfirmModalForGroup(grp, story, save);
                        }
                    }
                    GUI.backgroundColor = prevBtn;
                }
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
                GUILayout.Space(4f);
            }

            if (shownCount == 0)
            {
                GUILayout.Space(25f);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("<color=#AAAAAA>No se encontraron arcos narrativos con los filtros seleccionados.</color>", styleHeader);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
        }

        private void DrawRawQuestsView(StoryManager story, SaveManager save)
        {
            // Table Header
            GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.Height(26f));
            {
                GUILayout.Label("#", styleHeader, GUILayout.Width(25f));
                GUILayout.Label("Clave Interna", styleHeader, GUILayout.Width(150f));
                GUILayout.Label("Nombre de la Sub-Mision", styleHeader, GUILayout.Width(220f));
                GUILayout.Label("Arco Vinculado", styleHeader, GUILayout.Width(170f));
                GUILayout.Label("Estado", styleHeader, GUILayout.Width(120f));
                GUILayout.Label("Accion", styleHeader, GUILayout.Width(110f));
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(2f);
            int shownCount = 0;

            for (int i = 0; i < AllQuests.Length; i++)
            {
                string key = AllQuests[i][0];
                string name = AllQuests[i][1];
                string cat = AllQuests[i][2];
                string grpId = AllQuests[i][3];

                int progress = GetQuestProgress(key, save, story);

                // Filters
                if (filterCategory == 1 && cat != "Main") continue;
                if (filterCategory == 2 && cat != "Boss") continue;
                if (filterCategory == 3 && cat != "Sub")  continue;
                if (filterCategory == 4 && progress <= 0) continue;

                // Search
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    bool match = key.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                 name.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0;
                    if (!match) continue;
                }

                shownCount++;

                Color origBg = GUI.backgroundColor;
                if (progress <= 0)
                    GUI.backgroundColor = new Color(0.16f, 0.18f, 0.22f, 0.85f);
                else if (progress >= 99)
                    GUI.backgroundColor = new Color(0.16f, 0.38f, 0.20f, 0.9f);
                else
                    GUI.backgroundColor = new Color(0.38f, 0.30f, 0.10f, 0.9f);

                GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.Height(32f));

                GUILayout.Label(shownCount.ToString(), styleQuestName, GUILayout.Width(25f), GUILayout.Height(26f));
                GUILayout.Label(key, styleQuestName, GUILayout.Width(150f), GUILayout.Height(26f));
                GUILayout.Label(name, styleQuestName, GUILayout.Width(220f), GUILayout.Height(26f));

                // Parent group badge
                QuestGroupDef parentGrp = FindGroupById(grpId);
                string groupBadge = parentGrp != null ? string.Format("<color=#88CCFF>{0}</color>", parentGrp.Title.Replace("👥 ", "").Replace("🥥 ", "").Replace("🌲 ", "").Replace("🔬 ", "").Replace("🎀 ", "").Replace("🌊 ", "").Replace("⛩️ ", "").Replace("🌉 ", "").Replace("🕷️ ", "").Replace("⚔️ ", "").Replace("🌱 ", "").Replace("💀 ", "").Replace("🦕 ", "").Replace("🔥 ", "").Replace("🧪 ", "").Replace("🦍 ", "").Replace("🏹 ", "").Replace("👶 ", "").Replace("🏛️ ", "").Replace("🧜‍♀️ ", "").Replace("🚩 ", "").Replace("🗿 ", "").Replace("🌸 ", "").Replace("⛓️ ", "").Replace("🎄 ", "").Replace("⚠️ ", "")) : "-";
                GUILayout.Label(groupBadge, styleQuestStatus, GUILayout.Width(170f), GUILayout.Height(26f));

                string statusTxt;
                if (progress <= 0)
                    statusTxt = "<color=#888888>0 (Inactivo)</color>";
                else if (progress >= 99)
                    statusTxt = string.Format("<color=#55FF55>Hecho ({0})</color>", progress);
                else
                    statusTxt = string.Format("<color=#FFD700>Progreso ({0})</color>", progress);

                GUILayout.Label(statusTxt, styleQuestStatus, GUILayout.Width(120f), GUILayout.Height(26f));

                GUI.backgroundColor = origBg;
                if (progress <= 0)
                {
                    GUI.backgroundColor = new Color(0.35f, 0.35f, 0.35f);
                    GUILayout.Button("En 0", styleBtnAlready, GUILayout.Width(110f), GUILayout.Height(26f));
                }
                else
                {
                    GUI.backgroundColor = new Color(0.80f, 0.25f, 0.15f);
                    if (GUILayout.Button("Reiniciar", styleBtnReset, GUILayout.Width(110f), GUILayout.Height(26f)))
                    {
                        OpenConfirmModalForSingle(key, name, parentGrp, story, save);
                    }
                }

                GUILayout.EndHorizontal();
                GUI.backgroundColor = origBg;
                GUILayout.Space(2f);
            }

            if (shownCount == 0)
            {
                GUILayout.Space(25f);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("<color=#AAAAAA>No se encontraron misiones individuales con los filtros seleccionados.</color>", styleHeader);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
        }

        private QuestGroupDef FindGroupById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            for (int i = 0; i < AllGroups.Length; i++)
            {
                if (AllGroups[i].GroupId == id) return AllGroups[i];
            }
            return null;
        }

        private void OpenConfirmModalForGroup(QuestGroupDef grp, StoryManager story, SaveManager save)
        {
            pendingResetTitle = grp.Title;
            pendingResetWarn = grp.WarningNote;
            pendingResetKeys.Clear();
            pendingResetKeys.AddRange(grp.QuestKeys);
            pendingStory = story;
            pendingSave = save;

            confirmRect = new Rect((Screen.width - 580f) / 2f, (Screen.height - 380f) / 2f, 580f, 380f);
            showConfirmModal = true;
        }

        private void OpenConfirmModalForSingle(string key, string dispName, QuestGroupDef parentGrp, StoryManager story, SaveManager save)
        {
            pendingResetTitle = string.Format("Mision Individual: {0} ({1})", dispName, key);
            if (parentGrp != null && parentGrp.QuestKeys.Length > 1)
            {
                pendingResetWarn = string.Format(
                    "⚠️ NOTA DE VINCULACION: Esta mision forma parte del arco '{0}'. Si solo reinicias esta sub-mision, las demas partes de la historia permaneceran en su estado actual, lo cual podria desincronizar dialogos o cinematicas vinculadas.",
                    parentGrp.Title);
            }
            else
            {
                pendingResetWarn = "Reiniciara el estado de la mision en memoria y archivo de guardado.";
            }

            pendingResetKeys.Clear();
            pendingResetKeys.Add(key);
            pendingStory = story;
            pendingSave = save;

            confirmRect = new Rect((Screen.width - 580f) / 2f, (Screen.height - 380f) / 2f, 580f, 380f);
            showConfirmModal = true;
        }

        private void DrawConfirmModalOverlay()
        {
            Color origBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.24f, 0.08f, 0.08f, 0.99f);
            confirmRect = GUI.Window(987202, confirmRect, DrawConfirmModalContent, "⚠️ ADVERTENCIA: REINICIO PERMANENTE DE MISIONES");
            GUI.BringWindowToFront(987202);
            GUI.backgroundColor = origBg;
        }

        private void DrawConfirmModalContent(int id)
        {
            GUI.DragWindow(new Rect(0, 0, confirmRect.width - 32f, 24f));

            if (GUI.Button(new Rect(confirmRect.width - 28f, 4f, 24f, 20f), "X"))
            {
                showConfirmModal = false;
                return;
            }

            GUILayout.BeginVertical();
            GUILayout.Space(8f);

            GUILayout.Label(string.Format("¿Deseas reiniciar <b><color=#FFD700>{0}</color></b> a estado inicial?", pendingResetTitle), styleModalTitle);
            GUILayout.Label(string.Format("<color=#88CCFF>Misiones que seran reajustadas ({0}):</color> <color=#DDDDDD>{1}</color>", pendingResetKeys.Count, string.Join(", ", pendingResetKeys.ToArray())), styleTip);

            GUILayout.Space(8f);

            Color prevBg = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.12f, 0.14f, 0.18f, 0.95f);
            GUILayout.BeginVertical(GUI.skin.box);
            GUI.backgroundColor = prevBg;

            GUILayout.Label("<b><color=#FF4444>⚠️ LEA DETENIDAMENTE ANTES DE CONFIRMAR:</color></b>", styleHeader);
            GUILayout.Space(4f);
            GUILayout.Label(string.Format("• <b>Efecto Especifico:</b> {0}", pendingResetWarn), styleModalBody);
            GUILayout.Label("• <b>Accion Permanente:</b> Este cambio sobreescribe de forma permanente el estado en memoria y en los archivos de guardado (SaveData*.xml).", styleModalBody);
            GUILayout.Label("• <b>Duplicacion de NPCs y Jefes:</b> Al reiniciar eventos de historia, el juego permitira volver a reclutar o luchar contra los personajes involucrados, lo cual <b>puede duplicar NPCs o recompensas</b>.", styleModalBody);
            GUILayout.Label("• <b>Responsabilidad del Usuario:</b> Se recomienda encarecidamente respaldar tus archivos de guardado antes de proceder.", styleModalBody);

            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            {
                Color prevBtnBg = GUI.backgroundColor;

                GUI.backgroundColor = new Color(0.85f, 0.20f, 0.15f);
                string btnLabel = pendingResetKeys.Count > 1 ? string.Format("⚠️ Si, Reiniciar Arco ({0} Misiones)", pendingResetKeys.Count) : "⚠️ Si, Reiniciar Mision";
                if (GUILayout.Button(btnLabel, GUI.skin.button, GUILayout.Height(36f), GUILayout.Width(275f)))
                {
                    showConfirmModal = false;
                    DoResetMultipleQuests(pendingResetTitle, pendingResetKeys, pendingStory, pendingSave);
                }

                GUILayout.FlexibleSpace();

                GUI.backgroundColor = new Color(0.28f, 0.35f, 0.45f);
                if (GUILayout.Button("❌ Cancelar", GUI.skin.button, GUILayout.Height(36f), GUILayout.Width(275f)))
                {
                    showConfirmModal = false;
                }

                GUI.backgroundColor = prevBtnBg;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(6f);

            GUILayout.EndVertical();
        }

        private void DoResetMultipleQuests(string title, List<string> keys, StoryManager story, SaveManager save)
        {
            int count = 0;
            for (int i = 0; i < keys.Count; i++)
            {
                DoResetSingleQuest(keys[i], story, save);
                count++;
            }

            _lastResetTitle = title;
            _lastResetCount = count;
            _showResetNotice = true;
            _resetNoticeTime = Time.time;
        }

        private void DoResetSingleQuest(string key, StoryManager story, SaveManager save)
        {
            var mn = GameManagers.Managers;

            // 1. Set progress in QuestList (StoryManager memory)
            if (story != null)
            {
                try { story.ProgressChange(key, 0); } catch { }
                try { story.QuestDiaryChange(key, 0); } catch { }
            }

            // 2. Set progress in in-memory SaveEntry
            if (save != null && save.saveEntry != null && save.saveEntry.quests != null)
            {
                for (int i = 0; i < save.saveEntry.quests.Length; i++)
                {
                    if (save.saveEntry.quests[i].questKey == key)
                    {
                        save.saveEntry.quests[i].progress = 0;
                        break;
                    }
                }
            }

            // 3. Write progress = 0 directly to ALL SaveData*.xml files on disk
            try { WriteQuestToAllSaveFiles(key, 0); } catch { }

            // Get mapping info for this quest
            string folderName = key;
            string triggerName = "event_" + key.ToLower().Replace("boss_", "boss_").Replace("main_", "").Replace("sub_", "");
            string stageName = "";
            string[] mapInfo;
            if (QuestTriggerMap.TryGetValue(key, out mapInfo))
            {
                folderName = mapInfo[0];
                triggerName = mapInfo[1];
                stageName = mapInfo.Length > 2 ? mapInfo[2] : "";
            }

            // 4. Reactivate the trigger via StoryManager.ActiveEventTrigger
            if (story != null)
            {
                try
                {
                    story.ActiveEventTrigger(folderName, triggerName, true);
                }
                catch { }
            }

            // 5. Access Events GameObject directly
            GameObject eventsObj = GameObject.Find("Events");
            Transform eventsTransform = eventsObj != null ? eventsObj.transform : null;

            if (eventsTransform != null)
            {
                try
                {
                    Transform questFolder = eventsTransform.Find(folderName);
                    if (questFolder != null)
                    {
                        questFolder.gameObject.SetActive(true);

                        string[] victoryBlocks = new string[] { "Block_00", "Block_01", "Block_01_open", "fence_01" };
                        for (int b = 0; b < victoryBlocks.Length; b++)
                        {
                            Transform blk = questFolder.Find(victoryBlocks[b]);
                            if (blk != null) blk.gameObject.SetActive(false);
                        }

                        string[] entranceBlocks = new string[] { "cave01_block_00", "fence_00", "Key/stones" };
                        for (int b = 0; b < entranceBlocks.Length; b++)
                        {
                            Transform blk = questFolder.Find(entranceBlocks[b]);
                            if (blk != null) blk.gameObject.SetActive(true);
                        }

                        var commons = questFolder.GetComponentsInChildren<CommonStates>(true);
                        for (int c = 0; c < commons.Length; c++)
                        {
                            commons[c].gameObject.SetActive(true);
                            commons[c].dead = 0;
                            commons[c].life = commons[c].maxLife;
                        }

                        var spawners = questFolder.GetComponentsInChildren<EnemySpawner>(true);
                        for (int s = 0; s < spawners.Length; s++)
                        {
                            spawners[s].gameObject.SetActive(true);
                            spawners[s].active = true;
                        }
                    }
                }
                catch { }
            }

            // Special logic for Main_Gate
            if (key == "Main_Gate")
            {
                try
                {
                    GameObject gateObj = GameObject.Find("st_gate_01");
                    if (gateObj == null)
                    {
                        GameObject sbg = GameObject.Find("StaticBG");
                        if (sbg != null)
                        {
                            Transform t = sbg.transform.Find("st_gate_01");
                            if (t != null) gateObj = t.gameObject;
                        }
                    }
                    if (gateObj != null)
                    {
                        Transform doorL = gateObj.transform.Find("Gate/DoorL");
                        if (doorL != null) doorL.gameObject.SetActive(true);
                        Transform doorR = gateObj.transform.Find("Gate/DoorR");
                        if (doorR != null) doorR.gameObject.SetActive(true);
                        Transform stones = gateObj.transform.Find("Key/stones");
                        if (stones != null) stones.gameObject.SetActive(true);
                        Transform warp = gateObj.transform.Find("Warp_Gate_a");
                        if (warp != null) warp.gameObject.SetActive(false);
                    }
                }
                catch { }
            }

            // Special logic for Main_Entking / Ent tree
            if (key == "Main_Entking" || key == "Main_Entking2" || key == "Main_Entqueen")
            {
                try
                {
                    if (eventsTransform != null)
                    {
                        Transform entFolder = eventsTransform.Find("Main_Entking");
                        if (entFolder != null)
                        {
                            Transform break00 = entFolder.Find("Break_00");
                            if (break00 != null) break00.gameObject.SetActive(false);
                            Transform barkBridge = entFolder.Find("Bark_Bridge");
                            if (barkBridge != null) barkBridge.gameObject.SetActive(false);
                        }
                    }
                }
                catch { }
            }

            // 6. Reactivate ALL EventStarters matching this quest
            EventStarter[] allStarters = null;
            if (eventsObj != null)
            {
                allStarters = eventsObj.GetComponentsInChildren<EventStarter>(true);
            }
            if (allStarters == null || allStarters.Length == 0)
            {
                allStarters = UnityEngine.Object.FindObjectsOfType<EventStarter>();
            }

            var gm = mn != null ? mn.gameMN : null;

            if (allStarters != null)
            {
                for (int i = 0; i < allStarters.Length; i++)
                {
                    var es = allStarters[i];
                    if (es == null) continue;

                    string esName = es.eventName != null ? es.eventName : "";
                    string parentName = es.transform.parent != null ? es.transform.parent.name : "";

                    bool isMatch =
                        string.Equals(esName, triggerName, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(esName, key, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(parentName, folderName, StringComparison.OrdinalIgnoreCase) ||
                        (!string.IsNullOrEmpty(key) && esName.ToLower().Contains(key.ToLower().Replace("boss_", "").Replace("main_", "").Replace("sub_", "")));

                    if (isMatch)
                    {
                        try
                        {
                            es.trigged = false;

                            Transform p = es.transform;
                            while (p != null)
                            {
                                if (!p.gameObject.activeSelf) p.gameObject.SetActive(true);
                                p = p.parent;
                            }

                            es.gameObject.SetActive(true);
                            es.TriggerCheck();

                            var qi = es.GetComponent<QuestIcon>();
                            if (qi != null && qi.questIcon != null)
                            {
                                qi.questIcon.SetActive(true);
                            }

                            if (es.questIconImage != null)
                            {
                                es.questIconImage.gameObject.SetActive(true);
                                es.questIconImage.enabled = true;
                                Color c = es.questIconImage.color;
                                c.a = 1.0f;
                                es.questIconImage.color = c;
                            }

                            es.MapIconFadeCheck();
                            if (gm != null)
                            {
                                gm.MapIconFade();
                            }
                        }
                        catch { }
                    }
                }
            }

            // 7. If stageName is defined, check StaticBG
            if (!string.IsNullOrEmpty(stageName))
            {
                try
                {
                    GameObject sbg = GameObject.Find("StaticBG");
                    if (sbg != null)
                    {
                        Transform stg = sbg.transform.Find(stageName);
                        if (stg != null)
                        {
                            stg.gameObject.SetActive(true);
                            var commons = stg.GetComponentsInChildren<CommonStates>(true);
                            for (int c = 0; c < commons.Length; c++)
                            {
                                commons[c].gameObject.SetActive(true);
                                commons[c].dead = 0;
                                commons[c].life = commons[c].maxLife;
                            }
                        }
                    }
                }
                catch { }
            }

            // 8. Remove from completed quest UI
            try
            {
                if (mn != null && mn.uiMN != null)
                    mn.uiMN.DeleteQuestByName(key);
            }
            catch { }

            // 9. Play sound
            try
            {
                if (mn != null && mn.sound != null)
                    mn.sound.GoSoundButton(0);
            }
            catch { }
        }

        private int GetQuestProgress(string key, SaveManager save, StoryManager story)
        {
            if (story != null)
            {
                try
                {
                    int p = story.QuestProgress(key);
                    if (p >= 0) return p;
                }
                catch { }
            }

            if (save != null && save.saveEntry != null && save.saveEntry.quests != null)
            {
                for (int i = 0; i < save.saveEntry.quests.Length; i++)
                {
                    if (save.saveEntry.quests[i].questKey == key)
                        return save.saveEntry.quests[i].progress;
                }
            }
            return -1;
        }

        private void WriteQuestToAllSaveFiles(string questKey, int progress)
        {
            try
            {
                string dataDir = System.IO.Path.Combine(
                    UnityEngine.Application.dataPath,
                    "StreamingAssets", "XML");
                if (!System.IO.Directory.Exists(dataDir)) return;

                string[] files = System.IO.Directory.GetFiles(dataDir, "SaveData*.xml");
                for (int f = 0; f < files.Length; f++)
                {
                    try
                    {
                        string xmlPath = files[f];
                        var doc = new System.Xml.XmlDocument();
                        doc.Load(xmlPath);
                        var nodes = doc.SelectNodes(string.Format("//QuestSave[questKey='{0}']", questKey));
                        if (nodes != null && nodes.Count > 0)
                        {
                            foreach (System.Xml.XmlNode node in nodes)
                            {
                                var progNode = node.SelectSingleNode("progress");
                                if (progNode != null)
                                    progNode.InnerText = progress.ToString();
                            }
                            doc.Save(xmlPath);
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }
    }
}
