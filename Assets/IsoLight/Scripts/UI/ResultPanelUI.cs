using System;
using IsoLight.Power;
using IsoLight.Relationships;
using UnityEngine;

namespace IsoLight.UI
{
    public class ResultPanelUI : MonoBehaviour
    {
        [SerializeField] private Rect panelRect = new Rect(160f, 90f, 780f, 520f);

        private PowerAllocationResult result;
        private Action continueRequested;
        private GUIStyle titleStyle;
        private GUIStyle labelStyle;
        private GUIStyle bodyStyle;
        private GUIStyle buttonStyle;

        public void Show(PowerAllocationResult allocationResult, Action onContinueRequested)
        {
            result = allocationResult;
            continueRequested = onContinueRequested;
        }

        public void Hide()
        {
            result = null;
            continueRequested = null;
        }

        private void OnGUI()
        {
            if (result == null)
            {
                return;
            }

            EnsureStyles();

            GUI.Box(panelRect, GUIContent.none);
            var x = panelRect.x + 18f;
            var y = panelRect.y + 16f;
            var width = panelRect.width - 36f;

            GUI.Label(new Rect(x, y, width, 32f), "Решение принято", titleStyle);
            y += 42f;
            GUI.Label(new Rect(x, y, width, 28f), $"Выбор: {result.ChosenSystem.DisplayName}", labelStyle);
            y += 36f;
            GUI.Label(new Rect(x, y, width, 72f), result.ConsequenceText, bodyStyle);
            y += 82f;
            GUI.Label(new Rect(x, y, width, 28f), $"Довольны: {result.SupportersText}", labelStyle);
            y += 34f;
            GUI.Label(new Rect(x, y, width, 46f), $"Недовольны: {result.OppositionText}", bodyStyle);
            y += 58f;
            GUI.Label(new Rect(x, y, width, 28f), "Изменение отношений:", labelStyle);
            y += 28f;

            foreach (var change in result.RelationshipChanges)
            {
                GUI.Label(new Rect(x, y, width, 24f), $"{FormatGroupName(change.GroupName)}: {FormatLevel(change.PreviousLevel)} -> {FormatLevel(change.NewLevel)}", bodyStyle);
                y += 24f;
            }

            if (result.Choice == PowerChoice.PartyReserve)
            {
                y += 8f;
                GUI.Label(new Rect(x, y, width, 48f), $"Резерв отряда заряжен: {result.PartyPowerCellsCharge}. Cormac молчит, и это звучит хуже обвинения.", bodyStyle);
                y += 56f;
            }

            GUI.Label(new Rect(x, panelRect.y + panelRect.height - 86f, width, 28f), "Миссия Riverside завершена. Последствия будут развиваться в следующих batch.", labelStyle);
            if (GUI.Button(new Rect(panelRect.x + panelRect.width - 180f, panelRect.y + panelRect.height - 48f, 150f, 32f), "Продолжить", buttonStyle))
            {
                continueRequested?.Invoke();
            }
        }

        private static string FormatLevel(RelationshipLevel level)
        {
            return level switch
            {
                RelationshipLevel.Low => "низкое",
                RelationshipLevel.Medium => "среднее",
                RelationshipLevel.High => "высокое",
                _ => level.ToString()
            };
        }

        private static string FormatGroupName(string groupName)
        {
            return groupName switch
            {
                "RiversideTrust" => "Доверие Riverside",
                "DefenseSupport" => "Поддержка обороны",
                "FarmSupport" => "Поддержка фермы",
                "MedicalSupport" => "Поддержка медиков",
                "StageSupport" => "Поддержка сцены",
                "WorkshopSupport" => "Поддержка мастерской",
                "RelaySupport" => "Поддержка реле",
                _ => groupName
            };
        }

        private void EnsureStyles()
        {
            titleStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            labelStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                normal = { textColor = Color.white }
            };
            bodyStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                wordWrap = true,
                normal = { textColor = new Color(0.88f, 0.92f, 0.92f) }
            };
            buttonStyle ??= new GUIStyle(GUI.skin.button)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold
            };
        }
    }
}
