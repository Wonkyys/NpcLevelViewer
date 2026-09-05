using System;
using System.Collections.Generic;
using UnityEngine;
using MadIslandModCore;

namespace NpcLevelViewer
{
    public enum StatFilterMode
    {
        All = 0,
        AnyUpgradable = 1,
        OnlyHealth = 2,
        OnlyAttack = 3,
        OnlySpeed = 4
    }

    public class NpcStatUI : MonoBehaviour, IModModule
    {
        public static NpcStatUI Instance { get; private set; }

        // ── IModModule Properties ──────────────────────────────────────────────────
        public string ModId { get { return "npc_level_viewer"; } }
        public string ModName { get { return "👥 Aliados & Stats (NPC Manager)"; } }
        public string ModDescription { get { return "Visualiza niveles, puntos dorados y mejora stats (Vitalidad, Fuerza, Velocidad) de tus NPCs aliados."; } }
        public string ModVersion { get { return "1.0.0"; } }

        public void OnDrawUI(Rect area)
        {
            InitStyles();
            DrawContent();
        }

        // ── Standalone / Configuration ─────────────────────────────────────────────
        public bool IsVisible = false;
        public KeyCode ToggleKey = KeyCode.F5;
        public StatFilterMode CurrentFilter = StatFilterMode.All;
        public bool SpanishLabels = true; // H, F, V vs H, A, S
        public int BulkMultiplier = 1; // 1, 10, 100
        public string SearchQuery = "";

        private Rect windowRect = new Rect(60, 50, 800, 590);
        private Vector2 scrollPos = Vector2.zero;
        private List<CommonStates> cachedNpcs = new List<CommonStates>();
        private float lastScanTime = 0f;
        private const float SCAN_INTERVAL = 600.0f;

        private CommonStates hoveredNpc = null;

        // Custom GUI styles
        private GUIStyle winStyle;
        private GUIStyle headerStyle;
        private GUIStyle npcNameStyle;
        private GUIStyle pointsStyle;
        private GUIStyle btnUpgradeStyle;
        private GUIStyle btnDisabledStyle;
        private GUIStyle tipStyle;
        private GUIStyle rowBoxStyle;
        private bool stylesInitialized = false;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            RefreshList();
            ModRegistry.Register(this);
        }

        private void OnDestroy()
        {
            ModRegistry.Unregister(this);
        }

