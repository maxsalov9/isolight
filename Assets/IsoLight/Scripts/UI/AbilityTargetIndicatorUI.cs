using IsoLight.Characters;
using IsoLight.Core;
using IsoLight.Party;
using UnityEngine;

namespace IsoLight.UI
{
    public class AbilityTargetIndicatorUI : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private PartyManager partyManager;
        [SerializeField] private AbilityController abilityController;
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2.3f, 0f);

        private readonly string[] hotkeys = { "Q", "E", "R" };
        private GUIStyle labelStyle;

        public void SetReferences(GameManager game, PartyManager party, AbilityController controller)
        {
            gameManager = game;
            partyManager = party;
            abilityController = controller;
        }

        private void OnGUI()
        {
            CacheReferences();

            if (gameManager == null
                || gameManager.CurrentGameMode != GameMode.Combat
                || partyManager == null
                || partyManager.ActiveCharacter == null
                || abilityController == null
                || UnityEngine.Camera.main == null)
            {
                return;
            }

            labelStyle ??= CreateLabelStyle();
            var abilities = partyManager.ActiveCharacter.Abilities;
            var count = Mathf.Min(abilities.Count, hotkeys.Length);
            for (var i = 0; i < count; i++)
            {
                var preview = abilityController.GetAbilityPreview(i);
                if (preview == null || preview.Target == null || preview.Ability == null)
                {
                    continue;
                }

                DrawPreview(preview, hotkeys[i], i);
            }
        }

        private void DrawPreview(AbilityTargetPreview preview, string hotkey, int index)
        {
            var screenPosition = UnityEngine.Camera.main.WorldToScreenPoint(preview.Target.transform.position + worldOffset + new Vector3(0f, index * 0.22f, 0f));
            if (screenPosition.z <= 0f)
            {
                return;
            }

            var color = AbilityController.GetAbilityColor(preview.Ability);
            color.a = preview.IsUsable ? 1f : 0.62f;
            labelStyle.normal.textColor = color;

            var targetText = string.IsNullOrWhiteSpace(preview.TargetLabel) ? preview.Target.name : preview.TargetLabel;
            var label = $"{hotkey}: цель способности\n{targetText}";
            var size = labelStyle.CalcSize(new GUIContent(label));
            var rect = new Rect(screenPosition.x - size.x * 0.5f - 8f, Screen.height - screenPosition.y - 18f, size.x + 16f, 42f);
            GUI.Box(rect, label, labelStyle);
        }

        private void CacheReferences()
        {
            if (gameManager == null) gameManager = FindAnyObjectByType<GameManager>();
            if (partyManager == null) partyManager = FindAnyObjectByType<PartyManager>();
            if (abilityController == null) abilityController = FindAnyObjectByType<AbilityController>();
        }

        private static GUIStyle CreateLabelStyle()
        {
            var style = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                padding = new RectOffset(6, 6, 3, 3)
            };
            return style;
        }
    }
}
