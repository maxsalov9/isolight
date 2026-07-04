using System;
using System.Collections.Generic;
using IsoLight.Power;
using UnityEngine;

namespace IsoLight.UI
{
    public class PowerAllocationBoardUI : MonoBehaviour
    {
        [SerializeField] private Rect panelRect = new Rect(80f, 48f, 980f, 620f);

        private PowerManager powerManager;
        private IReadOnlyList<PowerSystemData> powerSystems;
        private PowerSystemData selectedSystem;
        private Vector2 scrollPosition;
        private bool isVisible;
        private GUIStyle titleStyle;
        private GUIStyle labelStyle;
        private GUIStyle smallStyle;
        private GUIStyle optionStyle;
        private GUIStyle selectedOptionStyle;
        private GUIStyle buttonStyle;

        public void Show(PowerManager manager, IReadOnlyList<PowerSystemData> systems)
        {
            powerManager = manager;
            powerSystems = systems;
            selectedSystem = systems != null && systems.Count > 0 ? systems[0] : null;
            scrollPosition = Vector2.zero;
            isVisible = true;
        }

        public void Hide()
        {
            isVisible = false;
        }

        private void OnGUI()
        {
            if (!isVisible)
            {
                return;
            }

            EnsureStyles();

            GUI.Box(panelRect, GUIContent.none);
            var content = new Rect(panelRect.x + 18f, panelRect.y + 16f, panelRect.width - 36f, panelRect.height - 32f);
            GUI.Label(new Rect(content.x, content.y, content.width, 34f), "Распределение энергии Riverside", titleStyle);
            GUI.Label(new Rect(content.x, content.y + 38f, content.width, 24f), $"Доступная стабильная мощность: {powerManager.AvailableStableOutput}%", labelStyle);

            DrawSystemList(new Rect(content.x, content.y + 74f, 330f, content.height - 126f));
            DrawSelectedSystem(new Rect(content.x + 352f, content.y + 74f, content.width - 352f, content.height - 126f));

            var wasEnabled = GUI.enabled;
            GUI.enabled = selectedSystem != null;
            if (GUI.Button(new Rect(content.x + content.width - 220f, content.y + content.height - 40f, 220f, 34f), "Подтвердить выбор", buttonStyle))
            {
                powerManager.ApplyPowerChoice(selectedSystem.PowerChoice);
            }
            GUI.enabled = wasEnabled;
        }

        private void DrawSystemList(Rect rect)
        {
            GUI.Label(new Rect(rect.x, rect.y, rect.width, 24f), "Варианты", labelStyle);

            var listRect = new Rect(rect.x, rect.y + 30f, rect.width, rect.height - 30f);
            var viewRect = new Rect(0f, 0f, rect.width - 20f, Mathf.Max(40f, (powerSystems?.Count ?? 0) * 42f));
            scrollPosition = GUI.BeginScrollView(listRect, scrollPosition, viewRect);

            if (powerSystems != null)
            {
                for (var i = 0; i < powerSystems.Count; i++)
                {
                    var system = powerSystems[i];
                    var optionRect = new Rect(0f, i * 42f, viewRect.width, 36f);
                    var isSelected = selectedSystem == system;
                    if (PowerSystemOptionUI.DrawOption(optionRect, system, isSelected, isSelected ? selectedOptionStyle : optionStyle))
                    {
                        selectedSystem = system;
                    }
                }
            }

            GUI.EndScrollView();
        }

        private void DrawSelectedSystem(Rect rect)
        {
            if (selectedSystem == null)
            {
                GUI.Label(rect, "Нет доступных систем.", labelStyle);
                return;
            }

            GUI.Box(rect, GUIContent.none);
            var x = rect.x + 14f;
            var y = rect.y + 12f;
            var width = rect.width - 28f;

            GUI.Label(new Rect(x, y, width, 28f), selectedSystem.DisplayName, titleStyle);
            y += 38f;
            GUI.Label(new Rect(x, y, width, 24f), $"Требуется энергии: {selectedSystem.RequiredPower}%", labelStyle);
            y += 32f;
            GUI.Label(new Rect(x, y, width, 58f), selectedSystem.Description, smallStyle);
            y += 68f;
            GUI.Label(new Rect(x, y, width, 58f), $"Плюс: {selectedSystem.BenefitDescription}", smallStyle);
            y += 68f;
            GUI.Label(new Rect(x, y, width, 58f), $"Цена: {selectedSystem.DownsideDescription}", smallStyle);
            y += 70f;
            GUI.Label(new Rect(x, y, width, 26f), $"Поддержит: {selectedSystem.SupportingNPCId}", labelStyle);
            y += 30f;
            GUI.Label(new Rect(x, y, width, 52f), $"Против: {FormatOpposition(selectedSystem.OpposingNPCIds)}", smallStyle);
        }

        private static string FormatOpposition(IReadOnlyList<string> opposition)
        {
            return opposition == null || opposition.Count == 0 ? "нет явного противника" : string.Join(", ", opposition);
        }

        private void EnsureStyles()
        {
            titleStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            labelStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                wordWrap = true,
                normal = { textColor = Color.white }
            };
            smallStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                wordWrap = true,
                normal = { textColor = new Color(0.86f, 0.9f, 0.92f) }
            };
            optionStyle ??= new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 14,
                padding = new RectOffset(10, 10, 4, 4)
            };
            selectedOptionStyle ??= new GUIStyle(optionStyle)
            {
                fontStyle = FontStyle.Bold
            };
            selectedOptionStyle.normal.textColor = new Color(1f, 0.8f, 0.35f);
            buttonStyle ??= new GUIStyle(GUI.skin.button)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold
            };
        }
    }
}
