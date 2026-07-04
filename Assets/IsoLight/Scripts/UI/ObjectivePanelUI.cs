using IsoLight.Quests;
using UnityEngine;

namespace IsoLight.UI
{
    public class ObjectivePanelUI : MonoBehaviour
    {
        [SerializeField] private QuestManager questManager;
        [SerializeField] private Vector2 screenPosition = new Vector2(24f, 68f);
        [SerializeField] private Vector2 size = new Vector2(420f, 58f);

        private ObjectiveData activeObjective;
        private GUIStyle panelStyle;

        public void SetQuestManager(QuestManager manager)
        {
            if (questManager != null)
            {
                questManager.ObjectiveChanged -= HandleObjectiveChanged;
            }

            questManager = manager;

            if (questManager != null)
            {
                questManager.ObjectiveChanged += HandleObjectiveChanged;
                activeObjective = questManager.ActiveObjective;
            }
        }

        private void Start()
        {
            SetQuestManager(questManager != null ? questManager : FindAnyObjectByType<QuestManager>());
        }

        private void OnDestroy()
        {
            if (questManager != null)
            {
                questManager.ObjectiveChanged -= HandleObjectiveChanged;
            }
        }

        private void OnGUI()
        {
            if (activeObjective == null || activeObjective.Status != ObjectiveStatus.Active)
            {
                return;
            }

            panelStyle ??= CreatePanelStyle();
            GUI.Box(new Rect(screenPosition, size), $"Цель: {activeObjective.Description}", panelStyle);
        }

        private void HandleObjectiveChanged(ObjectiveData objective)
        {
            activeObjective = objective;
        }

        private static GUIStyle CreatePanelStyle()
        {
            var style = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 16,
                wordWrap = true,
                padding = new RectOffset(12, 12, 6, 6)
            };
            style.normal.textColor = Color.white;
            return style;
        }
    }
}
