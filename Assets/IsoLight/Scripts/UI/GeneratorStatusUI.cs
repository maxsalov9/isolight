using IsoLight.Combat;
using IsoLight.Power;
using UnityEngine;

namespace IsoLight.UI
{
    public class GeneratorStatusUI : MonoBehaviour
    {
        [SerializeField] private Rect panelRect = new Rect(24f, 204f, 360f, 116f);

        private CombatManager combatManager;
        private GeneratorG17 generator;
        private GUIStyle panelStyle;
        private GUIStyle labelStyle;
        private GUIStyle warningStyle;

        public void SetReferences(CombatManager combat, GeneratorG17 targetGenerator)
        {
            combatManager = combat;
            generator = targetGenerator;
        }

        private void Awake()
        {
            if (combatManager == null)
            {
                combatManager = FindAnyObjectByType<CombatManager>();
            }

            if (generator == null)
            {
                generator = FindAnyObjectByType<GeneratorG17>();
            }
        }

        private void OnGUI()
        {
            if (generator == null || (combatManager != null && !combatManager.IsCombatActive && generator.IsAlive))
            {
                return;
            }

            EnsureStyles();

            GUI.Box(panelRect, GUIContent.none, panelStyle);
            GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 10f, panelRect.width - 24f, 24f), $"Генератор G-17: {FormatState()}", labelStyle);
            GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 38f, panelRect.width - 24f, 24f), $"HP {generator.CurrentHealth}/{generator.MaxHealth}", labelStyle);

            if (!generator.IsAlive)
            {
                GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 66f, panelRect.width - 24f, 40f), "Генератор уничтожен. Столкновение нужно переиграть.", warningStyle);
            }
            else if (generator.WasRecentlyDamaged)
            {
                GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 66f, panelRect.width - 24f, 40f), "ВНИМАНИЕ: генератор под атакой.", warningStyle);
            }
            else
            {
                GUI.Label(new Rect(panelRect.x + 12f, panelRect.y + 66f, panelRect.width - 24f, 24f), "Состояние стабильное.", labelStyle);
            }
        }

        private string FormatState()
        {
            if (!generator.IsAlive)
            {
                return "уничтожен";
            }

            if (generator.IsStarted)
            {
                return "запущен";
            }

            return generator.IsRepaired ? "отремонтирован" : "требует ремонта";
        }

        private void EnsureStyles()
        {
            panelStyle ??= new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(10, 10, 8, 8)
            };
            labelStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            warningStyle ??= new GUIStyle(labelStyle)
            {
                wordWrap = true
            };
            warningStyle.normal.textColor = new Color(1f, 0.45f, 0.22f);
        }
    }
}
