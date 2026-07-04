using System.Collections.Generic;
using IsoLight.Core;
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
        [SerializeField] private RelationshipManager relationshipManager;
        [SerializeField] private PowerAllocationBoardUI powerAllocationBoardUI;
        [SerializeField] private ResultPanelUI resultPanelUI;
        [SerializeField] private NotificationUI notificationUI;

        public int AvailableStableOutput => availableStableOutput;
        public IReadOnlyList<PowerSystemData> PowerSystems => powerSystems;

        public void SetReferences(
            GameManager game,
            RelationshipManager relationships,
            PowerAllocationBoardUI allocationBoard,
            ResultPanelUI resultPanel)
        {
            gameManager = game;
            relationshipManager = relationships;
            powerAllocationBoardUI = allocationBoard;
            resultPanelUI = resultPanel;
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
            missionState.MissionCompleted = true;

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
            ApplyVisualResultState(choice);

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
                gameManager.CurrentGameMode = GameMode.Exploration;
            }

            notificationUI?.ShowMessage("Riverside mission result recorded.");
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

        private static void ApplyVisualResultState(PowerChoice choice)
        {
            var targetName = choice switch
            {
                PowerChoice.WaterFilters => "Filter",
                PowerChoice.HydroponicFarm => "Farm",
                PowerChoice.DefenseGrid => "Defense",
                PowerChoice.PublicStage => "Stage",
                PowerChoice.Workshop => "Workshop",
                PowerChoice.RelayStation => "Relay",
                PowerChoice.PartyReserve => "Shelter",
                PowerChoice.SplitLoad => string.Empty,
                _ => string.Empty
            };

            var lights = FindObjectsByType<Light>(FindObjectsInactive.Exclude);
            foreach (var sceneLight in lights)
            {
                if (choice == PowerChoice.SplitLoad || (!string.IsNullOrEmpty(targetName) && sceneLight.name.Contains(targetName)))
                {
                    sceneLight.intensity *= 1.25f;
                }
            }
        }
    }
}