        private void Update()
        {
            // Keyboard toggle (only in standalone mode)
            if (!ModRegistry.IsModManagerActive && Input.GetKeyDown(ToggleKey))
            {
                IsVisible = !IsVisible;
                if (IsVisible)
                {
                    RefreshList();
                }
            }

            // Keyboard shortcut upgrade for currently hovered NPC
            if ((IsVisible || ModRegistry.IsModManagerActive) && hoveredNpc != null)
            {
                int multiplier = BulkMultiplier;
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    multiplier = 10;
                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                    multiplier = 100;

                // Health / Vitality (H)
                if (Input.GetKeyDown(KeyCode.H))
                {
                    NpcUpgradeHelper.UpgradeStat(hoveredNpc, 0, multiplier);
                }
                // Strength / Attack (F in Spanish, A in English)
                if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.A))
                {
                    NpcUpgradeHelper.UpgradeStat(hoveredNpc, 1, multiplier);
                }
                // Speed / Agility (V in Spanish, S in English)
                if (Input.GetKeyDown(KeyCode.V) || Input.GetKeyDown(KeyCode.S))
                {
                    NpcUpgradeHelper.UpgradeStat(hoveredNpc, 2, multiplier);
                }
            }

            // Periodic auto-refresh (10 minutes)
            if (Time.time - lastScanTime > SCAN_INTERVAL)
            {
                lastScanTime = Time.time;
                if (IsVisible || cachedNpcs.Count == 0)
                {
                    RefreshList();
                }
            }
        }

        public void RefreshList()
        {
            cachedNpcs = NpcUpgradeHelper.GetFriendNpcs();
        }

        private void InitStyles()
        {
            if (stylesInitialized) return;

            winStyle = new GUIStyle(GUI.skin.window);
            winStyle.fontSize = 13;
            winStyle.fontStyle = FontStyle.Bold;

            headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontSize = 12;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.normal.textColor = new Color(0.95f, 0.95f, 0.95f);

            npcNameStyle = new GUIStyle(GUI.skin.label);
            npcNameStyle.fontSize = 13;
            npcNameStyle.fontStyle = FontStyle.Bold;
            npcNameStyle.normal.textColor = Color.white;
            npcNameStyle.richText = true;
            npcNameStyle.alignment = TextAnchor.MiddleLeft;

            pointsStyle = new GUIStyle(GUI.skin.label);
            pointsStyle.fontSize = 12;
            pointsStyle.fontStyle = FontStyle.Bold;
            pointsStyle.alignment = TextAnchor.MiddleRight;
            pointsStyle.normal.textColor = new Color(1.0f, 0.85f, 0.2f); // Gold
            pointsStyle.richText = true;

            btnUpgradeStyle = new GUIStyle(GUI.skin.button);
            btnUpgradeStyle.fontSize = 11;
            btnUpgradeStyle.fontStyle = FontStyle.Bold;
            btnUpgradeStyle.richText = true;
            btnUpgradeStyle.normal.textColor = Color.white;

            btnDisabledStyle = new GUIStyle(GUI.skin.button);
            btnDisabledStyle.fontSize = 11;
            btnDisabledStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
            btnDisabledStyle.richText = true;

            tipStyle = new GUIStyle(GUI.skin.label);
            tipStyle.fontSize = 11;
            tipStyle.normal.textColor = new Color(0.75f, 0.85f, 1f);

            rowBoxStyle = new GUIStyle(GUI.skin.box);
            rowBoxStyle.padding = new RectOffset(6, 6, 4, 4);

            stylesInitialized = true;
        }

        private void OnGUI()
        {
            // If ModManager is active, do NOT render standalone UI
            if (ModRegistry.IsModManagerActive) return;

            // Standalone mode: mini-button in top-right corner
            float btnWidth = 60f;
            float btnHeight = 26f;
            float posX = Screen.width - btnWidth - 10f;
            float posY = 10f;

            Color prev = GUI.backgroundColor;
            GUI.backgroundColor = IsVisible ? new Color(0.2f, 0.7f, 0.3f) : new Color(0.2f, 0.4f, 0.7f);
            if (GUI.Button(new Rect(posX, posY, btnWidth, btnHeight), "[NPC]"))
            {
                IsVisible = !IsVisible;
                if (IsVisible) RefreshList();
            }
            GUI.backgroundColor = prev;

            if (!IsVisible) return;

            InitStyles();

            GUI.backgroundColor = new Color(0.12f, 0.14f, 0.18f, 0.96f);
            windowRect = GUI.Window(987654, windowRect, DrawWindow, "👥 Mad Island - NPC Stat Manager");
        }

        private void DrawWindow(int id)
        {
            GUI.DragWindow(new Rect(0, 0, windowRect.width - 40, 25));

            // Close button [X]
            if (GUI.Button(new Rect(windowRect.width - 32, 4, 26, 20), "X"))
            {
                IsVisible = false;
                return;
            }

            DrawContent();
        }

        public void DrawContent()
        {
            GUILayout.BeginVertical();
            GUILayout.Space(4);

            // Controls Bar - Row 1: Search, Multiplier, Language
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Buscar:", GUILayout.Width(50));
                SearchQuery = GUILayout.TextField(SearchQuery, GUILayout.Width(140));

                GUILayout.Space(15);

                GUILayout.Label("Multiplicador:", GUILayout.Width(85));
                if (GUILayout.Toggle(BulkMultiplier == 1, "1x", GUI.skin.button, GUILayout.Width(35))) BulkMultiplier = 1;
                if (GUILayout.Toggle(BulkMultiplier == 10, "10x", GUI.skin.button, GUILayout.Width(40))) BulkMultiplier = 10;
                if (GUILayout.Toggle(BulkMultiplier == 100, "100x", GUI.skin.button, GUILayout.Width(45))) BulkMultiplier = 100;

                GUILayout.FlexibleSpace();

                string langLabel = SpanishLabels ? "ES (H-F-V)" : "EN (H-A-S)";
                if (GUILayout.Button(langLabel, GUILayout.Width(90)))
                {
                    SpanishLabels = !SpanishLabels;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4);

            // Controls Bar - Row 2: Stat Filters
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("<b>Filtro:</b>", headerStyle, GUILayout.Width(45));

                if (GUILayout.Toggle(CurrentFilter == StatFilterMode.All, "Todos", GUI.skin.button, GUILayout.Width(60)))
                    CurrentFilter = StatFilterMode.All;

                if (GUILayout.Toggle(CurrentFilter == StatFilterMode.AnyUpgradable, "⭐ Con Puntos", GUI.skin.button, GUILayout.Width(100)))
                    CurrentFilter = StatFilterMode.AnyUpgradable;

                string hFilterLabel = SpanishLabels ? "❤️ Solo Vitalidad (H)" : "❤️ Only Health (H)";
                if (GUILayout.Toggle(CurrentFilter == StatFilterMode.OnlyHealth, hFilterLabel, GUI.skin.button, GUILayout.Width(140)))
                    CurrentFilter = StatFilterMode.OnlyHealth;

                string fFilterLabel = SpanishLabels ? "⚔️ Solo Fuerza (F)" : "⚔️ Only Attack (A)";
                if (GUILayout.Toggle(CurrentFilter == StatFilterMode.OnlyAttack, fFilterLabel, GUI.skin.button, GUILayout.Width(130)))
                    CurrentFilter = StatFilterMode.OnlyAttack;

                string vFilterLabel = SpanishLabels ? "💨 Solo Velocidad (V)" : "💨 Only Speed (S)";
                if (GUILayout.Toggle(CurrentFilter == StatFilterMode.OnlySpeed, vFilterLabel, GUI.skin.button, GUILayout.Width(145)))
                    CurrentFilter = StatFilterMode.OnlySpeed;
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);

            // Table Header
            GUILayout.BeginHorizontal(GUI.skin.box);
            {
                GUILayout.Label("NPC (Nivel)", headerStyle, GUILayout.Width(180));
                GUILayout.Label("Puntos", headerStyle, GUILayout.Width(95));

                string hTitle = SpanishLabels ? "Vitalidad (H)" : "Health (H)";
                string fTitle = SpanishLabels ? "Fuerza (F)" : "Attack (A)";
                string vTitle = SpanishLabels ? "Velocidad (V)" : "Speed (S)";

                GUILayout.Label(hTitle, headerStyle, GUILayout.Width(135));
                GUILayout.Label(fTitle, headerStyle, GUILayout.Width(135));
                GUILayout.Label(vTitle, headerStyle, GUILayout.Width(135));
            }
            GUILayout.EndHorizontal();

            // Quick Hotkey hint
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("💡 Pasa el mouse sobre un NPC y presiona '{0}', '{1}' o '{2}' en el teclado para subirlo al instante.",
                "H",
                SpanishLabels ? "F" : "A",
                SpanishLabels ? "V" : "S"
            ), tipStyle);
            GUILayout.EndHorizontal();

            GUILayout.Space(2);

            // Scrollable List of NPCs
            scrollPos = GUILayout.BeginScrollView(scrollPos);
            hoveredNpc = null;

            int displayedCount = 0;
            for (int i = 0; i < cachedNpcs.Count; i++)
            {
                var npc = cachedNpcs[i];
                if (npc == null) continue;

                // Check active filter
                bool passesFilter = true;
                switch (CurrentFilter)
                {
                    case StatFilterMode.AnyUpgradable:
                        passesFilter = NpcUpgradeHelper.CanUpgradeAny(npc);
                        break;
                    case StatFilterMode.OnlyHealth:
                        passesFilter = NpcUpgradeHelper.CanUpgrade(npc, 0);
                        break;
                    case StatFilterMode.OnlyAttack:
                        passesFilter = NpcUpgradeHelper.CanUpgrade(npc, 1);
                        break;
                    case StatFilterMode.OnlySpeed:
                        passesFilter = NpcUpgradeHelper.CanUpgrade(npc, 2);
                        break;
                    case StatFilterMode.All:
                    default:
                        passesFilter = true;
                        break;
                }

                if (!passesFilter)
                    continue;

                string displayName = NpcUpgradeHelper.GetNpcDisplayName(npc);
                if (!string.IsNullOrEmpty(SearchQuery) &&
                    displayName.IndexOf(SearchQuery, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                displayedCount++;
                bool canAny = NpcUpgradeHelper.CanUpgradeAny(npc);
                DrawNpcRow(npc, displayName, canAny);
            }

            if (displayedCount == 0)
            {
                GUILayout.Space(30);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                string emptyMsg = "No hay NPCs que coincidan con el filtro actual.";
                if (CurrentFilter == StatFilterMode.OnlySpeed)
                    emptyMsg = "Ningún NPC tiene suficientes puntos para subir Velocidad (V) en este momento.";
                else if (CurrentFilter == StatFilterMode.OnlyAttack)
                    emptyMsg = "Ningún NPC tiene suficientes puntos para subir Fuerza (F) en este momento.";
                else if (CurrentFilter == StatFilterMode.OnlyHealth)
                    emptyMsg = "Ningún NPC tiene suficientes puntos para subir Vitalidad (H) en este momento.";
                else if (cachedNpcs.Count == 0)
                    emptyMsg = "No se encontraron NPCs amigos. Pulsa 'Actualizar' si acabas de cargar partida.";

                GUILayout.Label(emptyMsg, headerStyle);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();

            // Footer
            GUILayout.BeginHorizontal();
            {
                string filterDesc = "";
                if (CurrentFilter == StatFilterMode.OnlySpeed) filterDesc = "(Solo Velocidad V)";
                else if (CurrentFilter == StatFilterMode.OnlyAttack) filterDesc = "(Solo Fuerza F)";
                else if (CurrentFilter == StatFilterMode.OnlyHealth) filterDesc = "(Solo Vitalidad H)";
                else if (CurrentFilter == StatFilterMode.AnyUpgradable) filterDesc = "(Con Puntos)";

                GUILayout.Label(string.Format("Total amigos: {0} | Mostrando: {1} {2}",
                    cachedNpcs.Count,
                    displayedCount,
                    filterDesc),
                    tipStyle);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("🔄 Actualizar", GUILayout.Width(100), GUILayout.Height(24)))
                {
                    RefreshList();
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void DrawNpcRow(CommonStates npc, string displayName, bool canAny)
        {
            Color origBg = GUI.backgroundColor;
            Color origContent = GUI.contentColor;

            // Highlight background if upgradable
            if (canAny)
            {
                GUI.backgroundColor = new Color(0.22f, 0.48f, 0.28f, 0.95f);
            }
            else
            {
                GUI.backgroundColor = new Color(0.20f, 0.22f, 0.28f, 0.85f);
            }

            GUILayout.BeginHorizontal(GUI.skin.box, GUILayout.Height(36));
            {
                // Name & Level
                string nameText = string.Format("<b>{0}</b> (Lv.{1})", displayName, npc.level);
                GUILayout.Label(nameText, npcNameStyle, GUILayout.Width(180), GUILayout.Height(30));

                // Points available
                string pts = npc.statusPoint > 0 
                    ? string.Format("<color=#FFD700><b>⭐ {0} pt</b></color>", npc.statusPoint) 
                    : string.Format("<color=#AAAAAA>{0} pt</color>", npc.statusPoint);
                GUILayout.Label(pts, pointsStyle, GUILayout.Width(95), GUILayout.Height(30));

                GUILayout.Space(5);

                // Stats Buttons:
                // 0: Health / Vitality (H)
                DrawStatButton(npc, 0, SpanishLabels ? "H" : "H", 130);

                // 1: Attack / Strength (F/A)
                DrawStatButton(npc, 1, SpanishLabels ? "F" : "A", 130);

                // 2: Speed / Agility (V/S)
                DrawStatButton(npc, 2, SpanishLabels ? "V" : "S", 130);
            }
            GUILayout.EndHorizontal();

            // Detect mouse hover over this row for instant keyboard upgrade
            if (Event.current.type == EventType.Repaint)
            {
                Rect lastRect = GUILayoutUtility.GetLastRect();
                if (lastRect.Contains(Event.current.mousePosition))
                {
                    hoveredNpc = npc;
                }
            }

            GUI.backgroundColor = origBg;
            GUI.contentColor = origContent;
            GUILayout.Space(2);
        }

        private void DrawStatButton(CommonStates npc, int statIndex, string letter, float width)
        {
            int currentLevel = (npc.status != null && statIndex < npc.status.Length) ? npc.status[statIndex] : 0;
            int cost = NpcUpgradeHelper.GetStatCost(npc, statIndex);
            bool canUp = NpcUpgradeHelper.CanUpgrade(npc, statIndex);

            // If speed limit is reached
            if (statIndex == 2 && npc.speedLimit > 0f && npc.speed >= npc.speedLimit)
            {
                Color oldColor = GUI.backgroundColor;
                GUI.backgroundColor = Color.gray;
                GUILayout.Button(string.Format("{0} (MAX)", letter), btnDisabledStyle, GUILayout.Width(width), GUILayout.Height(28));
                GUI.backgroundColor = oldColor;
                return;
            }

            string label = string.Format("{0} Lv.{1} ({2}pt)", letter, currentLevel, cost);
            if (BulkMultiplier > 1)
            {
                label = string.Format("{0} +{1} ({2}pt)", letter, BulkMultiplier, cost);
            }

            Color prevBg = GUI.backgroundColor;
            if (canUp)
            {
                GUI.backgroundColor = new Color(0.2f, 0.85f, 0.35f, 1f); // Bright green
                if (GUILayout.Button(label, btnUpgradeStyle, GUILayout.Width(width), GUILayout.Height(28)))
                {
                    int mult = BulkMultiplier;
                    if (Event.current.shift) mult = 10;
                    if (Event.current.control) mult = 100;
                    NpcUpgradeHelper.UpgradeStat(npc, statIndex, mult);
                }
            }
            else
            {
                GUI.backgroundColor = new Color(0.35f, 0.38f, 0.42f, 0.65f); // Dimmed
                GUILayout.Button(label, btnDisabledStyle, GUILayout.Width(width), GUILayout.Height(28));
            }
            GUI.backgroundColor = prevBg;
        }
    }
}
