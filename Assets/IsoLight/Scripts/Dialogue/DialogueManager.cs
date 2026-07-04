using System;
using System.Linq;
using IsoLight.Core;
using IsoLight.Quests;
using IsoLight.UI;
using UnityEngine;

namespace IsoLight.Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private QuestManager questManager;
        [SerializeField] private DialoguePanelUI dialoguePanelUI;

        private DialogueData activeDialogue;
        private DialogueNode currentNode;

        public DialogueData ActiveDialogue => activeDialogue;
        public DialogueNode CurrentNode => currentNode;

        public void SetReferences(GameManager game, QuestManager quests, DialoguePanelUI panel)
        {
            gameManager = game;
            questManager = quests;
            dialoguePanelUI = panel;
        }

        private void Awake()
        {
            if (gameManager == null)
            {
                gameManager = FindAnyObjectByType<GameManager>();
            }

            if (questManager == null)
            {
                questManager = FindAnyObjectByType<QuestManager>();
            }

            if (dialoguePanelUI == null)
            {
                dialoguePanelUI = FindAnyObjectByType<DialoguePanelUI>();
            }
        }

        public void StartDialogue(DialogueData dialogueData)
        {
            if (dialogueData == null || dialogueData.Nodes.Count == 0)
            {
                return;
            }

            activeDialogue = dialogueData;
            gameManager.CurrentGameMode = GameMode.Dialogue;
            ShowNode(string.IsNullOrEmpty(dialogueData.StartNodeId) ? dialogueData.Nodes[0].NodeId : dialogueData.StartNodeId);
        }

        public void SelectChoice(int choiceIndex)
        {
            if (currentNode == null || choiceIndex < 0 || choiceIndex >= currentNode.Choices.Count)
            {
                return;
            }

            var choice = currentNode.Choices[choiceIndex];
            ApplyEffects(choice.Effects);

            if (string.IsNullOrEmpty(choice.NextNodeId))
            {
                CloseDialogue();
                return;
            }

            ShowNode(choice.NextNodeId);
        }

        public void CloseDialogue()
        {
            activeDialogue = null;
            currentNode = null;
            dialoguePanelUI?.Hide();

            if (gameManager != null)
            {
                gameManager.CurrentGameMode = GameMode.Exploration;
            }
        }

        private void ShowNode(string nodeId)
        {
            currentNode = activeDialogue.Nodes.FirstOrDefault(node => node.NodeId == nodeId);
            if (currentNode == null)
            {
                CloseDialogue();
                return;
            }

            ApplyEffects(currentNode.Effects);
            dialoguePanelUI?.Show(currentNode, SelectChoice, CloseDialogue);
        }

        private void ApplyEffects(System.Collections.Generic.IEnumerable<DialogueEffect> effects)
        {
            foreach (var effect in effects)
            {
                ApplyEffect(effect);
            }
        }

        private void ApplyEffect(DialogueEffect effect)
        {
            if (effect == null)
            {
                return;
            }

            switch (effect.Type)
            {
                case DialogueEffectType.SetMissionFlag:
                    SetMissionFlag(effect.Key, effect.BoolValue);
                    break;
                case DialogueEffectType.StartQuest:
                    if (effect.Quest != null)
                    {
                        questManager?.StartQuest(effect.Quest);
                    }
                    break;
                case DialogueEffectType.ActivateObjective:
                    questManager?.ActivateObjective(effect.Key);
                    break;
                case DialogueEffectType.CompleteObjective:
                    questManager?.CompleteObjective(effect.Key);
                    break;
                case DialogueEffectType.CloseDialogue:
                    CloseDialogue();
                    break;
            }
        }

        private void SetMissionFlag(string flag, bool value)
        {
            if (gameManager == null || string.IsNullOrEmpty(flag))
            {
                return;
            }

            var missionState = gameManager.MissionState;
            switch (flag)
            {
                case "has_met_mara":
                case "HasMetMara":
                    missionState.HasMetMara = value;
                    break;
                case "power_priorities_started":
                case "HasStartedPowerPrioritiesQuest":
                    missionState.HasStartedPowerPrioritiesQuest = value;
                    break;
                case "talked_to_hale":
                case "TalkedToHale":
                    missionState.TalkedToHale = value;
                    break;
                case "talked_to_ivo":
                case "TalkedToIvo":
                    missionState.TalkedToIvo = value;
                    break;
                case "talked_to_sela":
                case "TalkedToSela":
                    missionState.TalkedToSela = value;
                    break;
                case "talked_to_edda":
                case "TalkedToEdda":
                    missionState.TalkedToEdda = value;
                    break;
                case "talked_to_greer":
                case "TalkedToGreer":
                    missionState.TalkedToGreer = value;
                    break;
                case "talked_to_lysa":
                case "TalkedToLysa":
                    missionState.TalkedToLysa = value;
                    break;
            }
        }
    }
}
