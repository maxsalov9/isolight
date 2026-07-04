using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IsoLight.Quests
{
    public class QuestManager : MonoBehaviour
    {
        [SerializeField] private QuestData startupQuest;
        [SerializeField] private string startupObjectiveId;

        private readonly List<ObjectiveData> runtimeObjectives = new List<ObjectiveData>();

        public event Action<QuestData> QuestStarted;
        public event Action<ObjectiveData> ObjectiveChanged;

        public QuestData ActiveQuest { get; private set; }
        public ObjectiveData ActiveObjective => runtimeObjectives.FirstOrDefault(objective => objective.Status == ObjectiveStatus.Active);
        public IReadOnlyList<ObjectiveData> RuntimeObjectives => runtimeObjectives;

        public void SetStartupQuest(QuestData quest, string objectiveId)
        {
            startupQuest = quest;
            startupObjectiveId = objectiveId;
        }

        private void Start()
        {
            if (startupQuest != null)
            {
                StartQuest(startupQuest);

                if (!string.IsNullOrEmpty(startupObjectiveId))
                {
                    ActivateObjective(startupObjectiveId);
                }
            }
        }

        public void StartQuest(QuestData quest)
        {
            ActiveQuest = quest;
            runtimeObjectives.Clear();

            if (quest == null)
            {
                ObjectiveChanged?.Invoke(null);
                return;
            }

            foreach (var objective in quest.Objectives)
            {
                runtimeObjectives.Add(new ObjectiveData(objective)
                {
                    Status = ObjectiveStatus.Hidden
                });
            }

            QuestStarted?.Invoke(ActiveQuest);

            if (runtimeObjectives.Count > 0)
            {
                ActivateObjective(runtimeObjectives[0].Id);
            }
            else
            {
                ObjectiveChanged?.Invoke(null);
            }
        }

        public void ActivateObjective(string objectiveId)
        {
            var objective = GetObjective(objectiveId);
            if (objective == null)
            {
                return;
            }

            foreach (var runtimeObjective in runtimeObjectives)
            {
                if (runtimeObjective.Status == ObjectiveStatus.Active)
                {
                    runtimeObjective.Status = ObjectiveStatus.Hidden;
                }
            }

            objective.Status = ObjectiveStatus.Active;
            ObjectiveChanged?.Invoke(objective);
        }

        public void CompleteObjective(string objectiveId)
        {
            var objective = GetObjective(objectiveId);
            if (objective == null)
            {
                return;
            }

            objective.Status = ObjectiveStatus.Completed;
            ObjectiveChanged?.Invoke(ActiveObjective);
        }

        public void UpdateObjectiveDescription(string objectiveId, string description)
        {
            var objective = GetObjective(objectiveId);
            if (objective == null)
            {
                return;
            }

            objective.Description = description;

            if (objective.Status == ObjectiveStatus.Active)
            {
                ObjectiveChanged?.Invoke(objective);
            }
        }

        public ObjectiveData GetObjective(string objectiveId)
        {
            if (string.IsNullOrEmpty(objectiveId))
            {
                return null;
            }

            return runtimeObjectives.FirstOrDefault(objective => objective.Id == objectiveId);
        }
    }
}
