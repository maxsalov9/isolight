using IsoLight.Characters;
using UnityEngine;

namespace IsoLight.UI
{
    public static class CharacterHUDPanel
    {
        public static void Draw(Rect rect, PlayerCharacter character, bool selected, GUIStyle boxStyle, GUIStyle labelStyle)
        {
            GUI.Box(rect, GUIContent.none, boxStyle);

            if (character == null)
            {
                GUI.Label(new Rect(rect.x + 10f, rect.y + 10f, rect.width - 20f, 24f), "Нет персонажа", labelStyle);
                return;
            }

            var namePrefix = selected ? "> " : string.Empty;
            var state = character.IsAlive ? string.Empty : "  [без сознания]";
            GUI.Label(new Rect(rect.x + 10f, rect.y + 8f, rect.width - 20f, 22f), $"{namePrefix}{character.DisplayName}{state}", labelStyle);
            GUI.Label(new Rect(rect.x + 10f, rect.y + 30f, rect.width - 20f, 20f), character.CharacterData != null ? character.CharacterData.Role : "Роль", labelStyle);

            DrawBar(new Rect(rect.x + 10f, rect.y + 56f, rect.width - 20f, 12f), character.CurrentHealth, character.MaxHealth, new Color(0.75f, 0.12f, 0.1f));
            GUI.Label(new Rect(rect.x + 10f, rect.y + 70f, rect.width - 20f, 18f), $"HP {character.CurrentHealth}/{character.MaxHealth}", labelStyle);
            DrawBar(new Rect(rect.x + 10f, rect.y + 92f, rect.width - 20f, 10f), character.CurrentEnergy, character.MaxEnergy, new Color(0.1f, 0.48f, 0.82f));
            GUI.Label(new Rect(rect.x + 10f, rect.y + 104f, rect.width - 20f, 18f), $"Энергия {character.CurrentEnergy}/{character.MaxEnergy}", labelStyle);
        }

        private static void DrawBar(Rect rect, int current, int max, Color fillColor)
        {
            GUI.Box(rect, GUIContent.none);

            if (max <= 0)
            {
                return;
            }

            var previousColor = GUI.color;
            GUI.color = fillColor;
            GUI.DrawTexture(new Rect(rect.x + 1f, rect.y + 1f, Mathf.Max(0f, rect.width - 2f) * Mathf.Clamp01((float)current / max), rect.height - 2f), Texture2D.whiteTexture);
            GUI.color = previousColor;
        }
    }
}
