using UnityEngine;

namespace IsoLight.UI
{
    public class NotificationUI : MonoBehaviour
    {
        [SerializeField] private Vector2 screenPosition = new Vector2(24f, 136f);
        [SerializeField] private Vector2 size = new Vector2(520f, 48f);
        [SerializeField] private float messageDuration = 3f;

        private string currentMessage;
        private float hideAtTime;
        private GUIStyle notificationStyle;

        public void ShowMessage(string message)
        {
            currentMessage = message;
            hideAtTime = Time.time + messageDuration;
        }

        private void OnGUI()
        {
            if (string.IsNullOrEmpty(currentMessage) || Time.time >= hideAtTime)
            {
                return;
            }

            notificationStyle ??= CreateNotificationStyle();
            GUI.Box(new Rect(screenPosition, size), currentMessage, notificationStyle);
        }

        private static GUIStyle CreateNotificationStyle()
        {
            var style = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 15,
                wordWrap = true,
                padding = new RectOffset(12, 12, 6, 6)
            };
            style.normal.textColor = Color.white;
            return style;
        }
    }
}
