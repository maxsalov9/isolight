using System;
using System.Collections.Generic;
using IsoLight.Camera;
using IsoLight.Characters;
using IsoLight.Combat;
using IsoLight.Core;
using IsoLight.Dialogue;
using IsoLight.Input;
using IsoLight.Interaction;
using IsoLight.Party;
using IsoLight.Power;
using IsoLight.Quests;
using IsoLight.Relationships;
using IsoLight.Save;
using IsoLight.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace IsoLight.Editor
{
    public static class IsoLightPrototypeSceneBuilder
    {
        private const string ScenePath = "Assets/IsoLight/Scenes/Prototype/Riverside_Prototype.unity";
        private const string CharacterDataPath = "Assets/IsoLight/Data/Characters";
        private const string QuestDataPath = "Assets/IsoLight/Data/Quests";

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

            CreateGround(levelRoot.transform);
            TryBuildNavMesh(levelRoot);

            var cameraController = CreateMainCamera();
            var clickToMoveController = inputManager.gameObject.AddComponent<ClickToMoveController>();
            var partyCharacters = CreatePlaceholderParty(playerPartyRoot.transform);
            var questData = EnsureQuestDataAsset();

            CreatePlaceholderUI(uiRoot.transform, questManager);
            CreatePrototypeInteractable(interactablesRoot.transform);

            partyManager.SetPartyMembers(partyCharacters);
            questManager.SetStartupQuest(questData, "reach_riverside_shelter");
            inputManager.SetReferences(gameManager, partyManager);
            cameraManager.SetReferences(partyManager, cameraController);
            clickToMoveController.SetReferences(gameManager, partyManager, UnityEngine.Camera.main);

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

            _ = uiRoot;
            _ = npcsRoot;
            _ = enemiesRoot;
            _ = interactablesRoot;

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
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

        private static void CreatePlaceholderUI(Transform parent, QuestManager questManager)
        {
            var promptObject = new GameObject("InteractionPromptUI");
            promptObject.transform.SetParent(parent);
            promptObject.AddComponent<InteractionPromptUI>();

            var objectiveObject = new GameObject("ObjectivePanelUI");
            objectiveObject.transform.SetParent(parent);
            var objectivePanel = objectiveObject.AddComponent<ObjectivePanelUI>();
            objectivePanel.SetQuestManager(questManager);
        }

        private static void CreateGround(Transform parent)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground_NavMesh";
            ground.transform.SetParent(parent);
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(3f, 1f, 3f);

            var renderer = ground.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = CreateRuntimeMaterial("MAT_Runtime_Ground", new Color(0.32f, 0.36f, 0.32f));
            }
        }

        private static IsometricCameraController CreateMainCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(-8f, 10f, -8f);
            cameraObject.transform.rotation = Quaternion.Euler(60f, 45f, 0f);

            var camera = cameraObject.AddComponent<UnityEngine.Camera>();
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 200f;

            cameraObject.AddComponent<AudioListener>();
            return cameraObject.AddComponent<IsometricCameraController>();
        }

        private static List<PlayerCharacter> CreatePlaceholderParty(Transform parent)
        {
            return new List<PlayerCharacter>
            {
                CreatePlaceholderCharacter(
                    parent,
                    EnsureCharacterDataAsset("SO_Character_Dax", "dax", "Dax", "Scavenger", 100, 50),
                    new Vector3(-2f, 0f, 0f),
                    new Color(0.28f, 0.58f, 0.94f)),
                CreatePlaceholderCharacter(
                    parent,
                    EnsureCharacterDataAsset("SO_Character_Nyra", "nyra", "Nyra", "Tech", 85, 80),
                    new Vector3(0f, 0f, 0f),
                    new Color(0.4f, 0.9f, 0.7f)),
                CreatePlaceholderCharacter(
                    parent,
                    EnsureCharacterDataAsset("SO_Character_Cormac", "cormac", "Cormac", "Medic", 95, 65),
                    new Vector3(2f, 0f, 0f),
                    new Color(0.95f, 0.78f, 0.34f))
            };
        }

        private static PlayerCharacter CreatePlaceholderCharacter(
            Transform parent,
            CharacterData characterData,
            Vector3 position,
            Color color)
        {
            var characterRoot = new GameObject(characterData.DisplayName);
            characterRoot.transform.SetParent(parent);
            characterRoot.transform.position = position;

            var agent = characterRoot.AddComponent<NavMeshAgent>();
            agent.speed = 3.5f;
            agent.angularSpeed = 720f;
            agent.acceleration = 12f;
            agent.radius = 0.35f;
            agent.height = 2f;
            agent.stoppingDistance = 0.1f;

            var playerCharacter = characterRoot.AddComponent<PlayerCharacter>();
            playerCharacter.SetCharacterData(characterData);

            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.name = "Visual";
            visual.transform.SetParent(characterRoot.transform);
            visual.transform.localPosition = Vector3.up;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one;

            var visualRenderer = visual.GetComponent<Renderer>();
            if (visualRenderer != null)
            {
                visualRenderer.sharedMaterial = CreateRuntimeMaterial($"MAT_Runtime_{characterData.DisplayName}", color);
            }

            var selectionRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            selectionRing.name = "SelectionRing";
            selectionRing.transform.SetParent(characterRoot.transform);
            selectionRing.transform.localPosition = new Vector3(0f, 0.03f, 0f);
            selectionRing.transform.localScale = new Vector3(1.4f, 0.02f, 1.4f);

            var ringCollider = selectionRing.GetComponent<Collider>();
            if (ringCollider != null)
            {
                UnityEngine.Object.DestroyImmediate(ringCollider);
            }

            var ringRenderer = selectionRing.GetComponent<Renderer>();
            if (ringRenderer != null)
            {
                ringRenderer.sharedMaterial = CreateRuntimeMaterial("MAT_Runtime_SelectionRing", new Color(1f, 0.92f, 0.25f));
            }

            playerCharacter.SetSelectionIndicator(selectionRing);
            return playerCharacter;
        }

        private static void CreatePrototypeInteractable(Transform parent)
        {
            var interactableObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            interactableObject.name = "Prototype_Interactable_Notice";
            interactableObject.transform.SetParent(parent);
            interactableObject.transform.position = new Vector3(0f, 0.5f, 4f);
            interactableObject.transform.localScale = new Vector3(1.5f, 1f, 0.4f);

            var renderer = interactableObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = CreateRuntimeMaterial("MAT_Runtime_Interactable", new Color(0.45f, 0.42f, 0.35f));
            }

            var interactable = interactableObject.AddComponent<InteractableObject>();
            interactable.Configure("Inspect Notice", "[Click]", 3f);
        }

        private static CharacterData EnsureCharacterDataAsset(
            string assetName,
            string id,
            string displayName,
            string role,
            int maxHealth,
            int maxEnergy)
        {
            var path = $"{CharacterDataPath}/{assetName}.asset";
            var characterData = AssetDatabase.LoadAssetAtPath<CharacterData>(path);

            if (characterData == null)
            {
                characterData = ScriptableObject.CreateInstance<CharacterData>();
                AssetDatabase.CreateAsset(characterData, path);
            }

            characterData.Id = id;
            characterData.DisplayName = displayName;
            characterData.Role = role;
            characterData.MaxHealth = maxHealth;
            characterData.MaxEnergy = maxEnergy;
            EditorUtility.SetDirty(characterData);

            return characterData;
        }

        private static QuestData EnsureQuestDataAsset()
        {
            const string questAssetName = "SO_Quest_RestorePowerToRiverside";
            var path = $"{QuestDataPath}/{questAssetName}.asset";
            var questData = AssetDatabase.LoadAssetAtPath<QuestData>(path);

            if (questData == null)
            {
                questData = ScriptableObject.CreateInstance<QuestData>();
                AssetDatabase.CreateAsset(questData, path);
            }

            questData.Id = "restore_power_to_riverside";
            questData.Title = "Restore Power to Riverside";
            questData.Description = "Prototype quest shell for tracking the current mission objective.";
            questData.Objectives.Clear();
            questData.Objectives.Add(new ObjectiveData
            {
                Id = "reach_riverside_shelter",
                Description = "Reach Riverside Shelter",
                Status = ObjectiveStatus.Hidden
            });
            questData.Objectives.Add(new ObjectiveData
            {
                Id = "speak_with_mara",
                Description = "Speak with Mara Vey",
                Status = ObjectiveStatus.Hidden
            });
            questData.Objectives.Add(new ObjectiveData
            {
                Id = "inspect_key_systems",
                Description = "Inspect key systems",
                Status = ObjectiveStatus.Hidden
            });

            EditorUtility.SetDirty(questData);
            return questData;
        }

        private static Material CreateRuntimeMaterial(string name, Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            var material = new Material(shader)
            {
                name = name,
                color = color
            };

            return material;
        }

        private static void TryBuildNavMesh(GameObject levelRoot)
        {
            var surfaceType = Type.GetType("Unity.AI.Navigation.NavMeshSurface, Unity.AI.Navigation");
            if (surfaceType == null)
            {
                return;
            }

            var surface = levelRoot.AddComponent(surfaceType);
            var buildMethod = surfaceType.GetMethod("BuildNavMesh", Type.EmptyTypes);
            buildMethod?.Invoke(surface, null);
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
