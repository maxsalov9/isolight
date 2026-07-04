using IsoLight.Characters;
using IsoLight.Core;
using IsoLight.Party;
using UnityEngine;

namespace IsoLight.UI
{
    public class AbilityBarUI : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private PartyManager partyManager;
        [SerializeField] private AbilityController abilityController;

        private readonly string[] hotkeys = { "Q", "E", "R" };
        private GUIStyle panelStyle;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle buttonStyle;
        private GUIStyle statusStyle;

        public void SetReferences(GameManager game, PartyManager party, AbilityController controller)
        {
            gameManager = game;
            partyManager = party;
            abilityController = controller;
        }

        private void Awake()
        {
            CacheReferences();
        }

        private void OnGUI()
        {
            CacheReferences();

            var activeCharacter = partyManager != null ? partyManager.ActiveCharacter : null;
            if (activeCharacter == null || activeCharacter.Abilities == null || activeCharacter.Abilities.Count == 0)
            {
                return;
            }

            panelStyle ??= new GUIStyle(GUI.skin.box);
            titleStyle ??= CreateTitleStyle();
            bodyStyle ??= CreateBodyStyle();
            buttonStyle ??= CreateButtonStyle();
            statusStyle ??= CreateStatusStyle();

            var abilityCount = Mathf.Min(activeCharacter.Abilities.Count, hotkeys.Length);
            var width = 220f * abilityCount + 16f;
            var rect = new Rect(Screen.width * 0.5f - width * 0.5f, Screen.height - 136f, width, 120f);
            GUI.Box(rect, string.Empty, panelStyle);
            GUI.Label(new Rect(rect.x + 10f, rect.y + 6f, rect.width - 20f, 20f), $"Способности: {activeCharacter.DisplayName} | Энергия {activeCharacter.CurrentEnergy}/{activeCharacter.MaxEnergy}", titleStyle);

            for (var i = 0; i < abilityCount; i++)
            {
                DrawAbilityButton(activeCharacter, activeCharacter.Abilities[i], i, rect.x + 10f + i * 220f, rect.y + 32f);
            }
        }

        private void DrawAbilityButton(PlayerCharacter character, AbilityData ability, int index, float x, float y)
        {
            var rect = new Rect(x, y, 210f, 50f);
            var preview = abilityController != null
                ? abilityController.GetAbilityPreview(character, ability)
                : null;
            var cooldown = preview != null ? preview.CooldownRemaining : character.GetAbilityCooldownRemaining(ability);
            var label = ability == null
                ? $"{hotkeys[index]}: -"
                : $"{hotkeys[index]}: {ability.DisplayName}\nЭнергия: {ability.EnergyCost}";

            if (ability != null && cooldown > 0f)
            {
                label += $" | КД: {cooldown:0.0}с";
            }

            var previousColor = GUI.backgroundColor;
            if (ability != null)
            {
                GUI.backgroundColor = preview != null && preview.IsUsable
                    ? AbilityController.GetAbilityColor(ability)
                    : new Color(0.45f, 0.45f, 0.45f);
            }

            if (GUI.Button(rect, label, buttonStyle))
            {
                abilityController?.UseAbilitySlot(index);
            }

            GUI.backgroundColor = previousColor;

            if (ability == null)
            {
                return;
            }

            var status = GetStatusText(preview);
            GUI.Label(new Rect(x + 4f, y + 53f, 202f, 20f), status, statusStyle);

            if (!string.IsNullOrWhiteSpace(ability.Description))
            {
                GUI.Label(new Rect(x + 4f, y + 75f, 202f, 34f), ability.Description, bodyStyle);
            }
        }

        private static string GetStatusText(AbilityTargetPreview preview)
        {
            if (preview == null)
            {
                return "Недоступно";
            }

            if (!preview.IsUsable)
            {
                return string.IsNullOrWhiteSpace(preview.UnavailableReason)
                    ? "Недоступно"
                    : preview.UnavailableReason;
            }

            return string.IsNullOrWhiteSpace(preview.TargetLabel)
                ? "Готово"
                : $"Цель: {preview.TargetLabel}";
        }

        private void CacheReferences()
        {
            if (gameManager == null) gameManager = FindAnyObjectByType<GameManager>();
            if (partyManager == null) partyManager = FindAnyObjectByType<PartyManager>();
            if (abilityController == null) abilityController = FindAnyObjectByType<AbilityController>();
        }

        private static GUIStyle CreateTitleStyle()
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            style.normal.textColor = Color.white;
            return style;
        }

        private static GUIStyle CreateBodyStyle()
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 10,
                wordWrap = true
            };
            style.normal.textColor = new Color(0.78f, 0.86f, 1f);
            return style;
        }

        private static GUIStyle CreateStatusStyle()
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                wordWrap = true
            };
            style.normal.textColor = new Color(1f, 0.88f, 0.45f);
            return style;
        }

        private static GUIStyle CreateButtonStyle()
        {
            var style = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                wordWrap = true,
                alignment = TextAnchor.MiddleCenter
            };
            return style;
        }
    }
}
