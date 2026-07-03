using IsoLight.Camera;
using IsoLight.Combat;
using IsoLight.Core;
using IsoLight.Dialogue;
using IsoLight.Input;
using IsoLight.Party;
using IsoLight.Power;
using IsoLight.Quests;
using IsoLight.Relationships;
using IsoLight.Save;
using IsoLight.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IsoLight.Editor
{
    public static class IsoLightPrototypeSceneBuilder
    {
        private const string ScenePath = "Assets/IsoLight/Scenes/Prototype/Riverside_Prototype.unity";

        [MenuItem("IsoLight/Build Riverside Prototype Scene")]
        public static void BuildRiversidePrototypeScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Riverside_Prototype";

            var gameRoot = CreateRoot("_Game");
            var levelRoot = CreateRoot("_Level");
            var uiRoot = CreateRoot("_UI");
            var playerPartyRoot = CreateRoot("_PlayerParty");
            var npcsRoot = CreateRoot("_NPCs");
            var enemiesRoot = CreateRoot("_Enemies");
            var interactablesRoot = CreateRoot("_Interactables");

            var gameManager = CreateManager<GameManager>("GameManager", gameRoot.transform);
            var inputManager = CreateManager<InputManager>("InputManager", gameRoot.transform);
            var cameraManager = CreateManager<CameraManager>("CameraManager", gameRoot.transform);
            var partyManager = CreateManager<PartyManager>("PartyManager", gameRoot.transform);
            var questManager = CreateManager<QuestManager>("QuestManager", gameRoot.transform);
            var dialogueManager = CreateManager<DialogueManager>("DialogueManager", gameRoot.transform);
            var combatManager = CreateManager<CombatManager>("CombatManager", gameRoot.transform);
            var powerManager = CreateManager<PowerManager>("PowerManager", gameRoot.transform);
            var relationshipManager = CreateManager<RelationshipManager>("RelationshipManager", gameRoot.transform);
            var saveManager = CreateManager<SaveManager>("SaveManager", gameRoot.transform);
            var uiManager = CreateManager<UIManager>("UIManager", gameRoot.transform);

            AssignManagerReferences(
                gameManager,
                inputManager,
                cameraManager,
                partyManager,
                questManager,
                dialogueManager,
                combatManager,
                powerManager,
                relationshipManager,
                saveManager,
                uiManager);

            _ = levelRoot;
            _ = uiRoot;
            _ = playerPartyRoot;
            _ = npcsRoot;
            _ = enemiesRoot;
            _ = interactablesRoot;

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
        }

        private static GameObject CreateRoot(string name)
        {
            return new GameObject(name);
        }

        private static T CreateManager<T>(string name, Transform parent) where T : Component
        {
            var managerObject = new GameObject(name);
            managerObject.transform.SetParent(parent);
            return managerObject.AddComponent<T>();
        }

        private static void AssignManagerReferences(
            GameManager gameManager,
            InputManager inputManager,
            CameraManager cameraManager,
            PartyManager partyManager,
            QuestManager questManager,
            DialogueManager dialogueManager,
            CombatManager combatManager,
            PowerManager powerManager,
            RelationshipManager relationshipManager,
            SaveManager saveManager,
            UIManager uiManager)
        {
            var serializedGameManager = new SerializedObject(gameManager);
            serializedGameManager.FindProperty("inputManager").objectReferenceValue = inputManager;
            serializedGameManager.FindProperty("cameraManager").objectReferenceValue = cameraManager;
            serializedGameManager.FindProperty("partyManager").objectReferenceValue = partyManager;
            serializedGameManager.FindProperty("questManager").objectReferenceValue = questManager;
            serializedGameManager.FindProperty("dialogueManager").objectReferenceValue = dialogueManager;
            serializedGameManager.FindProperty("combatManager").objectReferenceValue = combatManager;
            serializedGameManager.FindProperty("powerManager").objectReferenceValue = powerManager;
            serializedGameManager.FindProperty("relationshipManager").objectReferenceValue = relationshipManager;
            serializedGameManager.FindProperty("saveManager").objectReferenceValue = saveManager;
            serializedGameManager.FindProperty("uiManager").objectReferenceValue = uiManager;
            serializedGameManager.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
