using System.Collections.Generic;
using IsoLight.Quests;
using UnityEngine;

namespace IsoLight.UI
{
    public class MissionHintNotifier : MonoBehaviour
    {
        [SerializeField] private QuestManager questManager;
        [SerializeField] private NotificationUI notificationUI;

        private readonly HashSet<string> shownObjectiveHints = new HashSet<string>();

        public void SetReferences(QuestManager quests, NotificationUI notifications)
        {
            if (questManager != null)
            {
                questManager.ObjectiveChanged -= HandleObjectiveChanged;
            }

            questManager = quests;
            notificationUI = notifications;

            if (questManager != null)
            {
                questManager.ObjectiveChanged += HandleObjectiveChanged;
            }
        }

        private void Start()
        {
            SetReferences(
                questManager != null ? questManager : FindAnyObjectByType<QuestManager>(),
                notificationUI != null ? notificationUI : FindAnyObjectByType<NotificationUI>());
        }

        private void OnDestroy()
        {
            if (questManager != null)
            {
                questManager.ObjectiveChanged -= HandleObjectiveChanged;
            }
        }

        private void HandleObjectiveChanged(ObjectiveData objective)
        {
            if (objective == null || objective.Status != ObjectiveStatus.Active || notificationUI == null)
            {
                return;
            }

            if (!shownObjectiveHints.Add(objective.Id))
            {
                return;
            }

            var hint = GetHint(objective.Id);
            if (!string.IsNullOrWhiteSpace(hint))
            {
                notificationUI.ShowMessage(hint);
            }
        }

        private static string GetHint(string objectiveId)
        {
            return objectiveId switch
            {
                MissionFlowController.SpeakWithMaraObjectiveId => "Поговорите с Mara в убежище.",
                MissionFlowController.ResolvePowerPrioritiesObjectiveId => "Узнайте энергетические приоритеты жителей Riverside.",
                MissionFlowController.InspectKeySystemsObjectiveId => "Осмотрите системы поселения.",
                MissionFlowController.FindBreakerModulesObjectiveId => "Найдите 2 breaker-модуля.",
                MissionFlowController.RepairGeneratorLineObjectiveId => "Вернитесь к Generator G-17.",
                MissionFlowController.StartGeneratorObjectiveId => "Генератор починен. Запустите его.",
                MissionFlowController.DefendGeneratorObjectiveId => "Генератор запущен. Защитите его.",
                MissionFlowController.AllocatePowerObjectiveId => "Генератор защищен. Идите к Switch Room Console.",
                MissionFlowController.FaceConsequencesObjectiveId => "Посмотрите последствия решения.",
                MissionFlowController.LeaveRiversideObjectiveId => "Миссия Riverside завершена.",
                _ => null
            };
        }
    }
}
