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

        public int MaxHealth => maxHealth;
        public int CurrentHealth => currentHealth;
        public bool IsAlive => currentHealth > 0;
        public bool IsRepaired => isRepaired;
        public bool IsStarted => isStarted;

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

            notificationUI?.ShowMessage("Generator G-17 is already running.");
        }

        public void TakeDamage(int amount)
        {
            if (amount <= 0 || !IsAlive)
            {
                return;
            }

            currentHealth = Mathf.Max(0, currentHealth - amount);
            if (currentHealth <= 0)
            {
                combatManager?.HandleGeneratorDestroyed();
            }
        }

        private void TryRepair()
        {
            if (gameManager == null)
            {
                return;
            }

            if (gameManager.MissionState.BreakerModulesCollected < 2)
            {
                notificationUI?.ShowMessage("Generator G-17 needs 2 breaker modules before repair.");
                return;
            }

            isRepaired = true;
            currentHealth = maxHealth;
            gameManager.MissionState.GeneratorRepaired = true;
            questManager?.CompleteObjective(RepairObjectiveId);
            notificationUI?.ShowMessage("Generator G-17 repaired. Interact again to start it.");
        }

        private void StartGenerator()
        {
            if (gameManager == null)
            {
                return;
            }

            isStarted = true;
            gameManager.MissionState.GeneratorStarted = true;
            notificationUI?.ShowMessage("Generator G-17 started. Raiders incoming.");
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
