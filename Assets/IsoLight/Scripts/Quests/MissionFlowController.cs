using IsoLight.Core;
using IsoLight.Party;
using UnityEngine;

namespace IsoLight.Quests
{
    public class MissionFlowController : MonoBehaviour
    {
        public const string ReachShelterObjectiveId = "reach_riverside_shelter";
        public const string SpeakWithMaraObjectiveId = "speak_with_mara";
        public const string ResolvePowerPrioritiesObjectiveId = "resolve_power_priorities";
        public const string InspectKeySystemsObjectiveId = "inspect_key_systems";
        public const string FindBreakerModulesObjectiveId = "find_breaker_modules";
        public const string RepairGeneratorLineObjectiveId = "repair_generator_line";
        public const string StartGeneratorObjectiveId = "start_generator_g17";
        public const string DefendGeneratorObjectiveId = "defend_generator_g17";
        public const string AllocatePowerObjectiveId = "allocate_power";
        public const string FaceConsequencesObjectiveId = "face_consequences";
        public const string LeaveRiversideObjectiveId = "leave_riverside";

        private const int RequiredInspections = 6;
        private const int RequiredBreakerModules = 2;

        [SerializeField] private GameManager gameManager;
        [SerializeField] private QuestManager questManager;
        [SerializeField] private PartyManager partyManager;
        [SerializeField] private Vector3 shelterPosition = new Vector3(0f, 0f, -6f);
        [SerializeField] private float shelterRadius = 4f;

        public void SetReferences(GameManager game, QuestManager quests, PartyManager party)
        {
            gameManager = game;
            questManager = quests;
            partyManager = party;
        }

        private void Awake()
        {
            CacheReferences();
        }

        private void Update()
        {
            CacheReferences();

            if (gameManager == null || questManager == null || questManager.ActiveQuest == null)
            {
                return;
            }

            var missionState = gameManager.MissionState;
            CompleteReachShelterIfNeeded();
            AdvanceAfterMara(missionState);
            AdvanceAfterInspections(missionState);
            AdvanceAfterBreakerModules(missionState);
            AdvanceAfterGeneratorRepair(missionState);
            AdvanceAfterGeneratorStart(missionState);
            AdvanceAfterGeneratorDefense(missionState);
            AdvanceAfterPowerChoice(missionState);
        }

        private void CompleteReachShelterIfNeeded()
        {
            if (!IsActive(ReachShelterObjectiveId) || partyManager == null)
            {
                return;
            }

            var members = partyManager.PartyMembers;
            for (var i = 0; i < members.Count; i++)
            {
                var member = members[i];
                if (member != null && Vector3.Distance(member.transform.position, shelterPosition) <= shelterRadius)
                {
                    CompleteAndActivate(ReachShelterObjectiveId, SpeakWithMaraObjectiveId);
                    return;
                }
            }
        }

        private void AdvanceAfterMara(MissionState missionState)
        {
            if (!missionState.HasMetMara)
            {
                return;
            }

            CompleteIfNotCompleted(SpeakWithMaraObjectiveId);

            if (!IsCompleted(ResolvePowerPrioritiesObjectiveId)
                && !IsActive(ResolvePowerPrioritiesObjectiveId)
                && !HasAnyInspectedSystem(missionState))
            {
                questManager.ActivateObjective(ResolvePowerPrioritiesObjectiveId);
            }
        }

        private void AdvanceAfterInspections(MissionState missionState)
        {
            var inspectedCount = CountInspectedSystems(missionState);
            if (inspectedCount <= 0)
            {
                return;
            }

            CompleteIfNotCompleted(ResolvePowerPrioritiesObjectiveId);

            if (inspectedCount >= RequiredInspections)
            {
                questManager.UpdateObjectiveDescription(InspectKeySystemsObjectiveId, $"Осмотреть ключевые системы: {inspectedCount}/{RequiredInspections}");
                CompleteAndActivate(InspectKeySystemsObjectiveId, FindBreakerModulesObjectiveId);
            }
            else if (IsActive(ResolvePowerPrioritiesObjectiveId))
            {
                questManager.ActivateObjective(InspectKeySystemsObjectiveId);
            }
        }

        private void AdvanceAfterBreakerModules(MissionState missionState)
        {
            if (missionState.BreakerModulesCollected < RequiredBreakerModules)
            {
                return;
            }

            CompleteAndActivate(FindBreakerModulesObjectiveId, RepairGeneratorLineObjectiveId);
        }

        private void AdvanceAfterGeneratorRepair(MissionState missionState)
        {
            if (missionState.GeneratorRepaired)
            {
                CompleteAndActivate(RepairGeneratorLineObjectiveId, StartGeneratorObjectiveId);
            }
        }

        private void AdvanceAfterGeneratorStart(MissionState missionState)
        {
            if (missionState.GeneratorStarted)
            {
                CompleteAndActivate(StartGeneratorObjectiveId, DefendGeneratorObjectiveId);
            }
        }

        private void AdvanceAfterGeneratorDefense(MissionState missionState)
        {
            if (missionState.GeneratorDefended)
            {
                CompleteAndActivate(DefendGeneratorObjectiveId, AllocatePowerObjectiveId);
            }
        }

        private void AdvanceAfterPowerChoice(MissionState missionState)
        {
            if (missionState.FinalPowerChoice == Power.PowerChoice.None)
            {
                return;
            }

            CompleteIfNotCompleted(AllocatePowerObjectiveId);

            if (missionState.MissionCompleted)
            {
                CompleteIfNotCompleted(FaceConsequencesObjectiveId);
                CompleteIfNotCompleted(LeaveRiversideObjectiveId);
            }
        }

        private void CompleteAndActivate(string completedObjectiveId, string nextObjectiveId)
        {
            CompleteIfNotCompleted(completedObjectiveId);

            if (!IsCompleted(nextObjectiveId) && !IsActive(nextObjectiveId))
            {
                questManager.ActivateObjective(nextObjectiveId);
            }
        }

        private void CompleteIfNotCompleted(string objectiveId)
        {
            if (!IsCompleted(objectiveId))
            {
                questManager.CompleteObjective(objectiveId);
            }
        }

        private bool IsActive(string objectiveId)
        {
            return questManager.GetObjective(objectiveId)?.Status == ObjectiveStatus.Active;
        }

        private bool IsCompleted(string objectiveId)
        {
            return questManager.GetObjective(objectiveId)?.Status == ObjectiveStatus.Completed;
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

            if (partyManager == null)
            {
                partyManager = FindAnyObjectByType<PartyManager>();
            }
        }

        public static int CountInspectedSystems(MissionState missionState)
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

        private static bool HasAnyInspectedSystem(MissionState missionState)
        {
            return CountInspectedSystems(missionState) > 0;
        }
    }
}
