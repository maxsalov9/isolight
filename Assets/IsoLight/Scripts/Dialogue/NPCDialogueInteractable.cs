using IsoLight.Characters;
using IsoLight.Interaction;
using UnityEngine;

namespace IsoLight.Dialogue
{
    public class NPCDialogueInteractable : InteractableObject
    {
        [SerializeField] private DialogueData dialogueData;
        [SerializeField] private DialogueManager dialogueManager;

        protected override void Awake()
        {
            base.Awake();

            if (dialogueManager == null)
            {
                dialogueManager = FindAnyObjectByType<DialogueManager>();
            }
        }

        public void SetDialogue(DialogueData dialogue)
        {
            dialogueData = dialogue;
        }

        public override void Interact(PlayerCharacter character)
        {
            if (!CanInteract(character))
            {
                return;
            }

            if (dialogueManager == null)
            {
                dialogueManager = FindAnyObjectByType<DialogueManager>();
            }

            if (dialogueData != null)
            {
                dialogueManager?.StartDialogue(dialogueData);
            }
        }
    }
}
