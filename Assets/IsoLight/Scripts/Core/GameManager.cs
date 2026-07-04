using IsoLight.Camera;
using IsoLight.Combat;
using IsoLight.Dialogue;
using IsoLight.Input;
using IsoLight.Party;
using IsoLight.Power;
using IsoLight.Quests;
using IsoLight.Relationships;
using IsoLight.Save;
using IsoLight.UI;
using UnityEngine;

namespace IsoLight.Core
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private GameMode currentGameMode = GameMode.Exploration;
        [SerializeField] private MissionState missionState = new MissionState();

        [Header("Scene Managers")]
        [SerializeField] private InputManager inputManager = null;
        [SerializeField] private CameraManager cameraManager = null;
        [SerializeField] private PartyManager partyManager = null;
        [SerializeField] private QuestManager questManager = null;
        [SerializeField] private DialogueManager dialogueManager = null;
        [SerializeField] private CombatManager combatManager = null;
        [SerializeField] private PowerManager powerManager = null;
        [SerializeField] private RelationshipManager relationshipManager = null;
        [SerializeField] private SaveManager saveManager = null;
        [SerializeField] private UIManager uiManager = null;

        public GameMode CurrentGameMode
        {
            get => currentGameMode;
            set => currentGameMode = value;
        }

        public MissionState MissionState => missionState;
        public InputManager InputManager => inputManager;
        public CameraManager CameraManager => cameraManager;
        public PartyManager PartyManager => partyManager;
        public QuestManager QuestManager => questManager;
        public DialogueManager DialogueManager => dialogueManager;
        public CombatManager CombatManager => combatManager;
        public PowerManager PowerManager => powerManager;
        public RelationshipManager RelationshipManager => relationshipManager;
        public SaveManager SaveManager => saveManager;
        public UIManager UIManager => uiManager;

        [ContextMenu("Debug Mark Generator Defended")]
        public void DebugMarkGeneratorDefended()
        {
            missionState.GeneratorDefended = true;
            Debug.Log("Debug: Generator G-17 marked as defended.");
        }
    }
}
