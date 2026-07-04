using IsoLight.Characters;
using IsoLight.Core;
using IsoLight.Power;
using IsoLight.Quests;
using IsoLight.UI;
using UnityEngine;

namespace IsoLight.Interaction
{
    public class SwitchRoomConsole : InteractableObject
    {
        private const string AllocatePowerObjectiveId = "allocate_power";

        [SerializeField] private GameManager gameManager;
        [SerializeField] private QuestManager questManager;
        [SerializeField] private PowerManager powerManager;
        [SerializeField] private NotificationUI notificationUI;

        protected override void Awake()
        {
            base.Awake();
            CacheReferences();
        }

        public override void Interact(PlayerCharacter character)
        {
            if (!base.CanInteract(character))
            {
                return;
            }

            CacheReferences();

            if (gameManager == null)
            {
                return;
            }

            if (!gameManager.MissionState.GeneratorDefended)
            {
                const string lockedMessage = "Консоль Switch Room заблокирована, пока Generator G-17 не защищен.";
                Debug.Log(lockedMessage);
                notificationUI?.ShowMessage(lockedMessage);
                return;
            }

            questManager?.ActivateObjective(AllocatePowerObjectiveId);
            powerManager?.OpenPowerAllocationBoard();
        }

        protected override string GetPromptText(PlayerCharacter character)
        {
            CacheReferences();

            if (gameManager != null && !gameManager.MissionState.GeneratorDefended)
            {
                return "Консоль заблокирована: сначала защитите Generator G-17.";
            }

            return $"{PromptPrefix} Открыть распределение энергии";
        }

        public void SetReferences(GameManager game, QuestManager quest, PowerManager power, NotificationUI notifications)
        {
            gameManager = game;
            questManager = quest;
            powerManager = power;
            notificationUI = notifications;
        }

        [ContextMenu("Debug Mark Generator Defended")]
        public void DebugMarkGeneratorDefended()
        {
            CacheReferences();
            if (gameManager != null)
            {
                gameManager.MissionState.GeneratorDefended = true;
                Debug.Log("Debug: Generator G-17 marked as defended from SwitchRoomConsole.");
            }
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

            if (powerManager == null)
            {
                powerManager = FindAnyObjectByType<PowerManager>();
            }

            if (notificationUI == null)
            {
                notificationUI = FindAnyObjectByType<NotificationUI>();
            }
        }
    }
}
