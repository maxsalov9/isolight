using UnityEngine;

namespace IsoLight.UI
{
    public class InteractionPromptUI : MonoBehaviour
    {
        [SerializeField] private Vector2 screenPosition = new Vector2(24f, 24f);
        [SerializeField] private Vector2 size = new Vector2(260f, 34f);

        private string currentText;
        private Object currentOwner;
        private GUIStyle promptStyle;

        public void Show(string text, Object owner)
        {
            currentText = text;
            currentOwner = owner;
        }

        public void Hide(Object owner)
        {
            if (currentOwner == owner)
            {
                currentText = null;
                currentOwner = null;
            }
        }

        private void OnGUI()
        {
            if (string.IsNullOrEmpty(currentText))
            {
                return;
            }

            promptStyle ??= CreatePromptStyle();
            GUI.Box(new Rect(screenPosition, size), currentText, promptStyle);
        }

        private static GUIStyle CreatePromptStyle()
        {
            var style = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 16,
                padding = new RectOffset(12, 12, 4, 4)
            };
            style.normal.textColor = Color.white;
            return style;
        }
    }
}
