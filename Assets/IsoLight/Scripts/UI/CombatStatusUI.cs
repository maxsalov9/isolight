using IsoLight.Combat;
using IsoLight.Enemies;
using IsoLight.Power;
using UnityEngine;

namespace IsoLight.UI
{
    public class CombatStatusUI : MonoBehaviour
    {
        [SerializeField] private Rect panelRect = new Rect(24f, 204f, 360f, 112f);

        private CombatManager combatManager;
        private GeneratorG17 generator;
        private GUIStyle panelStyle;
        private GUIStyle labelStyle;

        public void SetReferences(CombatManager combat, GeneratorG17 targetGenerator)
        {
            combatManager = combat;
            generator = targetGenerator;
        }

        private void OnGUI()
        {
            if (combatManager == null || !combatManager.IsCombatActive)
            {
                return;
            }

            panelStyle ??= CreatePanelStyle();
            labelStyle ??= CreateLabelStyle();

            GUI.Box(panelRect, GUIContent.none, panelStyle);
            var generatorHealth = generator != null ? $"{generator.CurrentHealth}/{generator.MaxHealth}" : "unknown";
            GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 10f, panelRect.width - 24f, 24f), $"Generator G-17 HP: {generatorHealth}", labelStyle);
            GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 42f, panelRect.width - 24f, 24f), $"Raiders remaining: {combatManager.LivingEnemyCount}", labelStyle);
            GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 66f, panelRect.width - 24f, 24f), FormatFocusedEnemy(combatManager.FocusedEnemy), labelStyle);
        }

        private static string FormatFocusedEnemy(Enemy enemy)
        {
            return enemy != null && enemy.IsAlive
                ? $"Target HP: {enemy.CurrentHealth}/{enemy.MaxHealth}"
                : "Target HP: none";
        }

        private static GUIStyle CreatePanelStyle()
        {
            return new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(10, 10, 8, 8)
            };
        }

        private static GUIStyle CreateLabelStyle()
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold
            };
            style.normal.textColor = Color.white;
            return style;
        }
    }
}
