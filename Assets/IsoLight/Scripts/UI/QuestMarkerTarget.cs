using System;
using IsoLight.Quests;
using UnityEngine;

namespace IsoLight.UI
{
    public class QuestMarkerTarget : MonoBehaviour
    {
        [SerializeField] private QuestManager questManager;
        [SerializeField] private string markerText = "Цель";
        [SerializeField] private string[] objectiveIds = Array.Empty<string>();
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2.2f, 0f);
        [SerializeField] private Color markerColor = new Color(1f, 0.85f, 0.25f);

        private GUIStyle markerStyle;

        public void Configure(string text, string[] activeObjectiveIds, Vector3 offset, Color color)
        {
            markerText = text;
            objectiveIds = activeObjectiveIds ?? Array.Empty<string>();
            worldOffset = offset;
            markerColor = color;
        }

        public void SetQuestManager(QuestManager manager)
        {
            questManager = manager;
        }

        private void Awake()
        {
            CacheReferences();
        }

        private void OnGUI()
        {
            CacheReferences();

            if (questManager == null || !IsRelevantObjective() || UnityEngine.Camera.main == null)
            {
                return;
            }

            var screenPosition = UnityEngine.Camera.main.WorldToScreenPoint(transform.position + worldOffset);
            if (screenPosition.z <= 0f)
            {
                return;
            }

            markerStyle ??= CreateMarkerStyle();
            markerStyle.normal.textColor = markerColor;

            var label = $"▼ {markerText}";
            var size = markerStyle.CalcSize(new GUIContent(label));
            var rect = new Rect(screenPosition.x - size.x * 0.5f - 10f, Screen.height - screenPosition.y - 20f, size.x + 20f, 26f);
            GUI.Box(rect, label, markerStyle);
        }

        private bool IsRelevantObjective()
        {
            var activeObjective = questManager.ActiveObjective;
            if (activeObjective == null || activeObjective.Status != ObjectiveStatus.Active || objectiveIds == null)
            {
                return false;
            }

            for (var i = 0; i < objectiveIds.Length; i++)
            {
                if (activeObjective.Id == objectiveIds[i])
                {
                    return true;
                }
            }

            return false;
        }

        private void CacheReferences()
        {
            if (questManager == null)
            {
                questManager = FindAnyObjectByType<QuestManager>();
            }
        }

        private static GUIStyle CreateMarkerStyle()
        {
            var style = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(8, 8, 3, 3)
            };
            return style;
        }
    }
}
