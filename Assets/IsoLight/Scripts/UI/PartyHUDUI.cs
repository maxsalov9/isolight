using IsoLight.Party;
using UnityEngine;

namespace IsoLight.UI
{
    public class PartyHUDUI : MonoBehaviour
    {
        [SerializeField] private PartyManager partyManager;
        [SerializeField] private Vector2 screenPosition = new Vector2(24f, 450f);
        [SerializeField] private Vector2 panelSize = new Vector2(210f, 128f);
        [SerializeField] private float gap = 8f;

        private GUIStyle boxStyle;
        private GUIStyle labelStyle;

        public void SetPartyManager(PartyManager manager)
        {
            partyManager = manager;
        }

        private void Awake()
        {
            if (partyManager == null)
            {
                partyManager = FindAnyObjectByType<PartyManager>();
            }
        }

        private void OnGUI()
        {
            if (partyManager == null || partyManager.PartyMembers == null)
            {
                return;
            }

            boxStyle ??= CreateBoxStyle();
            labelStyle ??= CreateLabelStyle();

            var members = partyManager.PartyMembers;
            for (var i = 0; i < members.Count; i++)
            {
                var rect = new Rect(screenPosition.x + i * (panelSize.x + gap), screenPosition.y, panelSize.x, panelSize.y);
                CharacterHUDPanel.Draw(rect, members[i], members[i] == partyManager.ActiveCharacter, boxStyle, labelStyle);
            }
        }

        private static GUIStyle CreateBoxStyle()
        {
            var style = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(8, 8, 6, 6)
            };
            return style;
        }

        private static GUIStyle CreateLabelStyle()
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                wordWrap = true
            };
            style.normal.textColor = Color.white;
            return style;
        }
    }
}
