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

            var abilityCount = Mathf.Min(activeCharacter.Abilities.Count, hotkeys.Length);
            var width = 196f * abilityCount + 16f;
            var rect = new Rect(Screen.width * 0.5f - width * 0.5f, Screen.height - 108f, width, 92f);
            GUI.Box(rect, string.Empty, panelStyle);
            GUI.Label(new Rect(rect.x + 10f, rect.y + 6f, rect.width - 20f, 20f), $"Способности: {activeCharacter.DisplayName}", titleStyle);

            for (var i = 0; i < abilityCount; i++)
            {
                DrawAbilityButton(activeCharacter, activeCharacter.Abilities[i], i, rect.x + 10f + i * 196f, rect.y + 32f);
            }
        }

        private void DrawAbilityButton(PlayerCharacter character, AbilityData ability, int index, float x, float y)
        {
            var rect = new Rect(x, y, 186f, 48f);
            var cooldown = character.GetAbilityCooldownRemaining(ability);
            var label = ability == null
                ? $"{hotkeys[index]}: -"
                : $"{hotkeys[index]}: {ability.DisplayName}\nЭнергия {ability.EnergyCost}";

            if (ability != null && cooldown > 0f)
            {
                label += $" | {cooldown:0.0}с";
            }

            var previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled && ability != null && gameManager != null && gameManager.CurrentGameMode == GameMode.Combat && cooldown <= 0f && character.HasEnoughEnergy(ability);
            if (GUI.Button(rect, label, buttonStyle))
            {
                abilityController?.UseAbilitySlot(index);
            }

            GUI.enabled = previousEnabled;

            if (ability != null && !string.IsNullOrWhiteSpace(ability.Description))
            {
                GUI.Label(new Rect(x + 4f, y + 50f, 180f, 18f), ability.Description, bodyStyle);
            }
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
                wordWrap = false
            };
            style.normal.textColor = new Color(0.78f, 0.86f, 1f);
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
