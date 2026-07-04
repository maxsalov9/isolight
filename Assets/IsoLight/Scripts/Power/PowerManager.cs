using System.Collections.Generic;
using IsoLight.Core;
using IsoLight.Quests;
using IsoLight.Relationships;
using IsoLight.UI;
using UnityEngine;

namespace IsoLight.Power
{
    public class PowerManager : MonoBehaviour
    {
        [SerializeField] private int availableStableOutput = 100;
        [SerializeField] private List<PowerSystemData> powerSystems = new List<PowerSystemData>();
        [SerializeField] private GameManager gameManager;
        [SerializeField] private QuestManager questManager;
        [SerializeField] private RelationshipManager relationshipManager;
        [SerializeField] private PowerAllocationBoardUI powerAllocationBoardUI;
        [SerializeField] private ResultPanelUI resultPanelUI;
        [SerializeField] private NotificationUI notificationUI;
        [SerializeField] private PowerResultVisualController resultVisualController;

        public int AvailableStableOutput => availableStableOutput;
        public IReadOnlyList<PowerSystemData> PowerSystems => powerSystems;

        public void SetReferences(
            GameManager game,
            RelationshipManager relationships,
            PowerAllocationBoardUI allocationBoard,
            ResultPanelUI resultPanel)
        {
            SetReferences(game, null, relationships, allocationBoard, resultPanel, null);
        }

        public void SetReferences(
            GameManager game,
            QuestManager quests,
            RelationshipManager relationships,
            PowerAllocationBoardUI allocationBoard,
            ResultPanelUI resultPanel,
            PowerResultVisualController visualController)
        {
            gameManager = game;
            questManager = quests;
            relationshipManager = relationships;
            powerAllocationBoardUI = allocationBoard;
            resultPanelUI = resultPanel;
            resultVisualController = visualController;
        }

        public void SetPowerSystems(IEnumerable<PowerSystemData> systems)
        {
            powerSystems.Clear();
            if (systems == null)
            {
                return;
            }

            powerSystems.AddRange(systems);
        }

        public void OpenPowerAllocationBoard()
        {
            CacheReferences();

            if (gameManager == null || powerAllocationBoardUI == null)
            {
                Debug.LogWarning("Power allocation board cannot open because references are missing.");
                return;
            }

            gameManager.CurrentGameMode = GameMode.PowerAllocation;
            notificationUI?.ShowMessage("Выберите, кому достанется энергия.");
            powerAllocationBoardUI.Show(this, powerSystems);
        }

        public void ApplyPowerChoice(PowerChoice choice)
        {
            CacheReferences();

            var chosenSystem = FindSystem(choice);
            if (gameManager == null || chosenSystem == null)
            {
                return;
            }

            var missionState = gameManager.MissionState;
            missionState.FinalPowerChoice = choice;

            if (choice == PowerChoice.PartyReserve)
            {
                missionState.PartyPowerCellsCharge += Mathf.Max(10, chosenSystem.RequiredPower);
                missionState.PartySelfishnessSeen = true;
            }

            var result = BuildResult(chosenSystem);
            var changes = relationshipManager != null
                ? relationshipManager.ApplyPowerChoice(choice)
                : new List<RelationshipChange>();
            result.RelationshipChanges.AddRange(changes);

            powerAllocationBoardUI?.Hide();
            questManager?.CompleteObjective(MissionFlowController.AllocatePowerObjectiveId);
            questManager?.ActivateObjective(MissionFlowController.FaceConsequencesObjectiveId);
            resultVisualController?.Apply(choice);

            if (resultPanelUI != null)
            {
                resultPanelUI.Show(result, CloseResultPanel);
            }
            else
            {
                gameManager.CurrentGameMode = GameMode.Exploration;
            }
        }

        private void CloseResultPanel()
        {
            resultPanelUI?.Hide();

            if (gameManager != null)
            {
                gameManager.MissionState.MissionCompleted = true;
                gameManager.CurrentGameMode = GameMode.Exploration;
            }

            questManager?.CompleteObjective(MissionFlowController.FaceConsequencesObjectiveId);
            questManager?.ActivateObjective(MissionFlowController.LeaveRiversideObjectiveId);
            questManager?.CompleteObjective(MissionFlowController.LeaveRiversideObjectiveId);
            notificationUI?.ShowMessage("Решение Riverside записано. Миссия завершена.");
        }

        private PowerAllocationResult BuildResult(PowerSystemData chosenSystem)
        {
            var missionState = gameManager.MissionState;
            return new PowerAllocationResult
            {
                ChosenSystem = chosenSystem,
                Choice = chosenSystem.PowerChoice,
                ConsequenceText = GetConsequenceText(chosenSystem.PowerChoice),
                SupportersText = chosenSystem.SupportingNPCId,
                OppositionText = chosenSystem.OpposingNPCIds == null || chosenSystem.OpposingNPCIds.Count == 0
                    ? "никто открыто не возражает"
                    : string.Join(", ", chosenSystem.OpposingNPCIds),
                PartyPowerCellsCharge = missionState.PartyPowerCellsCharge,
                PartySelfishnessSeen = missionState.PartySelfishnessSeen,
                MissionCompleted = missionState.MissionCompleted
            };
        }

        private PowerSystemData FindSystem(PowerChoice choice)
        {
            for (var i = 0; i < powerSystems.Count; i++)
            {
                if (powerSystems[i] != null && powerSystems[i].PowerChoice == choice)
                {
                    return powerSystems[i];
                }
            }

            return null;
        }

        private void CacheReferences()
        {
            if (gameManager == null)
            {
                gameManager = FindAnyObjectByType<GameManager>();
            }

            if (relationshipManager == null)
            {
                relationshipManager = FindAnyObjectByType<RelationshipManager>();
            }

            if (questManager == null)
            {
                questManager = FindAnyObjectByType<QuestManager>();
            }

            if (powerAllocationBoardUI == null)
            {
                powerAllocationBoardUI = FindAnyObjectByType<PowerAllocationBoardUI>();
            }

            if (resultPanelUI == null)
            {
                resultPanelUI = FindAnyObjectByType<ResultPanelUI>();
            }

            if (notificationUI == null)
            {
                notificationUI = FindAnyObjectByType<NotificationUI>();
            }

            if (resultVisualController == null)
            {
                resultVisualController = FindAnyObjectByType<PowerResultVisualController>();
            }
        }

        private static string GetConsequenceText(PowerChoice choice)
        {
            return choice switch
            {
                PowerChoice.WaterFilters => "Фильтры получают стабильный ток. Вода станет безопаснее, но ферма останется на грани.",
                PowerChoice.HydroponicFarm => "Ферма удержит урожай и шанс на зиму. Больным придется ждать чистой воды дольше.",
                PowerChoice.DefenseGrid => "Забор снова держит периметр. Riverside будет спать спокойнее, но не теплее.",
                PowerChoice.PublicStage => "Сцена оживает как общий огонь. Практики назовут это роскошью, пока люди снова собираются вместе.",
                PowerChoice.Workshop => "Мастерская получает питание для ремонта. Сломанное начнут чинить, но не все переживут ожидание.",
                PowerChoice.RelayStation => "Реле пробует поймать дальний сигнал. Riverside получает шанс услышать внешний мир и новый страх не получить ответ.",
                PowerChoice.PartyReserve => "Часть энергии уходит в переносные ячейки отряда. Это полезно вам и почти невозможно оправдать перед Riverside.",
                PowerChoice.SplitLoad => "Нагрузка разделена осторожно. Никто не получает все, но Riverside видит попытку не бросить ни одну сторону.",
                _ => "Решение записано."
            };
        }
    }
}
