using IsoLight.Combat;
using IsoLight.Enemies;
using IsoLight.Power;
using UnityEngine;

namespace IsoLight.UI
{
    public class CombatStatusUI : MonoBehaviour
    {
        [SerializeField] private Rect panelRect = new Rect(24f, 326f, 360f, 92f);

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
            GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 10f, panelRect.width - 24f, 24f), $"Рейдеров осталось: {combatManager.LivingEnemyCount}", labelStyle);
            GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 40f, panelRect.width - 24f, 24f), FormatFocusedEnemy(combatManager.FocusedEnemy), labelStyle);
        }

        private static string FormatFocusedEnemy(Enemy enemy)
        {
            return enemy != null && enemy.IsAlive
                ? $"Цель HP: {enemy.CurrentHealth}/{enemy.MaxHealth}"
                : "Цель HP: нет";
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
