using IsoLight.Combat;
using IsoLight.Enemies;
using IsoLight.Party;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace IsoLight.UI
{
    public class CombatCommandUI : MonoBehaviour
    {
        [SerializeField] private CombatManager combatManager;
        [SerializeField] private PartyManager partyManager;
        [SerializeField] private CombatCommandController commandController;
        [SerializeField] private Vector3 partyLabelOffset = new Vector3(0f, 2.35f, 0f);
        [SerializeField] private Vector3 targetLabelOffset = new Vector3(0f, 2.65f, 0f);

        private GUIStyle pauseStyle;
        private GUIStyle labelStyle;
        private GUIStyle targetModeStyle;

        public void SetReferences(CombatManager combat, PartyManager party, CombatCommandController controller)
        {
            combatManager = combat;
            partyManager = party;
            commandController = controller;
        }

        private void OnGUI()
        {
            CacheReferences();
            if (combatManager == null || !combatManager.IsCombatActive)
            {
                return;
            }

            EnsureStyles();

            if (combatManager.IsTacticalPaused)
            {
                GUI.Box(new Rect(Screen.width * 0.5f - 180f, 22f, 360f, 42f), "ТАКТИЧЕСКАЯ ПАУЗА", pauseStyle);
            }

            DrawCommandLabels();
            DrawAutoAttackLabels();
            DrawAbilityTargetMode();
        }

        private void DrawCommandLabels()
        {
            if (partyManager?.PartyMembers == null || commandController == null || UnityEngine.Camera.main == null)
            {
                return;
            }

            for (var i = 0; i < partyManager.PartyMembers.Count; i++)
            {
                var character = partyManager.PartyMembers[i];
                var label = commandController.GetCommandLabel(character);
                if (character == null || string.IsNullOrWhiteSpace(label))
                {
                    continue;
                }

                DrawWorldLabel(character.transform.position + partyLabelOffset, label, labelStyle);
            }
        }

        private void DrawAutoAttackLabels()
        {
            if (partyManager?.PartyMembers == null || commandController == null || UnityEngine.Camera.main == null)
            {
                return;
            }

            for (var i = 0; i < partyManager.PartyMembers.Count; i++)
            {
                var character = partyManager.PartyMembers[i];
                var target = commandController.GetAutoAttackTarget(character);
                if (character == null || target == null || !target.IsAlive)
                {
                    continue;
                }

                DrawWorldLabel(target.transform.position + targetLabelOffset, $"{character.DisplayName}: автоатака", labelStyle);
            }
        }

        private void DrawAbilityTargetMode()
        {
            if (commandController == null || !commandController.IsAbilityTargetModeActive)
            {
                return;
            }

            var pointer = GetPointerPosition();
            var rect = new Rect(pointer.x + 18f, Screen.height - pointer.y - 42f, 310f, 54f);
            GUI.Box(rect, $"{commandController.TargetModeHint}\nЛКМ: выбрать цель | Esc: отменить", targetModeStyle);

            var x = pointer.x;
            var y = Screen.height - pointer.y;
            GUI.Box(new Rect(x - 10f, y - 1f, 20f, 2f), GUIContent.none);
            GUI.Box(new Rect(x - 1f, y - 10f, 2f, 20f), GUIContent.none);
        }

        private static void DrawWorldLabel(Vector3 worldPosition, string text, GUIStyle style)
        {
            var screenPosition = UnityEngine.Camera.main.WorldToScreenPoint(worldPosition);
            if (screenPosition.z <= 0f)
            {
                return;
            }

            var content = new GUIContent(text);
            var size = style.CalcSize(content);
            var rect = new Rect(screenPosition.x - size.x * 0.5f - 8f, Screen.height - screenPosition.y - 16f, size.x + 16f, 30f);
            GUI.Box(rect, text, style);
        }

        private void EnsureStyles()
        {
            pauseStyle ??= new GUIStyle(GUI.skin.box)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            pauseStyle.normal.textColor = new Color(1f, 0.9f, 0.35f);

            labelStyle ??= new GUIStyle(GUI.skin.box)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(6, 6, 3, 3)
            };
            labelStyle.normal.textColor = Color.white;

            targetModeStyle ??= new GUIStyle(GUI.skin.box)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                wordWrap = true,
                padding = new RectOffset(8, 8, 5, 5)
            };
            targetModeStyle.normal.textColor = new Color(0.7f, 0.92f, 1f);
        }

        private void CacheReferences()
        {
            if (combatManager == null) combatManager = FindAnyObjectByType<CombatManager>();
            if (partyManager == null) partyManager = FindAnyObjectByType<PartyManager>();
            if (commandController == null) commandController = FindAnyObjectByType<CombatCommandController>();
        }

        private static Vector3 GetPointerPosition()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector3.zero;
#else
            return UnityEngine.Input.mousePosition;
#endif
        }
    }
}
