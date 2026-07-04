using System;
using System.Collections.Generic;
using IsoLight.Dialogue;
using UnityEngine;

namespace IsoLight.UI
{
    public class DialoguePanelUI : MonoBehaviour
    {
        [SerializeField] private Rect panelRect = new Rect(120f, 360f, 760f, 260f);

        private DialogueNode currentNode;
        private Action<int> choiceSelected;
        private Action closeRequested;
        private GUIStyle speakerStyle;
        private GUIStyle textStyle;
        private GUIStyle companionStyle;
        private GUIStyle buttonStyle;

        public void Show(DialogueNode node, Action<int> onChoiceSelected, Action onCloseRequested)
        {
            currentNode = node;
            choiceSelected = onChoiceSelected;
            closeRequested = onCloseRequested;
        }

        public void Hide()
        {
            currentNode = null;
            choiceSelected = null;
            closeRequested = null;
        }

        private void OnGUI()
        {
            if (currentNode == null)
            {
                return;
            }

            EnsureStyles();

            GUI.Box(panelRect, GUIContent.none);
            var contentRect = new Rect(panelRect.x + 16f, panelRect.y + 12f, panelRect.width - 32f, panelRect.height - 24f);
            GUI.Label(new Rect(contentRect.x, contentRect.y, contentRect.width, 28f), currentNode.SpeakerName, speakerStyle);
            GUI.Label(new Rect(contentRect.x, contentRect.y + 34f, contentRect.width, 80f), currentNode.Text, textStyle);

            var y = contentRect.y + 116f;
            if (!string.IsNullOrWhiteSpace(currentNode.CompanionComment))
            {
                GUI.Label(new Rect(contentRect.x, y, contentRect.width, 28f), currentNode.CompanionComment, companionStyle);
                y += 34f;
            }

            DrawChoices(currentNode.Choices, contentRect.x, y, contentRect.width);
        }

        private void DrawChoices(IReadOnlyList<DialogueChoice> choices, float x, float y, float width)
        {
            if (choices == null || choices.Count == 0)
            {
                if (GUI.Button(new Rect(x, y, 180f, 32f), "Закрыть", buttonStyle))
                {
                    closeRequested?.Invoke();
                }

                return;
            }

            for (var i = 0; i < choices.Count; i++)
            {
                var rect = new Rect(x, y + i * 34f, width, 30f);
                if (GUI.Button(rect, choices[i].Text, buttonStyle))
                {
                    choiceSelected?.Invoke(i);
                }
            }
        }

        private void EnsureStyles()
        {
            speakerStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            textStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                wordWrap = true,
                normal = { textColor = Color.white }
            };
            companionStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Italic,
                wordWrap = true,
                normal = { textColor = new Color(0.78f, 0.9f, 1f) }
            };
            buttonStyle ??= new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 14,
                padding = new RectOffset(10, 10, 4, 4)
            };
        }
    }
}
