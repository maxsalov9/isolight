using IsoLight.Characters;
using IsoLight.Core;
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
                const string lockedMessage = "Switch Room Console locked until Generator G-17 is defended.";
                Debug.Log(lockedMessage);
                notificationUI?.ShowMessage(lockedMessage);
                return;
            }

            questManager?.ActivateObjective(AllocatePowerObjectiveId);

            const string unlockedMessage = "Power Allocation Board will be implemented in Batch 6.";
            Debug.Log(unlockedMessage);
            notificationUI?.ShowMessage(unlockedMessage);
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
    }
}
