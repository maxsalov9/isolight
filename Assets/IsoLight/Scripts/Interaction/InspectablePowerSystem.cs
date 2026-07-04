using IsoLight.Characters;
using IsoLight.Core;
using IsoLight.Quests;
using IsoLight.UI;
using UnityEngine;

namespace IsoLight.Interaction
{
    public class InspectablePowerSystem : InteractableObject
    {
        private const string InspectKeySystemsObjectiveId = "inspect_key_systems";
        private const string FindBreakerModulesObjectiveId = "find_breaker_modules";
        private const int RequiredInspections = 6;

        [SerializeField] private PowerSystemType systemType;
        [SerializeField] private GameManager gameManager;
        [SerializeField] private QuestManager questManager;
        [SerializeField] private NotificationUI notificationUI;
        [TextArea]
        [SerializeField] private string inspectionMessage;

        private bool inspected;

        protected override void Awake()
        {
            base.Awake();
            CacheReferences();
        }

        public void ConfigurePowerSystem(PowerSystemType type, string message)
        {
            systemType = type;
            inspectionMessage = message;
        }

        public override void Interact(PlayerCharacter character)
        {
            if (!CanInteract(character))
            {
                return;
            }

            CacheReferences();

            if (gameManager == null)
            {
                return;
            }

            SetMissionFlag(gameManager.MissionState);
            inspected = true;
            var marker = GetComponent<QuestMarkerTarget>();
            if (marker != null)
            {
                marker.enabled = false;
            }

            var inspectedCount = CountInspectedSystems(gameManager.MissionState);
            questManager?.UpdateObjectiveDescription(
                InspectKeySystemsObjectiveId,
                $"Осмотреть ключевые системы: {inspectedCount}/{RequiredInspections}");

            if (inspectedCount >= RequiredInspections)
            {
                questManager?.CompleteObjective(InspectKeySystemsObjectiveId);
                questManager?.ActivateObjective(FindBreakerModulesObjectiveId);
            }
            else
            {
                questManager?.ActivateObjective(InspectKeySystemsObjectiveId);
            }

            var message = string.IsNullOrWhiteSpace(inspectionMessage)
                ? $"{InteractionName}: осмотр завершен."
                : inspectionMessage;
            notificationUI?.ShowMessage(message);
        }

        public override bool CanInteract(PlayerCharacter character)
        {
            return !inspected && base.CanInteract(character);
        }

        protected override string GetPromptText(PlayerCharacter character)
        {
            return $"{PromptPrefix} {GetInspectionPrompt()}";
        }

        protected override bool CanShowPrompt(PlayerCharacter character)
        {
            return !inspected;
        }

        private void CacheReferences()
        {
            if (gameManager == null)
            {
                gameManager = FindAnyObjectByType<GameManager>();
            }

            if (questManager == null)
            {
                questManager = FindAnyObjectByType<QuestManager>();
            }

            if (notificationUI == null)
            {
                notificationUI = FindAnyObjectByType<NotificationUI>();
            }
        }

        private void SetMissionFlag(MissionState missionState)
        {
            switch (systemType)
            {
                case PowerSystemType.WaterFilters:
                    missionState.InspectedWaterFilters = true;
                    break;
                case PowerSystemType.HydroponicFarm:
                    missionState.InspectedHydroponicFarm = true;
                    break;
                case PowerSystemType.DefenseGate:
                    missionState.InspectedDefenseGate = true;
                    break;
                case PowerSystemType.PublicStage:
                    missionState.InspectedPublicStage = true;
                    break;
                case PowerSystemType.Workshop:
                    missionState.InspectedWorkshop = true;
                    break;
                case PowerSystemType.RelayStation:
                    missionState.InspectedRelayStation = true;
                    break;
            }
        }

        private string GetInspectionPrompt()
        {
            return systemType switch
            {
                PowerSystemType.WaterFilters => "Осмотреть фильтры",
                PowerSystemType.HydroponicFarm => "Осмотреть ферму",
                PowerSystemType.DefenseGate => "Осмотреть ворота",
                PowerSystemType.PublicStage => "Осмотреть сцену",
                PowerSystemType.Workshop => "Осмотреть мастерскую",
                PowerSystemType.RelayStation => "Осмотреть реле",
                _ => InteractionName
            };
        }

        private static int CountInspectedSystems(MissionState missionState)
        {
            var count = 0;
            if (missionState.InspectedWaterFilters) count++;
            if (missionState.InspectedHydroponicFarm) count++;
            if (missionState.InspectedDefenseGate) count++;
            if (missionState.InspectedPublicStage) count++;
            if (missionState.InspectedWorkshop) count++;
            if (missionState.InspectedRelayStation) count++;
            return count;
        }
    }
}
