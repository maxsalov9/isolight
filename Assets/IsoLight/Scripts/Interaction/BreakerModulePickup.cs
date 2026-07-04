using IsoLight.Characters;
using IsoLight.Core;
using IsoLight.Quests;
using IsoLight.UI;
using UnityEngine;

namespace IsoLight.Interaction
{
    public class BreakerModulePickup : InteractableObject
    {
        private const int RequiredBreakerModules = 2;
        private const string FindBreakerModulesObjectiveId = "find_breaker_modules";
        private const string RepairGeneratorLineObjectiveId = "repair_generator_line";

        [SerializeField] private GameManager gameManager;
        [SerializeField] private QuestManager questManager;
        [SerializeField] private NotificationUI notificationUI;
        [SerializeField] private int moduleCount = 1;

        private bool collected;

        protected override void Awake()
        {
            base.Awake();
            CacheReferences();
        }

        public override void Interact(PlayerCharacter character)
        {
            if (collected || !CanInteract(character))
            {
                return;
            }

            CacheReferences();

            if (gameManager == null)
            {
                return;
            }

            collected = true;
            gameManager.MissionState.BreakerModulesCollected = Mathf.Clamp(
                gameManager.MissionState.BreakerModulesCollected + moduleCount,
                0,
                RequiredBreakerModules);

            UpdateBreakerObjective();
            notificationUI?.ShowMessage($"Breaker-модуль найден: {gameManager.MissionState.BreakerModulesCollected}/{RequiredBreakerModules}");
            gameObject.SetActive(false);
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

        private void UpdateBreakerObjective()
        {
            if (questManager == null)
            {
                return;
            }

            var collectedCount = gameManager.MissionState.BreakerModulesCollected;
            questManager.UpdateObjectiveDescription(
                FindBreakerModulesObjectiveId,
                $"Найти 2 breaker-модуля: {collectedCount}/{RequiredBreakerModules}");

            if (collectedCount >= RequiredBreakerModules)
            {
                questManager.CompleteObjective(FindBreakerModulesObjectiveId);
                questManager.ActivateObjective(RepairGeneratorLineObjectiveId);
            }
            else
            {
                questManager.ActivateObjective(FindBreakerModulesObjectiveId);
            }
        }

        protected override bool CanShowPrompt(PlayerCharacter character)
        {
            return !collected;
        }
    }
}
