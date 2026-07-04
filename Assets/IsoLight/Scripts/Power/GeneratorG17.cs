using IsoLight.Characters;
using IsoLight.Combat;
using IsoLight.Core;
using IsoLight.Interaction;
using IsoLight.Quests;
using IsoLight.UI;
using UnityEngine;

namespace IsoLight.Power
{
    public class GeneratorG17 : InteractableObject, IDamageable
    {
        private const string RepairObjectiveId = "repair_generator_line";

        [SerializeField] private int maxHealth = 250;
        [SerializeField] private int currentHealth = 250;
        [SerializeField] private bool isRepaired;
        [SerializeField] private bool isStarted;
        [SerializeField] private GameManager gameManager;
        [SerializeField] private CombatManager combatManager;
        [SerializeField] private QuestManager questManager;
        [SerializeField] private NotificationUI notificationUI;

        private float lastDamageTime = -100f;

        public int MaxHealth => maxHealth;
        public int CurrentHealth => currentHealth;
        public bool IsAlive => currentHealth > 0;
        public bool IsRepaired => isRepaired;
        public bool IsStarted => isStarted;
        public bool WasRecentlyDamaged => Time.time - lastDamageTime <= 2f;

        protected override void Awake()
        {
            base.Awake();
            CacheReferences();
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        }

        public void SetReferences(GameManager game, CombatManager combat, QuestManager quest, NotificationUI notifications)
        {
            gameManager = game;
            combatManager = combat;
            questManager = quest;
            notificationUI = notifications;
        }

        public override void Interact(PlayerCharacter character)
        {
            if (!base.CanInteract(character))
            {
                return;
            }

            CacheReferences();

            if (!isRepaired)
            {
                TryRepair();
                return;
            }

            if (!isStarted)
            {
                StartGenerator();
                return;
            }

            notificationUI?.ShowMessage("Generator G-17 уже работает.");
        }

        protected override string GetPromptText(PlayerCharacter character)
        {
            CacheReferences();

            if (!isRepaired && gameManager != null && gameManager.MissionState.BreakerModulesCollected < 2)
            {
                return "Generator G-17 требует 2 breaker-модуля.";
            }

            if (!isRepaired)
            {
                return $"{PromptPrefix} Починить Generator G-17";
            }

            if (!isStarted)
            {
                return $"{PromptPrefix} Запустить Generator G-17";
            }

            return "Generator G-17 уже работает.";
        }

        public void TakeDamage(int amount)
        {
            if (amount <= 0 || !IsAlive)
            {
                return;
            }

            currentHealth = Mathf.Max(0, currentHealth - amount);
            lastDamageTime = Time.time;
            notificationUI?.ShowMessage($"Generator G-17 под ударом: {currentHealth}/{maxHealth} HP.");
            if (currentHealth <= 0)
            {
                combatManager?.HandleGeneratorDestroyed();
            }
        }

        public void RepairHealth(int amount)
        {
            if (amount <= 0 || !IsAlive)
            {
                return;
            }

            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        }

        public void DebugRepairForPlaytest()
        {
            CacheReferences();
            isRepaired = true;
            currentHealth = maxHealth;
            if (gameManager != null)
            {
                gameManager.MissionState.BreakerModulesCollected = Mathf.Max(2, gameManager.MissionState.BreakerModulesCollected);
                gameManager.MissionState.GeneratorRepaired = true;
            }

            questManager?.CompleteObjective(RepairObjectiveId);
            questManager?.ActivateObjective(MissionFlowController.StartGeneratorObjectiveId);
        }

        public void DebugStartForPlaytest()
        {
            CacheReferences();
            if (!isRepaired)
            {
                DebugRepairForPlaytest();
            }

            isStarted = true;
            if (gameManager != null)
            {
                gameManager.MissionState.GeneratorStarted = true;
            }

            questManager?.CompleteObjective(MissionFlowController.StartGeneratorObjectiveId);
            questManager?.ActivateObjective(MissionFlowController.DefendGeneratorObjectiveId);
        }

        private void TryRepair()
        {
            if (gameManager == null)
            {
                return;
            }

            if (gameManager.MissionState.BreakerModulesCollected < 2)
            {
                notificationUI?.ShowMessage("Generator G-17 требует 2 breaker-модуля для ремонта.");
                return;
            }

            isRepaired = true;
            currentHealth = maxHealth;
            gameManager.MissionState.GeneratorRepaired = true;
            questManager?.CompleteObjective(RepairObjectiveId);
            questManager?.ActivateObjective(MissionFlowController.StartGeneratorObjectiveId);
            notificationUI?.ShowMessage("Generator G-17 отремонтирован. Взаимодействуйте еще раз, чтобы запустить его.");
        }

        private void StartGenerator()
        {
            if (gameManager == null)
            {
                return;
            }

            isStarted = true;
            gameManager.MissionState.GeneratorStarted = true;
            questManager?.CompleteObjective(MissionFlowController.StartGeneratorObjectiveId);
            questManager?.ActivateObjective(MissionFlowController.DefendGeneratorObjectiveId);
            notificationUI?.ShowMessage("Generator G-17 запущен. Рейдеры приближаются.");
            combatManager?.StartCombat();
        }

        private void CacheReferences()
        {
            if (gameManager == null)
            {
                gameManager = FindAnyObjectByType<GameManager>();
            }

            if (combatManager == null)
            {
                combatManager = FindAnyObjectByType<CombatManager>();
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
    }
}
