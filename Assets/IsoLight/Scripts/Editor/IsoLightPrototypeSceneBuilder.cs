using System;
using System.Collections.Generic;
using IsoLight.Camera;
using IsoLight.Characters;
using IsoLight.Combat;
using IsoLight.Core;
using IsoLight.Dialogue;
using IsoLight.Enemies;
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
        private const string EnemyDataPath = "Assets/IsoLight/Data/Enemies";
        private const string QuestDataPath = "Assets/IsoLight/Data/Quests";
        private const string DialogueDataPath = "Assets/IsoLight/Data/Dialogues";
        private const string AbilityDataPath = "Assets/IsoLight/Data/Abilities";
        private const string PowerSystemDataPath = "Assets/IsoLight/Data/PowerSystems";
        private const string MaterialsPath = "Assets/IsoLight/Materials";

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
            var materials = EnsureMinimalMaterials();

            var gameManager = CreateManager<GameManager>("GameManager", gameRoot.transform);
            var inputManager = CreateManager<InputManager>("InputManager", gameRoot.transform);
            var cameraManager = CreateManager<CameraManager>("CameraManager", gameRoot.transform);
            var partyManager = CreateManager<PartyManager>("PartyManager", gameRoot.transform);
            var questManager = CreateManager<QuestManager>("QuestManager", gameRoot.transform);
            var missionFlowController = CreateManager<MissionFlowController>("MissionFlowController", gameRoot.transform);
            var dialogueManager = CreateManager<DialogueManager>("DialogueManager", gameRoot.transform);
            var combatManager = CreateManager<CombatManager>("CombatManager", gameRoot.transform);
            var powerManager = CreateManager<PowerManager>("PowerManager", gameRoot.transform);
            var abilityController = CreateManager<AbilityController>("AbilityController", gameRoot.transform);
            var relationshipManager = CreateManager<RelationshipManager>("RelationshipManager", gameRoot.transform);
            var saveManager = CreateManager<SaveManager>("SaveManager", gameRoot.transform);
            var uiManager = CreateManager<UIManager>("UIManager", gameRoot.transform);

            CreateGround(levelRoot.transform, materials);
            CreateLevelBlockout(levelRoot.transform, materials);
            TryBuildNavMesh(levelRoot);
            CreateSceneLighting(materials);
            var resultVisualController = CreatePowerResultVisuals(levelRoot.transform, materials);

            var cameraController = CreateMainCamera();
            var clickToMoveController = inputManager.gameObject.AddComponent<ClickToMoveController>();
            var abilityAssets = EnsureAbilityAssets();
            var partyCharacters = CreatePlaceholderParty(playerPartyRoot.transform, materials, abilityAssets);
            var questData = EnsureQuestDataAsset();
            var dialogueAssets = EnsureDialogueAssets(questData);
            var powerSystemAssets = EnsurePowerSystemAssets();
            var enemyDataAssets = EnsureEnemyDataAssets();

            var uiRefs = CreatePlaceholderUI(uiRoot.transform, questManager);
            var generator = CreateQuestInteractables(interactablesRoot.transform, materials, gameManager, questManager, powerManager, combatManager, uiRefs.NotificationUI);
            CreatePlaceholderNpcs(npcsRoot.transform, dialogueAssets, materials, questManager);

            partyManager.SetPartyMembers(partyCharacters);
            uiRefs.PartyHUDUI.SetPartyManager(partyManager);
            questManager.SetStartupQuest(questData, MissionFlowController.ReachShelterObjectiveId);
            missionFlowController.SetReferences(gameManager, questManager, partyManager);
            inputManager.SetReferences(gameManager, partyManager);
            cameraManager.SetReferences(partyManager, cameraController);
            clickToMoveController.SetReferences(gameManager, partyManager, UnityEngine.Camera.main);
            dialogueManager.SetReferences(gameManager, questManager, uiRefs.DialoguePanelUI);
            powerManager.SetReferences(gameManager, questManager, relationshipManager, uiRefs.PowerAllocationBoardUI, uiRefs.ResultPanelUI, resultVisualController);
            powerManager.SetPowerSystems(powerSystemAssets);
            abilityController.SetReferences(gameManager, partyManager, combatManager, generator, uiRefs.NotificationUI);
            uiRefs.AbilityBarUI.SetReferences(gameManager, partyManager, abilityController);
            uiRefs.AbilityTargetIndicatorUI.SetReferences(gameManager, partyManager, abilityController);
            uiRefs.MissionHintNotifier.SetReferences(questManager, uiRefs.NotificationUI);
            uiRefs.FailurePanelUI.SetCombatManager(combatManager);
            uiRefs.PlaytestDebugPanelUI.SetReferences(gameManager, questManager, combatManager, powerManager, generator, uiRefs.NotificationUI);
            combatManager.SetReferences(gameManager, partyManager, questManager, generator, uiRefs.NotificationUI, uiRefs.CombatStatusUI, uiRefs.GeneratorStatusUI, uiRefs.FailurePanelUI);
            combatManager.SetEnemyData(enemyDataAssets);
            combatManager.SetEnemyParent(enemiesRoot.transform);
            combatManager.SetSpawnPoints(new[]
            {
                new Vector3(-4.6f, 0f, 7.1f),
                new Vector3(4.6f, 0f, 7.1f),
                new Vector3(-5.2f, 0f, 1.2f),
                new Vector3(5.2f, 0f, 1.2f),
                new Vector3(0f, 0f, 8.8f)
            });

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

        private sealed class MinimalMaterials
        {
            public Material WetAsphaltDark;
            public Material ColdConcrete;
            public Material RustMetal;
            public Material DirtyPlastic;
            public Material DeadVegetation;
            public Material WarmLightSource;
            public Material TechBlue;
            public Material DangerRed;
        }

        private sealed class PrototypeUiRefs
        {
            public InteractionPromptUI InteractionPromptUI;
            public NotificationUI NotificationUI;
            public ObjectivePanelUI ObjectivePanelUI;
            public DialoguePanelUI DialoguePanelUI;
            public PowerAllocationBoardUI PowerAllocationBoardUI;
            public ResultPanelUI ResultPanelUI;
            public CombatStatusUI CombatStatusUI;
            public GeneratorStatusUI GeneratorStatusUI;
            public PartyHUDUI PartyHUDUI;
            public AbilityBarUI AbilityBarUI;
            public AbilityTargetIndicatorUI AbilityTargetIndicatorUI;
            public MissionHintNotifier MissionHintNotifier;
            public FailurePanelUI FailurePanelUI;
            public PlaytestDebugPanelUI PlaytestDebugPanelUI;
        }

        private static MinimalMaterials EnsureMinimalMaterials()
        {
            return new MinimalMaterials
            {
                WetAsphaltDark = EnsureMaterialAsset("MAT_WetAsphalt_Dark", new Color(0.025f, 0.032f, 0.04f), false, 0f, 0.58f),
                ColdConcrete = EnsureMaterialAsset("MAT_ColdConcrete", new Color(0.31f, 0.34f, 0.35f), false, 0f, 0.22f),
                RustMetal = EnsureMaterialAsset("MAT_RustMetal", new Color(0.29f, 0.15f, 0.08f), false, 0.38f, 0.18f),
                DirtyPlastic = EnsureMaterialAsset("MAT_DirtyPlastic", new Color(0.5f, 0.49f, 0.43f), false, 0f, 0.34f),
                DeadVegetation = EnsureMaterialAsset("MAT_DeadVegetation", new Color(0.16f, 0.24f, 0.15f), false, 0f, 0.12f),
                WarmLightSource = EnsureMaterialAsset("MAT_WarmLightSource", new Color(1f, 0.58f, 0.18f), true, 0f, 0.42f, 1.8f),
                TechBlue = EnsureMaterialAsset("MAT_TechBlue", new Color(0.1f, 0.48f, 0.82f), true, 0f, 0.46f, 1.6f),
                DangerRed = EnsureMaterialAsset("MAT_DangerRed", new Color(0.55f, 0.04f, 0.03f), true, 0f, 0.36f, 1.25f)
            };
        }

        private static Material EnsureMaterialAsset(
            string materialName,
            Color color,
            bool emissive = false,
            float metallic = 0f,
            float smoothness = 0.25f,
            float emissionIntensity = 1f)
        {
            var path = $"{MaterialsPath}/{materialName}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(GetPrototypeShader());
                AssetDatabase.CreateAsset(material, path);
            }

            material.name = materialName;
            material.color = color;
            SetMaterialColor(material, color);
            SetMaterialFloat(material, "_Metallic", metallic);
            SetMaterialFloat(material, "_Smoothness", smoothness);
            SetMaterialFloat(material, "_Glossiness", smoothness);

            if (emissive)
            {
                material.EnableKeyword("_EMISSION");
                SetMaterialColor(material, color * emissionIntensity, "_EmissionColor");
            }
            else
            {
                material.DisableKeyword("_EMISSION");
                SetMaterialColor(material, Color.black, "_EmissionColor");
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static void SetMaterialColor(Material material, Color color, string propertyName = "_BaseColor")
        {
            if (material.HasProperty(propertyName))
            {
                material.SetColor(propertyName, color);
                return;
            }

            if (propertyName == "_BaseColor" && material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }
        }

        private static void SetMaterialFloat(Material material, string propertyName, float value)
        {
            if (material.HasProperty(propertyName))
            {
                material.SetFloat(propertyName, value);
            }
        }

        private static Shader GetPrototypeShader()
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            return shader != null ? shader : Shader.Find("Standard");
        }

        private static PrototypeUiRefs CreatePlaceholderUI(Transform parent, QuestManager questManager)
        {
            var promptObject = new GameObject("InteractionPromptUI");
            promptObject.transform.SetParent(parent);
            var interactionPrompt = promptObject.AddComponent<InteractionPromptUI>();

            var notificationObject = new GameObject("NotificationUI");
            notificationObject.transform.SetParent(parent);
            var notificationUI = notificationObject.AddComponent<NotificationUI>();

            var objectiveObject = new GameObject("ObjectivePanelUI");
            objectiveObject.transform.SetParent(parent);
            var objectivePanel = objectiveObject.AddComponent<ObjectivePanelUI>();
            objectivePanel.SetQuestManager(questManager);

            var dialogueObject = new GameObject("DialoguePanelUI");
            dialogueObject.transform.SetParent(parent);
            var dialoguePanel = dialogueObject.AddComponent<DialoguePanelUI>();

            var powerBoardObject = new GameObject("PowerAllocationBoardUI");
            powerBoardObject.transform.SetParent(parent);
            var powerBoard = powerBoardObject.AddComponent<PowerAllocationBoardUI>();

            var resultPanelObject = new GameObject("ResultPanelUI");
            resultPanelObject.transform.SetParent(parent);
            var resultPanel = resultPanelObject.AddComponent<ResultPanelUI>();

            var combatStatusObject = new GameObject("CombatStatusUI");
            combatStatusObject.transform.SetParent(parent);
            var combatStatus = combatStatusObject.AddComponent<CombatStatusUI>();

            var generatorStatusObject = new GameObject("GeneratorStatusUI");
            generatorStatusObject.transform.SetParent(parent);
            var generatorStatus = generatorStatusObject.AddComponent<GeneratorStatusUI>();

            var partyHudObject = new GameObject("PartyHUDUI");
            partyHudObject.transform.SetParent(parent);
            var partyHud = partyHudObject.AddComponent<PartyHUDUI>();

            var abilityBarObject = new GameObject("AbilityBarUI");
            abilityBarObject.transform.SetParent(parent);
            var abilityBar = abilityBarObject.AddComponent<AbilityBarUI>();

            var abilityTargetObject = new GameObject("AbilityTargetIndicatorUI");
            abilityTargetObject.transform.SetParent(parent);
            var abilityTargetIndicator = abilityTargetObject.AddComponent<AbilityTargetIndicatorUI>();

            var hintObject = new GameObject("MissionHintNotifier");
            hintObject.transform.SetParent(parent);
            var missionHintNotifier = hintObject.AddComponent<MissionHintNotifier>();

            var failureObject = new GameObject("FailurePanelUI");
            failureObject.transform.SetParent(parent);
            var failurePanel = failureObject.AddComponent<FailurePanelUI>();

            var debugObject = new GameObject("PlaytestDebugPanelUI");
            debugObject.transform.SetParent(parent);
            var debugPanel = debugObject.AddComponent<PlaytestDebugPanelUI>();

            return new PrototypeUiRefs
            {
                InteractionPromptUI = interactionPrompt,
                NotificationUI = notificationUI,
                ObjectivePanelUI = objectivePanel,
                DialoguePanelUI = dialoguePanel,
                PowerAllocationBoardUI = powerBoard,
                ResultPanelUI = resultPanel,
                CombatStatusUI = combatStatus,
                GeneratorStatusUI = generatorStatus,
                PartyHUDUI = partyHud,
                AbilityBarUI = abilityBar,
                AbilityTargetIndicatorUI = abilityTargetIndicator,
                MissionHintNotifier = missionHintNotifier,
                FailurePanelUI = failurePanel,
                PlaytestDebugPanelUI = debugPanel
            };
        }

        private static void CreateGround(Transform parent, MinimalMaterials materials)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground_NavMesh";
            ground.transform.SetParent(parent);
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(5f, 1f, 5f);

            var renderer = ground.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = materials.WetAsphaltDark;
            }
        }

        private static void CreateLevelBlockout(Transform parent, MinimalMaterials materials)
        {
            var blockoutRoot = new GameObject("_VisualBlockout");
            blockoutRoot.transform.SetParent(parent);

            CreateZoneBase(blockoutRoot.transform, "Arrival Street", new Vector3(0f, 0.03f, -14f), new Vector3(10f, 0.06f, 5f), materials.WetAsphaltDark);
            CreateZoneBase(blockoutRoot.transform, "Local Shelter", new Vector3(0f, 0.04f, -6f), new Vector3(14f, 0.08f, 5f), materials.ColdConcrete);
            CreateZoneBase(blockoutRoot.transform, "Filter Station", new Vector3(-10f, 0.04f, 8f), new Vector3(5f, 0.08f, 5f), materials.ColdConcrete);
            CreateZoneBase(blockoutRoot.transform, "Hydroponic Farm", new Vector3(-4f, 0.04f, 8f), new Vector3(5f, 0.08f, 5f), materials.DeadVegetation);
            CreateZoneBase(blockoutRoot.transform, "Defense Gate", new Vector3(2f, 0.04f, 9f), new Vector3(5f, 0.08f, 6f), materials.ColdConcrete);
            CreateZoneBase(blockoutRoot.transform, "Public Stage", new Vector3(8f, 0.04f, -2f), new Vector3(6f, 0.08f, 5f), materials.ColdConcrete);
            CreateZoneBase(blockoutRoot.transform, "Workshop", new Vector3(9f, 0.04f, 5f), new Vector3(6f, 0.08f, 5f), materials.RustMetal);
            CreateZoneBase(blockoutRoot.transform, "Relay Station", new Vector3(0f, 0.04f, 15f), new Vector3(6f, 0.08f, 5f), materials.ColdConcrete);
            CreateZoneBase(blockoutRoot.transform, "Generator Yard", new Vector3(0f, 0.04f, 3f), new Vector3(9f, 0.08f, 5f), materials.WetAsphaltDark);
            CreateZoneBase(blockoutRoot.transform, "Switch Room", new Vector3(0f, 0.04f, -10f), new Vector3(5f, 0.08f, 3f), materials.ColdConcrete);

            CreatePath(blockoutRoot.transform, "Main Route North", new Vector3(0f, 0.06f, -5f), new Vector3(3f, 0.04f, 18f), materials.WetAsphaltDark);
            CreatePath(blockoutRoot.transform, "Cross Route East", new Vector3(5f, 0.06f, 4f), new Vector3(11f, 0.04f, 2.5f), materials.WetAsphaltDark);
            CreatePath(blockoutRoot.transform, "Cross Route West", new Vector3(-5f, 0.06f, 8f), new Vector3(12f, 0.04f, 2.5f), materials.WetAsphaltDark);

            CreateRiversideInfrastructure(blockoutRoot.transform, materials);
            CreateColdRuins(blockoutRoot.transform, materials);
            CreateArrivalStreetProps(blockoutRoot.transform, materials);
            CreateLocalShelterProps(blockoutRoot.transform, materials);
            CreateFilterStationProps(blockoutRoot.transform, materials);
            CreateHydroponicFarmProps(blockoutRoot.transform, materials);
            CreateDefenseGateProps(blockoutRoot.transform, materials);
            CreatePublicStageProps(blockoutRoot.transform, materials);
            CreateWorkshopProps(blockoutRoot.transform, materials);
            CreateRelayStationProps(blockoutRoot.transform, materials);
            CreateGeneratorYardProps(blockoutRoot.transform, materials);
            CreateSwitchRoomProps(blockoutRoot.transform, materials);
            CreateRiversideWearAndWater(blockoutRoot.transform, materials);
        }

        private static void CreateZoneBase(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            CreateCube(parent, $"Zone_{name}", position, scale, material, false);
        }

        private static void CreatePath(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            CreateCube(parent, $"Path_{name}", position, scale, material, false);
        }

        private static void CreateRiversideInfrastructure(Transform parent, MinimalMaterials materials)
        {
            CreateCube(parent, "Arrival_Raised_Curb_West", new Vector3(-2.7f, 0.16f, -14f), new Vector3(0.18f, 0.18f, 5.2f), materials.ColdConcrete, false);
            CreateCube(parent, "Arrival_Raised_Curb_East", new Vector3(2.7f, 0.16f, -14f), new Vector3(0.18f, 0.18f, 5.2f), materials.ColdConcrete, false);
            CreateCube(parent, "MainRoute_Curb_West", new Vector3(-1.75f, 0.17f, -4.2f), new Vector3(0.16f, 0.18f, 18.6f), materials.ColdConcrete, false);
            CreateCube(parent, "MainRoute_Curb_East", new Vector3(1.75f, 0.17f, -4.2f), new Vector3(0.16f, 0.18f, 18.6f), materials.ColdConcrete, false);
            CreateCube(parent, "CrossRoute_East_NorthCurb", new Vector3(5f, 0.17f, 5.35f), new Vector3(10.8f, 0.18f, 0.16f), materials.ColdConcrete, false);
            CreateCube(parent, "CrossRoute_East_SouthCurb", new Vector3(5f, 0.17f, 2.65f), new Vector3(10.8f, 0.18f, 0.16f), materials.ColdConcrete, false);
            CreateCube(parent, "CrossRoute_West_NorthCurb", new Vector3(-5f, 0.17f, 9.35f), new Vector3(11.8f, 0.18f, 0.16f), materials.ColdConcrete, false);
            CreateCube(parent, "CrossRoute_West_SouthCurb", new Vector3(-5f, 0.17f, 6.65f), new Vector3(11.8f, 0.18f, 0.16f), materials.ColdConcrete, false);

            CreateCube(parent, "Shelter_Raised_Floor", new Vector3(0f, 0.16f, -6f), new Vector3(10f, 0.22f, 3.6f), materials.ColdConcrete, false);
            CreateCube(parent, "Shelter_Step_Front", new Vector3(0f, 0.18f, -3.7f), new Vector3(6.2f, 0.18f, 0.55f), materials.ColdConcrete, false);
            CreateCube(parent, "Generator_Service_Platform", new Vector3(0f, 0.18f, 3f), new Vector3(5.2f, 0.24f, 2.8f), materials.ColdConcrete, false);
            CreateCube(parent, "SwitchRoom_Raised_Threshold", new Vector3(0f, 0.18f, -10f), new Vector3(4.4f, 0.24f, 2.2f), materials.ColdConcrete, false);

            CreateCube(parent, "Alley_Broken_Wall_Northwest", new Vector3(-12.8f, 1.15f, 11f), new Vector3(4f, 2.3f, 0.35f), materials.ColdConcrete, false);
            CreateCube(parent, "Alley_Broken_Wall_West", new Vector3(-12.8f, 1.05f, 5.1f), new Vector3(0.35f, 2.1f, 4.2f), materials.ColdConcrete, false);
            CreateCube(parent, "Stage_Back_RetainingWall", new Vector3(8f, 1f, -4.8f), new Vector3(5.8f, 2f, 0.32f), materials.ColdConcrete, false);
            CreateCube(parent, "Workshop_Back_Wall", new Vector3(11.8f, 1.2f, 5f), new Vector3(0.35f, 2.4f, 4.8f), materials.ColdConcrete, false);
            CreateCube(parent, "Relay_Service_Wall", new Vector3(0f, 1f, 17.4f), new Vector3(5.6f, 2f, 0.3f), materials.ColdConcrete, false);
        }

        private static void CreateColdRuins(Transform parent, MinimalMaterials materials)
        {
            CreateCube(parent, "Shelter_BackWall", new Vector3(0f, 1.6f, -8.7f), new Vector3(14f, 3.2f, 0.45f), materials.ColdConcrete, false);
            CreateCube(parent, "Shelter_LeftWall", new Vector3(-7.2f, 1.3f, -5.5f), new Vector3(0.45f, 2.6f, 5f), materials.ColdConcrete, false);
            CreateCube(parent, "Broken_Block_West", new Vector3(-15f, 1.2f, 0f), new Vector3(1.2f, 2.4f, 12f), materials.ColdConcrete, false);
            CreateCube(parent, "Broken_Block_East", new Vector3(15f, 1.2f, 2f), new Vector3(1.2f, 2.4f, 12f), materials.ColdConcrete, false);
            CreateCube(parent, "WaterWarning_Sign", new Vector3(-2.8f, 1.3f, -13.2f), new Vector3(1.8f, 1.1f, 0.12f), materials.DangerRed, false);
            CreateCube(parent, "Collapsed_Slab_West_A", new Vector3(-6.3f, 0.28f, -1.5f), new Vector3(2.6f, 0.25f, 0.8f), materials.ColdConcrete, false);
            CreateCube(parent, "Collapsed_Slab_West_B", new Vector3(-7f, 0.45f, 0.4f), new Vector3(1.8f, 0.35f, 1.1f), materials.ColdConcrete, false);
            CreateCube(parent, "Cold_Service_Pipe", new Vector3(-1.4f, 1.05f, -8.45f), new Vector3(3.4f, 0.16f, 0.16f), materials.RustMetal, false);
        }

        private static void CreateArrivalStreetProps(Transform parent, MinimalMaterials materials)
        {
            CreateCube(parent, "Arrival_Road_LaneMark_A", new Vector3(0f, 0.095f, -15.6f), new Vector3(0.18f, 0.03f, 1.1f), materials.DirtyPlastic, false);
            CreateCube(parent, "Arrival_Road_LaneMark_B", new Vector3(0f, 0.095f, -13.6f), new Vector3(0.18f, 0.03f, 1.1f), materials.DirtyPlastic, false);
            CreateCube(parent, "Arrival_Road_LaneMark_C", new Vector3(0f, 0.095f, -11.6f), new Vector3(0.18f, 0.03f, 1.1f), materials.DirtyPlastic, false);
            CreateCube(parent, "Arrival_Barricade_Left_Base", new Vector3(-3.6f, 0.35f, -12.1f), new Vector3(1.6f, 0.7f, 0.35f), materials.ColdConcrete, false);
            CreateCube(parent, "Arrival_Barricade_Left_Warning", new Vector3(-3.6f, 0.85f, -12.1f), new Vector3(1.5f, 0.18f, 0.18f), materials.DangerRed, false);
            CreateCube(parent, "Arrival_Barricade_Right_Base", new Vector3(3.6f, 0.35f, -15.7f), new Vector3(1.6f, 0.7f, 0.35f), materials.ColdConcrete, false);
            CreateCube(parent, "Arrival_Barricade_Right_Warning", new Vector3(3.6f, 0.85f, -15.7f), new Vector3(1.5f, 0.18f, 0.18f), materials.DangerRed, false);
            CreateCylinder(parent, "Arrival_Old_Barrel_A", new Vector3(-4.3f, 0.45f, -15f), new Vector3(0.45f, 0.9f, 0.45f), materials.RustMetal, false);
            CreateCylinder(parent, "Arrival_Old_Barrel_B", new Vector3(4.1f, 0.45f, -12.2f), new Vector3(0.42f, 0.85f, 0.42f), materials.DirtyPlastic, false);
            CreateCube(parent, "Arrival_Tech_Cabinet", new Vector3(2.7f, 0.75f, -11.3f), new Vector3(0.7f, 1.5f, 0.5f), materials.ColdConcrete, false);
        }

        private static void CreateLocalShelterProps(Transform parent, MinimalMaterials materials)
        {
            CreateCube(parent, "Shelter_Cot_A", new Vector3(-4.4f, 0.34f, -6.6f), new Vector3(1.8f, 0.28f, 0.75f), materials.DirtyPlastic, false);
            CreateCube(parent, "Shelter_Cot_B", new Vector3(-4.4f, 0.34f, -5.2f), new Vector3(1.8f, 0.28f, 0.75f), materials.DirtyPlastic, false);
            CreateCube(parent, "Shelter_Supply_Crate_A", new Vector3(4.8f, 0.45f, -7.1f), new Vector3(0.9f, 0.9f, 0.9f), materials.DirtyPlastic, false);
            CreateCube(parent, "Shelter_Supply_Crate_B", new Vector3(5.7f, 0.35f, -7.1f), new Vector3(0.7f, 0.7f, 0.7f), materials.RustMetal, false);
            CreateCube(parent, "Shelter_Notice_Board", new Vector3(2.8f, 1.25f, -8.35f), new Vector3(1.7f, 1.1f, 0.12f), materials.DirtyPlastic, false);
            CreateCube(parent, "Shelter_Warm_Lantern", new Vector3(-1.2f, 1.55f, -7.9f), new Vector3(0.25f, 0.35f, 0.25f), materials.WarmLightSource, false);
            CreateCableRun(parent, "Shelter_Hanging_Cable", new Vector3(-0.2f, 2f, -8.25f), 3.4f, materials.RustMetal);
        }

        private static void CreateFilterStationProps(Transform parent, MinimalMaterials materials)
        {
            CreateCylinder(parent, "Filter_Tank_A", new Vector3(-11.2f, 1f, 8.6f), new Vector3(1.1f, 1.8f, 1.1f), materials.DirtyPlastic, false);
            CreateCylinder(parent, "Filter_Tank_B", new Vector3(-9.8f, 1f, 8.6f), new Vector3(1.1f, 1.8f, 1.1f), materials.DirtyPlastic, false);
            CreateCube(parent, "Filter_Terminal_Blue", new Vector3(-8.5f, 0.55f, 8.4f), new Vector3(0.8f, 1.1f, 0.45f), materials.TechBlue, false);
            CreateCube(parent, "Filter_Pipe_Long", new Vector3(-10.4f, 1.15f, 7f), new Vector3(3.3f, 0.18f, 0.18f), materials.RustMetal, false);
            CreateCylinder(parent, "Filter_Canister_A", new Vector3(-11.6f, 0.35f, 6.8f), new Vector3(0.35f, 0.7f, 0.35f), materials.DirtyPlastic, false);
            CreateCylinder(parent, "Filter_Canister_B", new Vector3(-8.7f, 0.35f, 6.8f), new Vector3(0.35f, 0.7f, 0.35f), materials.DirtyPlastic, false);
            CreateCube(parent, "Filter_Drain_Channel", new Vector3(-10.4f, 0.12f, 6.45f), new Vector3(3.8f, 0.06f, 0.35f), materials.WetAsphaltDark, false);
            CreateCube(parent, "Filter_DoNotDrink_Sign", new Vector3(-12.15f, 1.15f, 7.1f), new Vector3(0.12f, 0.85f, 1.6f), materials.DangerRed, false);
            CreateCylinder(parent, "Filter_Bucket_A", new Vector3(-12.1f, 0.28f, 9.6f), new Vector3(0.28f, 0.55f, 0.28f), materials.RustMetal, false);
            CreateCylinder(parent, "Filter_Bucket_B", new Vector3(-8.1f, 0.28f, 9.7f), new Vector3(0.28f, 0.55f, 0.28f), materials.DirtyPlastic, false);
            CreateCube(parent, "Filter_Service_Panel", new Vector3(-8.5f, 1.15f, 9.25f), new Vector3(1.1f, 0.75f, 0.16f), materials.ColdConcrete, false);
        }

        private static void CreateHydroponicFarmProps(Transform parent, MinimalMaterials materials)
        {
            for (var i = 0; i < 3; i++)
            {
                var z = 7f + i * 1.2f;
                CreateCube(parent, $"Farm_Rack_{i + 1}", new Vector3(-4.2f, 0.75f, z), new Vector3(3.6f, 0.16f, 0.9f), materials.RustMetal, false);
                CreateCube(parent, $"Farm_PlantPanel_{i + 1}", new Vector3(-4.2f, 0.92f, z), new Vector3(3.2f, 0.08f, 0.65f), materials.DeadVegetation, false);
                CreateCube(parent, $"Farm_GrowLight_{i + 1}", new Vector3(-4.2f, 1.35f, z), new Vector3(3.1f, 0.08f, 0.08f), materials.WarmLightSource, false);
            }

            CreateCylinder(parent, "Farm_NutrientTank", new Vector3(-1.7f, 0.55f, 8.2f), new Vector3(0.6f, 1.1f, 0.6f), materials.DirtyPlastic, false);
            CreateCube(parent, "Farm_DeadTray", new Vector3(-6.2f, 0.32f, 9.7f), new Vector3(1.5f, 0.16f, 0.8f), materials.RustMetal, false);
            CreateCube(parent, "Farm_Pump_Block", new Vector3(-2f, 0.42f, 6.5f), new Vector3(0.9f, 0.8f, 0.65f), materials.RustMetal, false);
            CreateCube(parent, "Farm_Irrigation_Line_A", new Vector3(-4.2f, 1.18f, 6.4f), new Vector3(3.8f, 0.08f, 0.1f), materials.TechBlue, false);
            CreateCube(parent, "Farm_Irrigation_Line_B", new Vector3(-4.2f, 1.18f, 10f), new Vector3(3.8f, 0.08f, 0.1f), materials.TechBlue, false);
            CreateCube(parent, "Farm_Empty_Tray_A", new Vector3(-6.15f, 0.25f, 7.7f), new Vector3(1.35f, 0.12f, 0.65f), materials.ColdConcrete, false);
            CreateCube(parent, "Farm_Greenhouse_Frame_Back", new Vector3(-4.2f, 1.55f, 10.55f), new Vector3(4.4f, 1.9f, 0.16f), materials.RustMetal, false);
        }

        private static void CreateDefenseGateProps(Transform parent, MinimalMaterials materials)
        {
            CreateCube(parent, "Defense_Gate_Left", new Vector3(0.7f, 1.4f, 11.6f), new Vector3(0.35f, 2.8f, 0.35f), materials.RustMetal, false);
            CreateCube(parent, "Defense_Gate_Right", new Vector3(3.3f, 1.4f, 11.6f), new Vector3(0.35f, 2.8f, 0.35f), materials.RustMetal, false);
            CreateCube(parent, "Defense_Gate_Bar", new Vector3(2f, 2.5f, 11.6f), new Vector3(3.1f, 0.25f, 0.3f), materials.RustMetal, false);
            CreateCube(parent, "Defense_DangerStripe", new Vector3(2f, 1.1f, 11.25f), new Vector3(3.1f, 0.15f, 0.15f), materials.DangerRed, false);
            CreateFence(parent, materials, -0.6f);
            CreateFence(parent, materials, 4.6f);
            CreateCube(parent, "Defense_Barricade_A", new Vector3(0.3f, 0.45f, 8.4f), new Vector3(1.4f, 0.9f, 0.55f), materials.ColdConcrete, false);
            CreateCube(parent, "Defense_Barricade_B", new Vector3(3.7f, 0.45f, 8.4f), new Vector3(1.4f, 0.9f, 0.55f), materials.ColdConcrete, false);
            CreateCube(parent, "Defense_Sandbag_Line_A", new Vector3(1.2f, 0.32f, 7.35f), new Vector3(1.6f, 0.42f, 0.4f), materials.DirtyPlastic, false);
            CreateCube(parent, "Defense_Sandbag_Line_B", new Vector3(2.8f, 0.32f, 7.35f), new Vector3(1.6f, 0.42f, 0.4f), materials.DirtyPlastic, false);
            CreateCube(parent, "Defense_Warning_FloorStripe_A", new Vector3(1.05f, 0.1f, 10.65f), new Vector3(0.9f, 0.03f, 0.18f), materials.DangerRed, false);
            CreateCube(parent, "Defense_Warning_FloorStripe_B", new Vector3(2f, 0.1f, 10.65f), new Vector3(0.9f, 0.03f, 0.18f), materials.WarmLightSource, false);
            CreateCube(parent, "Defense_Warning_FloorStripe_C", new Vector3(2.95f, 0.1f, 10.65f), new Vector3(0.9f, 0.03f, 0.18f), materials.DangerRed, false);
            CreateCube(parent, "Defense_Searchlight_Left_Housing", new Vector3(0.6f, 2.55f, 10.9f), new Vector3(0.55f, 0.42f, 0.65f), materials.WarmLightSource, false);
            CreateCube(parent, "Defense_Searchlight_Right_Housing", new Vector3(3.4f, 2.55f, 10.9f), new Vector3(0.55f, 0.42f, 0.65f), materials.WarmLightSource, false);
        }

        private static void CreateFence(Transform parent, MinimalMaterials materials, float x)
        {
            for (var i = 0; i < 4; i++)
            {
                CreateCube(parent, $"Defense_Fence_{x}_{i}", new Vector3(x, 0.65f, 7.2f + i * 1.2f), new Vector3(0.1f, 1.3f, 0.1f), materials.RustMetal, false);
            }

            CreateCube(parent, $"Defense_Fence_TopRail_{x}", new Vector3(x, 1.25f, 9f), new Vector3(0.08f, 0.08f, 4.2f), materials.RustMetal, false);
            CreateCube(parent, $"Defense_Fence_LowRail_{x}", new Vector3(x, 0.65f, 9f), new Vector3(0.08f, 0.08f, 4.2f), materials.RustMetal, false);
        }

        private static void CreatePublicStageProps(Transform parent, MinimalMaterials materials)
        {
            CreateCube(parent, "Stage_Platform", new Vector3(8f, 0.35f, -2.2f), new Vector3(4.4f, 0.7f, 2.4f), materials.RustMetal, false);
            CreateCylinder(parent, "Stage_MicrophoneStand", new Vector3(8f, 1.25f, -1.5f), new Vector3(0.08f, 1.6f, 0.08f), materials.RustMetal, false);
            CreateSphere(parent, "Stage_Microphone", new Vector3(8f, 2.1f, -1.5f), new Vector3(0.18f, 0.18f, 0.18f), materials.DirtyPlastic, false);
            CreateCube(parent, "Stage_Spotlight_Warm", new Vector3(6f, 1.6f, -4.1f), new Vector3(0.5f, 0.5f, 0.7f), materials.WarmLightSource, false);
            CreateCube(parent, "Stage_Bench_A", new Vector3(6.8f, 0.35f, 0.7f), new Vector3(2.2f, 0.25f, 0.45f), materials.ColdConcrete, false);
            CreateCube(parent, "Stage_Bench_B", new Vector3(9.2f, 0.35f, 0.7f), new Vector3(2.2f, 0.25f, 0.45f), materials.ColdConcrete, false);
            CreateCube(parent, "Stage_Bench_A_Legs", new Vector3(6.8f, 0.18f, 0.7f), new Vector3(2f, 0.25f, 0.12f), materials.RustMetal, false);
            CreateCube(parent, "Stage_Bench_B_Legs", new Vector3(9.2f, 0.18f, 0.7f), new Vector3(2f, 0.25f, 0.12f), materials.RustMetal, false);
            CreateCube(parent, "Stage_Notice_Board", new Vector3(10.5f, 1.2f, -3.3f), new Vector3(0.16f, 1.2f, 1.8f), materials.DirtyPlastic, false);
            CreateCube(parent, "Stage_Broken_Step", new Vector3(8f, 0.16f, -0.75f), new Vector3(3.2f, 0.22f, 0.42f), materials.ColdConcrete, false);
            CreateCube(parent, "Stage_Warm_Cable", new Vector3(7.1f, 0.09f, -3.55f), new Vector3(2.2f, 0.07f, 0.12f), materials.RustMetal, false);
        }

        private static void CreateWorkshopProps(Transform parent, MinimalMaterials materials)
        {
            CreateCube(parent, "Workshop_Bench", new Vector3(9f, 0.75f, 5f), new Vector3(3.5f, 0.35f, 1.2f), materials.RustMetal, false);
            CreateCube(parent, "Workshop_ToolWall", new Vector3(11.2f, 1.4f, 5f), new Vector3(0.25f, 2.2f, 2.8f), materials.ColdConcrete, false);
            CreateCube(parent, "Workshop_Crate_A", new Vector3(7f, 0.4f, 6.4f), new Vector3(0.8f, 0.8f, 0.8f), materials.DirtyPlastic, false);
            CreateCube(parent, "Workshop_Crate_B", new Vector3(7.9f, 0.4f, 6.4f), new Vector3(0.8f, 0.8f, 0.8f), materials.RustMetal, false);
            CreateCube(parent, "Workshop_ChargeRack", new Vector3(10.6f, 0.9f, 3.4f), new Vector3(0.7f, 1.8f, 0.5f), materials.WarmLightSource, false);
            CreateCube(parent, "Workshop_CableRun", new Vector3(9f, 0.08f, 3.5f), new Vector3(3.5f, 0.08f, 0.18f), materials.TechBlue, false);
            CreateCube(parent, "Workshop_Tool_A", new Vector3(11.02f, 1.55f, 4.2f), new Vector3(0.08f, 0.9f, 0.12f), materials.RustMetal, false);
            CreateCube(parent, "Workshop_Tool_B", new Vector3(11.02f, 1.25f, 5.1f), new Vector3(0.08f, 0.65f, 0.12f), materials.WarmLightSource, false);
            CreateCube(parent, "Workshop_TechCabinet_A", new Vector3(10.7f, 0.9f, 6.7f), new Vector3(0.8f, 1.8f, 0.65f), materials.ColdConcrete, false);
            CreateCube(parent, "Workshop_Spare_Pipe_A", new Vector3(8.9f, 0.25f, 6.75f), new Vector3(2.7f, 0.12f, 0.12f), materials.RustMetal, false);
            CreateCylinder(parent, "Workshop_Oil_Drum", new Vector3(6.55f, 0.45f, 4.15f), new Vector3(0.45f, 0.9f, 0.45f), materials.RustMetal, false);
            CreateCableRun(parent, "Workshop_CableBundle", new Vector3(8.3f, 0.12f, 3.9f), 2.1f, materials.TechBlue);
        }

        private static void CreateRelayStationProps(Transform parent, MinimalMaterials materials)
        {
            CreateCube(parent, "Relay_Terminal_Blue", new Vector3(-1.2f, 0.65f, 14.4f), new Vector3(1.2f, 1.3f, 0.65f), materials.TechBlue, false);
            CreateCylinder(parent, "Relay_Antenna_Mast", new Vector3(1.2f, 2.2f, 15.1f), new Vector3(0.12f, 4.4f, 0.12f), materials.RustMetal, false);
            CreateCube(parent, "Relay_Antenna_Crossbar", new Vector3(1.2f, 3.9f, 15.1f), new Vector3(2f, 0.08f, 0.08f), materials.RustMetal, false);
            CreateCube(parent, "Relay_BlueBeacon", new Vector3(1.2f, 4.4f, 15.1f), new Vector3(0.32f, 0.32f, 0.32f), materials.TechBlue, false);
            CreateCube(parent, "Relay_CableCabinet", new Vector3(2.4f, 0.75f, 14.2f), new Vector3(0.8f, 1.5f, 0.7f), materials.ColdConcrete, false);
            CreateCube(parent, "Relay_GuyWire_Left", new Vector3(0.3f, 2.1f, 14.2f), new Vector3(0.08f, 3.2f, 0.08f), materials.RustMetal, false);
            CreateCube(parent, "Relay_GuyWire_Right", new Vector3(2.1f, 2.1f, 14.2f), new Vector3(0.08f, 3.2f, 0.08f), materials.RustMetal, false);
            CreateCube(parent, "Relay_Radio_Block", new Vector3(-1.95f, 0.35f, 15.4f), new Vector3(0.7f, 0.7f, 0.5f), materials.DirtyPlastic, false);
            CreateCube(parent, "Relay_Network_Sign", new Vector3(-2.55f, 1.15f, 14.1f), new Vector3(0.12f, 0.9f, 1.4f), materials.TechBlue, false);
            CreateCableRun(parent, "Relay_Ground_Cable", new Vector3(0.2f, 0.09f, 14.15f), 2.8f, materials.TechBlue);
        }

        private static void CreateGeneratorYardProps(Transform parent, MinimalMaterials materials)
        {
            CreateCube(parent, "Generator_G17_Block", new Vector3(0f, 1f, 3f), new Vector3(3.2f, 2f, 1.6f), materials.RustMetal, false);
            CreateCylinder(parent, "Generator_G17_Flywheel_A", new Vector3(-1.9f, 1f, 3f), new Vector3(0.8f, 0.32f, 0.8f), materials.ColdConcrete, false);
            CreateCylinder(parent, "Generator_G17_Flywheel_B", new Vector3(1.9f, 1f, 3f), new Vector3(0.8f, 0.32f, 0.8f), materials.ColdConcrete, false);
            CreateCube(parent, "Generator_WarningLight", new Vector3(0f, 2.2f, 2.15f), new Vector3(0.45f, 0.28f, 0.2f), materials.DangerRed, false);
            CreateCube(parent, "Generator_Cable_Left", new Vector3(-2.4f, 0.09f, 3.6f), new Vector3(2.2f, 0.08f, 0.2f), materials.TechBlue, false);
            CreateCube(parent, "Generator_Cable_Right", new Vector3(2.4f, 0.09f, 3.6f), new Vector3(2.2f, 0.08f, 0.2f), materials.TechBlue, false);
            CreateCube(parent, "Generator_Cover_A", new Vector3(-3.8f, 0.55f, 1.3f), new Vector3(1.4f, 1.1f, 0.7f), materials.ColdConcrete, false);
            CreateCube(parent, "Generator_Cover_B", new Vector3(3.8f, 0.55f, 1.3f), new Vector3(1.4f, 1.1f, 0.7f), materials.ColdConcrete, false);
            CreateCube(parent, "Generator_BatteryBlocks", new Vector3(0f, 0.5f, 5f), new Vector3(2.4f, 1f, 0.7f), materials.DirtyPlastic, false);
            CreateCube(parent, "Generator_Service_Panel_Left", new Vector3(-1.15f, 1.15f, 2.12f), new Vector3(0.55f, 0.75f, 0.16f), materials.TechBlue, false);
            CreateCube(parent, "Generator_Service_Panel_Right", new Vector3(1.15f, 1.15f, 2.12f), new Vector3(0.55f, 0.75f, 0.16f), materials.DirtyPlastic, false);
            CreateCube(parent, "Generator_Cable_Block_A", new Vector3(-2.75f, 0.25f, 4.6f), new Vector3(0.7f, 0.45f, 0.55f), materials.RustMetal, false);
            CreateCube(parent, "Generator_Cable_Block_B", new Vector3(2.75f, 0.25f, 4.6f), new Vector3(0.7f, 0.45f, 0.55f), materials.RustMetal, false);
            CreateCube(parent, "Generator_Warning_Stripe_A", new Vector3(-1f, 0.32f, 1.5f), new Vector3(0.9f, 0.05f, 0.18f), materials.DangerRed, false);
            CreateCube(parent, "Generator_Warning_Stripe_B", new Vector3(0f, 0.32f, 1.5f), new Vector3(0.9f, 0.05f, 0.18f), materials.WarmLightSource, false);
            CreateCube(parent, "Generator_Warning_Stripe_C", new Vector3(1f, 0.32f, 1.5f), new Vector3(0.9f, 0.05f, 0.18f), materials.DangerRed, false);
        }

        private static void CreateSwitchRoomProps(Transform parent, MinimalMaterials materials)
        {
            CreateCube(parent, "SwitchRoom_BackWall", new Vector3(0f, 1.35f, -11.55f), new Vector3(5.6f, 2.7f, 0.35f), materials.ColdConcrete, false);
            CreateCube(parent, "SwitchRoom_LeftWall", new Vector3(-2.95f, 1.15f, -10f), new Vector3(0.32f, 2.3f, 2.8f), materials.ColdConcrete, false);
            CreateCube(parent, "SwitchRoom_RightWall", new Vector3(2.95f, 1.15f, -10f), new Vector3(0.32f, 2.3f, 2.8f), materials.ColdConcrete, false);
            CreateCube(parent, "SwitchRoom_BreakerRack_Left", new Vector3(-1.8f, 0.95f, -11.15f), new Vector3(0.85f, 1.55f, 0.35f), materials.RustMetal, false);
            CreateCube(parent, "SwitchRoom_BreakerRack_Right", new Vector3(1.8f, 0.95f, -11.15f), new Vector3(0.85f, 1.55f, 0.35f), materials.RustMetal, false);
            CreateCube(parent, "SwitchRoom_Blue_Status_Lamp", new Vector3(0f, 1.8f, -11.22f), new Vector3(0.35f, 0.18f, 0.12f), materials.TechBlue, false);
            CreateCube(parent, "SwitchRoom_Cable_Feed", new Vector3(0f, 0.09f, -8.85f), new Vector3(2.8f, 0.08f, 0.22f), materials.RustMetal, false);
        }

        private static void CreateRiversideWearAndWater(Transform parent, MinimalMaterials materials)
        {
            CreateCube(parent, "Puddle_Arrival_Left", new Vector3(-1.15f, 0.085f, -15.2f), new Vector3(1.1f, 0.025f, 0.65f), materials.TechBlue, false);
            CreateCube(parent, "Puddle_Shelter_Door", new Vector3(1.4f, 0.095f, -3.5f), new Vector3(1.35f, 0.025f, 0.55f), materials.TechBlue, false);
            CreateCube(parent, "Puddle_Filter_Runoff", new Vector3(-9.6f, 0.095f, 6.15f), new Vector3(1.45f, 0.025f, 0.5f), materials.TechBlue, false);
            CreateCube(parent, "Oil_Stain_Workshop", new Vector3(8.2f, 0.095f, 4f), new Vector3(1.25f, 0.025f, 0.55f), materials.WetAsphaltDark, false);
            CreateCube(parent, "Dirt_Patch_Farm_Edge", new Vector3(-6.1f, 0.1f, 6.35f), new Vector3(1.5f, 0.035f, 0.75f), materials.DeadVegetation, false);
            CreateCube(parent, "Dirt_Patch_Defense_Edge", new Vector3(4.15f, 0.1f, 6.8f), new Vector3(1.2f, 0.035f, 0.75f), materials.DirtyPlastic, false);
            CreateCube(parent, "Broken_Road_Slab_A", new Vector3(-0.9f, 0.12f, -8.4f), new Vector3(1.3f, 0.06f, 0.55f), materials.ColdConcrete, false);
            CreateCube(parent, "Broken_Road_Slab_B", new Vector3(1.25f, 0.12f, 1.1f), new Vector3(1.6f, 0.06f, 0.5f), materials.ColdConcrete, false);
            CreateCube(parent, "Warning_Strip_Generator_Yard", new Vector3(0f, 0.11f, 0.55f), new Vector3(3.5f, 0.035f, 0.18f), materials.DangerRed, false);
            CreateCube(parent, "Warning_Strip_Switch_Threshold", new Vector3(0f, 0.12f, -8.55f), new Vector3(3.6f, 0.035f, 0.18f), materials.WarmLightSource, false);
        }

        private static void CreateCableRun(Transform parent, string name, Vector3 position, float length, Material material)
        {
            CreateCube(parent, $"{name}_A", position, new Vector3(length, 0.07f, 0.08f), material, false);
            CreateCube(parent, $"{name}_B", position + new Vector3(0.15f, 0.02f, 0.18f), new Vector3(length * 0.88f, 0.06f, 0.08f), material, false);
            CreateCube(parent, $"{name}_Clamp_A", position + new Vector3(-length * 0.35f, 0.05f, 0.09f), new Vector3(0.16f, 0.12f, 0.34f), material, false);
            CreateCube(parent, $"{name}_Clamp_B", position + new Vector3(length * 0.35f, 0.05f, 0.09f), new Vector3(0.16f, 0.12f, 0.34f), material, false);
        }

        private static GameObject CreateCube(Transform parent, string name, Vector3 position, Vector3 scale, Material material, bool keepCollider)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent);
            cube.transform.position = position;
            cube.transform.localScale = scale;
            AssignMaterial(cube, material);
            RemoveColliderIfNeeded(cube, keepCollider);
            return cube;
        }

        private static GameObject CreateCylinder(Transform parent, string name, Vector3 position, Vector3 scale, Material material, bool keepCollider)
        {
            var cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.name = name;
            cylinder.transform.SetParent(parent);
            cylinder.transform.position = position;
            cylinder.transform.localScale = scale;
            AssignMaterial(cylinder, material);
            RemoveColliderIfNeeded(cylinder, keepCollider);
            return cylinder;
        }

        private static GameObject CreateSphere(Transform parent, string name, Vector3 position, Vector3 scale, Material material, bool keepCollider)
        {
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = name;
            sphere.transform.SetParent(parent);
            sphere.transform.position = position;
            sphere.transform.localScale = scale;
            AssignMaterial(sphere, material);
            RemoveColliderIfNeeded(sphere, keepCollider);
            return sphere;
        }

        private static GameObject CreateChildCube(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material)
        {
            return CreateChildPrimitive(parent, PrimitiveType.Cube, name, localPosition, localScale, material);
        }

        private static GameObject CreateChildSphere(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material)
        {
            return CreateChildPrimitive(parent, PrimitiveType.Sphere, name, localPosition, localScale, material);
        }

        private static GameObject CreateChildPrimitive(
            Transform parent,
            PrimitiveType primitiveType,
            string name,
            Vector3 localPosition,
            Vector3 localScale,
            Material material)
        {
            var primitive = GameObject.CreatePrimitive(primitiveType);
            primitive.name = name;
            primitive.transform.SetParent(parent, false);
            primitive.transform.localPosition = localPosition;
            primitive.transform.localRotation = Quaternion.identity;
            primitive.transform.localScale = localScale;
            AssignMaterial(primitive, material);
            RemoveColliderIfNeeded(primitive, false);
            return primitive;
        }

        private static void AssignMaterial(GameObject gameObject, Material material)
        {
            var renderer = gameObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }
        }

        private static void RemoveColliderIfNeeded(GameObject gameObject, bool keepCollider)
        {
            if (keepCollider)
            {
                return;
            }

            var collider = gameObject.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }
        }

        private static void CreateSceneLighting(MinimalMaterials materials)
        {
            RenderSettings.ambientLight = new Color(0.035f, 0.045f, 0.06f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.045f, 0.06f, 0.07f);
            RenderSettings.fogDensity = 0.012f;

            var sunObject = new GameObject("Cold Evening Directional Light");
            var sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(0.45f, 0.56f, 0.68f);
            sun.intensity = 0.28f;
            sunObject.transform.rotation = Quaternion.Euler(55f, -30f, 0f);

            CreatePointLight("Arrival_Street_Cold_Glint", new Vector3(0f, 2.5f, -13.4f), new Color(0.32f, 0.55f, 0.72f), 0.9f, 7f);
            CreatePointLight("Shelter_Warm_Light", new Vector3(0f, 3.4f, -6.2f), new Color(1f, 0.58f, 0.28f), 3.1f, 8f);
            CreatePointLight("SwitchRoom_Blue_Lock_Light", new Vector3(0f, 2.1f, -10.7f), new Color(0.18f, 0.55f, 1f), 1.5f, 5f);
            CreatePointLight("Filter_Cold_Light", new Vector3(-10f, 2.8f, 8f), new Color(0.42f, 0.78f, 1f), 2.7f, 7f);
            CreatePointLight("Farm_Grow_Light", new Vector3(-4f, 2.6f, 8f), new Color(0.95f, 0.72f, 0.3f), 2.9f, 6.5f);
            CreatePointLight("Workshop_Rust_Light", new Vector3(9f, 2.8f, 5f), new Color(1f, 0.42f, 0.18f), 2.9f, 7f);
            CreatePointLight("Relay_Blue_Light", new Vector3(0f, 3.2f, 15f), new Color(0.18f, 0.62f, 1f), 3.1f, 8f);
            CreatePointLight("Generator_Warning_Light", new Vector3(0f, 3.2f, 3f), new Color(1f, 0.12f, 0.08f), 2.2f, 7f);

            CreateSpotLight("Defense_Left_Searchlight", new Vector3(0.5f, 3.4f, 10.5f), Quaternion.Euler(68f, 165f, 0f), new Color(1f, 0.84f, 0.58f), 3f, 12f);
            CreateSpotLight("Defense_Right_Searchlight", new Vector3(3.5f, 3.4f, 10.5f), Quaternion.Euler(68f, 195f, 0f), new Color(1f, 0.84f, 0.58f), 3f, 12f);
            CreateSpotLight("Stage_Old_Spotlight", new Vector3(6.2f, 2.7f, -4f), Quaternion.Euler(62f, 30f, 0f), new Color(1f, 0.64f, 0.28f), 2.5f, 10f);
            CreateSpotLight("Generator_Service_Lamp", new Vector3(-2.6f, 3.2f, 1.4f), Quaternion.Euler(62f, -25f, 0f), new Color(1f, 0.48f, 0.22f), 1.8f, 8f);

            _ = materials;
        }

        private static PowerResultVisualController CreatePowerResultVisuals(Transform parent, MinimalMaterials materials)
        {
            var controllerObject = new GameObject("PowerResultVisualController");
            controllerObject.transform.SetParent(parent);
            var controller = controllerObject.AddComponent<PowerResultVisualController>();

            var statesRoot = new GameObject("_PowerResultStates");
            statesRoot.transform.SetParent(parent);

            var states = new List<PowerResultVisualState>
            {
                CreatePowerResultState(statesRoot.transform, PowerChoice.WaterFilters, "Result_WaterFilters", new Vector3(-10f, 2.2f, 8f), materials.TechBlue, new Color(0.45f, 0.85f, 1f), 2.8f, 7f, false, true),
                CreatePowerResultState(statesRoot.transform, PowerChoice.HydroponicFarm, "Result_HydroponicFarm", new Vector3(-4f, 2.2f, 8f), materials.WarmLightSource, new Color(1f, 0.74f, 0.32f), 2.6f, 7f, false, true),
                CreatePowerResultState(statesRoot.transform, PowerChoice.DefenseGrid, "Result_DefenseGrid", new Vector3(2f, 2.7f, 9.8f), materials.DangerRed, new Color(1f, 0.2f, 0.1f), 2.7f, 8f, true, true),
                CreatePowerResultState(statesRoot.transform, PowerChoice.PublicStage, "Result_PublicStage", new Vector3(8f, 2.4f, -2.2f), materials.WarmLightSource, new Color(1f, 0.62f, 0.28f), 2.6f, 7f, false, true),
                CreatePowerResultState(statesRoot.transform, PowerChoice.Workshop, "Result_Workshop", new Vector3(9f, 2.2f, 5f), materials.WarmLightSource, new Color(1f, 0.44f, 0.18f), 2.5f, 7f, false, true),
                CreatePowerResultState(statesRoot.transform, PowerChoice.RelayStation, "Result_RelayStation", new Vector3(0f, 3.6f, 15f), materials.TechBlue, new Color(0.2f, 0.62f, 1f), 3f, 8f, true, true),
                CreatePowerResultState(statesRoot.transform, PowerChoice.PartyReserve, "Result_PartyReserve", new Vector3(2.8f, 1.2f, -6f), materials.TechBlue, new Color(0.25f, 0.7f, 1f), 2f, 5f, true, false)
            };

            controller.SetStates(states);
            return controller;
        }

        private static PowerResultVisualState CreatePowerResultState(
            Transform parent,
            PowerChoice choice,
            string name,
            Vector3 position,
            Material markerMaterial,
            Color lightColor,
            float intensity,
            float range,
            bool blink,
            bool enableForSplitLoad)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent);
            root.transform.position = position;

            var markerObject = CreateCube(root.transform, $"{name}_Marker", position, new Vector3(0.55f, 0.18f, 0.55f), markerMaterial, false);
            var markerRenderer = markerObject != null ? markerObject.GetComponent<Renderer>() : null;

            var lightObject = new GameObject($"{name}_Light");
            lightObject.transform.SetParent(root.transform);
            lightObject.transform.position = position;
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = lightColor;
            light.intensity = intensity;
            light.range = range;

            return new PowerResultVisualState(
                choice,
                root,
                light != null ? new[] { light } : Array.Empty<Light>(),
                markerRenderer != null ? new[] { markerRenderer } : Array.Empty<Renderer>(),
                markerMaterial != null ? new[] { markerMaterial } : Array.Empty<Material>(),
                blink,
                enableForSplitLoad);
        }

        private static void CreatePointLight(string name, Vector3 position, Color color, float intensity, float range)
        {
            var lightObject = new GameObject(name);
            lightObject.transform.position = position;
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
        }

        private static void CreateSpotLight(string name, Vector3 position, Quaternion rotation, Color color, float intensity, float range)
        {
            var lightObject = new GameObject(name);
            lightObject.transform.position = position;
            lightObject.transform.rotation = rotation;
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Spot;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.spotAngle = 42f;
        }

        private static IsometricCameraController CreateMainCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(-8.8f, 16.5f, -8.8f);
            cameraObject.transform.rotation = Quaternion.Euler(63f, 45f, 0f);

            var camera = cameraObject.AddComponent<UnityEngine.Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 15f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 200f;

            cameraObject.AddComponent<AudioListener>();
            var cameraController = cameraObject.AddComponent<IsometricCameraController>();
            cameraController.ConfigureView(63f, 45f, 15f, 10f, 22f, 0f, 18.5f);
            return cameraController;
        }

        private static List<PlayerCharacter> CreatePlaceholderParty(Transform parent, MinimalMaterials materials, Dictionary<string, AbilityData> abilityAssets)
        {
            return new List<PlayerCharacter>
            {
                CreatePlaceholderCharacter(
                    parent,
                    EnsureCharacterDataAsset(
                        "SO_Character_Dax",
                        "dax",
                        "Dax",
                        "Практик",
                        120,
                        60,
                        16,
                        6.5f,
                        1f,
                        GetAbilities(abilityAssets, "dax_repair_burst", "dax_suppressive_shot")),
                    new Vector3(-2f, 0f, 0f),
                    materials.WarmLightSource,
                    materials),
                CreatePlaceholderCharacter(
                    parent,
                    EnsureCharacterDataAsset(
                        "SO_Character_Nyra",
                        "nyra",
                        "Nyra",
                        "Техник",
                        95,
                        85,
                        13,
                        7f,
                        1f,
                        GetAbilities(abilityAssets, "nyra_shock_pulse", "nyra_overload_panel")),
                    new Vector3(0f, 0f, 0f),
                    materials.TechBlue,
                    materials),
                CreatePlaceholderCharacter(
                    parent,
                    EnsureCharacterDataAsset(
                        "SO_Character_Cormac",
                        "cormac",
                        "Cormac",
                        "Медик",
                        110,
                        75,
                        12,
                        6f,
                        1.15f,
                        GetAbilities(abilityAssets, "cormac_emergency_heal", "cormac_field_stabilize")),
                    new Vector3(2f, 0f, 0f),
                    materials.DeadVegetation,
                    materials)
            };
        }

        private static PlayerCharacter CreatePlaceholderCharacter(
            Transform parent,
            CharacterData characterData,
            Vector3 position,
            Material accentMaterial,
            MinimalMaterials materials)
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
                visualRenderer.sharedMaterial = accentMaterial;
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
                ringRenderer.sharedMaterial = materials.WarmLightSource;
            }

            CreateCharacterSilhouetteProps(characterRoot.transform, characterData, materials);
            playerCharacter.SetSelectionIndicator(selectionRing);
            return playerCharacter;
        }

        private static void CreateCharacterSilhouetteProps(Transform parent, CharacterData characterData, MinimalMaterials materials)
        {
            switch (characterData.Id)
            {
                case "dax":
                    CreateChildCube(parent, "Tool_Backpack", new Vector3(0f, 1.2f, -0.48f), new Vector3(0.58f, 0.72f, 0.28f), materials.RustMetal);
                    CreateChildCube(parent, "Shoulder_Tool", new Vector3(0.43f, 1.35f, 0f), new Vector3(0.18f, 0.9f, 0.18f), materials.WarmLightSource);
                    CreateChildCube(parent, "Belt_Pouch", new Vector3(-0.38f, 0.75f, 0.1f), new Vector3(0.22f, 0.3f, 0.2f), materials.DirtyPlastic);
                    break;
                case "nyra":
                    CreateChildCube(parent, "Chest_Scanner", new Vector3(0f, 1.22f, 0.48f), new Vector3(0.38f, 0.28f, 0.12f), materials.TechBlue);
                    CreateChildCube(parent, "Scanner_Antenna", new Vector3(0.32f, 1.8f, -0.1f), new Vector3(0.08f, 0.75f, 0.08f), materials.TechBlue);
                    CreateChildSphere(parent, "Scanner_Node", new Vector3(0.32f, 2.22f, -0.1f), new Vector3(0.18f, 0.18f, 0.18f), materials.TechBlue);
                    break;
                case "cormac":
                    CreateChildCube(parent, "Medic_Pack", new Vector3(0f, 1.15f, -0.47f), new Vector3(0.55f, 0.62f, 0.25f), materials.DirtyPlastic);
                    CreateChildCube(parent, "Medic_Cross_Vertical", new Vector3(0f, 1.17f, -0.61f), new Vector3(0.12f, 0.42f, 0.04f), materials.DeadVegetation);
                    CreateChildCube(parent, "Medic_Cross_Horizontal", new Vector3(0f, 1.17f, -0.62f), new Vector3(0.38f, 0.12f, 0.04f), materials.DeadVegetation);
                    CreateChildCube(parent, "Side_Medkit", new Vector3(-0.44f, 0.8f, 0f), new Vector3(0.22f, 0.34f, 0.24f), materials.DirtyPlastic);
                    break;
            }
        }

        private static GeneratorG17 CreateQuestInteractables(
            Transform parent,
            MinimalMaterials materials,
            GameManager gameManager,
            QuestManager questManager,
            PowerManager powerManager,
            CombatManager combatManager,
            NotificationUI notificationUI)
        {
            CreateBreakerModule(parent, "BreakerModule_A", "Breaker Module A", new Vector3(-5f, 0.25f, 4f), materials, questManager);
            CreateBreakerModule(parent, "BreakerModule_B", "Breaker Module B", new Vector3(5f, 0.25f, 4f), materials, questManager);

            CreateInspectableSystem(
                parent,
                "Filter Station",
                "Filter Station",
                PowerSystemType.WaterFilters,
                "Фильтры воды осмотрены.",
                new Vector3(-8f, 0.5f, 8f),
                materials.TechBlue,
                questManager);
            CreateInspectableSystem(
                parent,
                "Hydroponic Farm",
                "Hydroponic Farm",
                PowerSystemType.HydroponicFarm,
                "Гидропонная ферма осмотрена.",
                new Vector3(-4f, 0.5f, 8f),
                materials.DeadVegetation,
                questManager);
            CreateInspectableSystem(
                parent,
                "Defense Gate",
                "Defense Gate",
                PowerSystemType.DefenseGate,
                "Оборонительные ворота осмотрены.",
                new Vector3(0f, 0.5f, 8f),
                materials.DangerRed,
                questManager);
            CreateInspectableSystem(
                parent,
                "Public Stage",
                "Public Stage",
                PowerSystemType.PublicStage,
                "Общественная сцена осмотрена.",
                new Vector3(4f, 0.5f, 8f),
                materials.WarmLightSource,
                questManager);
            CreateInspectableSystem(
                parent,
                "Workshop",
                "Workshop",
                PowerSystemType.Workshop,
                "Мастерская осмотрена.",
                new Vector3(8f, 0.5f, 8f),
                materials.RustMetal,
                questManager);
            CreateInspectableSystem(
                parent,
                "Relay Station",
                "Relay Station",
                PowerSystemType.RelayStation,
                "Релейная станция осмотрена.",
                new Vector3(0f, 0.5f, 12f),
                materials.TechBlue,
                questManager);

            var generator = CreateGeneratorG17(parent, new Vector3(0f, 0.9f, 3f), materials, gameManager, combatManager, questManager, notificationUI);
            CreateSwitchRoomConsole(parent, new Vector3(0f, 0.6f, -8f), materials, gameManager, questManager, powerManager, notificationUI);
            return generator;
        }

        private static void CreateBreakerModule(Transform parent, string name, string markerLabel, Vector3 position, MinimalMaterials materials, QuestManager questManager)
        {
            var moduleObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            moduleObject.name = name;
            moduleObject.transform.SetParent(parent);
            moduleObject.transform.position = position;
            moduleObject.transform.localScale = new Vector3(0.8f, 0.35f, 0.8f);

            var renderer = moduleObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = materials.WarmLightSource;
            }

            var pickup = moduleObject.AddComponent<BreakerModulePickup>();
            pickup.Configure("Подобрать breaker-модуль", "[Клик]", 4.5f);
            AddQuestMarker(moduleObject, markerLabel, questManager, new Vector3(0f, 1.45f, 0f), MissionFlowController.FindBreakerModulesObjectiveId);
        }

        private static void CreateInspectableSystem(
            Transform parent,
            string displayName,
            string markerLabel,
            PowerSystemType systemType,
            string inspectionMessage,
            Vector3 position,
            Material material,
            QuestManager questManager)
        {
            var systemObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            systemObject.name = displayName;
            systemObject.transform.SetParent(parent);
            systemObject.transform.position = position;
            systemObject.transform.localScale = new Vector3(1.4f, 1f, 1.4f);

            var renderer = systemObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            var inspectable = systemObject.AddComponent<InspectablePowerSystem>();
            inspectable.Configure($"Осмотреть: {displayName}", "[Клик]", 5f);
            inspectable.ConfigurePowerSystem(systemType, inspectionMessage);
            AddQuestMarker(
                systemObject,
                markerLabel,
                questManager,
                new Vector3(0f, 1.75f, 0f),
                MissionFlowController.ResolvePowerPrioritiesObjectiveId,
                MissionFlowController.InspectKeySystemsObjectiveId);
        }

        private static GeneratorG17 CreateGeneratorG17(
            Transform parent,
            Vector3 position,
            MinimalMaterials materials,
            GameManager gameManager,
            CombatManager combatManager,
            QuestManager questManager,
            NotificationUI notificationUI)
        {
            var generatorObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            generatorObject.name = "GeneratorG17";
            generatorObject.transform.SetParent(parent);
            generatorObject.transform.position = position;
            generatorObject.transform.localScale = new Vector3(2.6f, 1.8f, 1.3f);

            var renderer = generatorObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = materials.RustMetal;
            }

            var generator = generatorObject.AddComponent<GeneratorG17>();
            generator.Configure("Починить / запустить Generator G-17", "[Клик]", 5f);
            generator.ConfigureStats(420);
            generator.SetReferences(gameManager, combatManager, questManager, notificationUI);
            AddQuestMarker(
                generatorObject,
                "Generator G-17",
                questManager,
                new Vector3(0f, 2.6f, 0f),
                MissionFlowController.RepairGeneratorLineObjectiveId,
                MissionFlowController.StartGeneratorObjectiveId,
                MissionFlowController.DefendGeneratorObjectiveId);
            return generator;
        }

        private static void CreateSwitchRoomConsole(
            Transform parent,
            Vector3 position,
            MinimalMaterials materials,
            GameManager gameManager,
            QuestManager questManager,
            PowerManager powerManager,
            NotificationUI notificationUI)
        {
            var consoleObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            consoleObject.name = "SwitchRoomConsole";
            consoleObject.transform.SetParent(parent);
            consoleObject.transform.position = position;
            consoleObject.transform.localScale = new Vector3(1.8f, 1.2f, 0.8f);

            var renderer = consoleObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = materials.TechBlue;
            }

            var console = consoleObject.AddComponent<SwitchRoomConsole>();
            console.Configure("Использовать консоль Switch Room", "[Клик]", 5f);
            console.SetReferences(gameManager, questManager, powerManager, notificationUI);
            AddQuestMarker(consoleObject, "Switch Room Console", questManager, new Vector3(0f, 1.9f, 0f), MissionFlowController.AllocatePowerObjectiveId);
        }

        private static void CreatePlaceholderNpcs(Transform parent, Dictionary<string, DialogueData> dialogueAssets, MinimalMaterials materials, QuestManager questManager)
        {
            var mara = CreateNpc(parent, "Mara", "Поговорить с Mara", dialogueAssets["D01_MARA_INTRO"], new Vector3(0f, 1f, -6f), materials.DirtyPlastic, materials);
            AddQuestMarker(mara, "Mara / убежище", questManager, new Vector3(0f, 2.35f, 0f), MissionFlowController.ReachShelterObjectiveId, MissionFlowController.SpeakWithMaraObjectiveId);

            AddQuestMarker(CreateNpc(parent, "Hale", "Поговорить с Hale", dialogueAssets["D02_HALE_DEFENSE"], new Vector3(2f, 1f, 7f), materials.DangerRed, materials), "Hale", questManager, new Vector3(0f, 2.35f, 0f), MissionFlowController.ResolvePowerPrioritiesObjectiveId);
            AddQuestMarker(CreateNpc(parent, "Ivo", "Поговорить с Ivo", dialogueAssets["D04_IVO_FARM"], new Vector3(-4.4f, 1f, 6f), materials.DeadVegetation, materials), "Ivo", questManager, new Vector3(0f, 2.35f, 0f), MissionFlowController.ResolvePowerPrioritiesObjectiveId);
            AddQuestMarker(CreateNpc(parent, "Sela", "Поговорить с Sela", dialogueAssets["D03_SELA_WATER"], new Vector3(-10f, 1f, 6f), materials.TechBlue, materials), "Sela", questManager, new Vector3(0f, 2.35f, 0f), MissionFlowController.ResolvePowerPrioritiesObjectiveId);
            AddQuestMarker(CreateNpc(parent, "Edda", "Поговорить с Edda", dialogueAssets["D05_EDDA_STAGE"], new Vector3(8f, 1f, -4.2f), materials.WarmLightSource, materials), "Edda", questManager, new Vector3(0f, 2.35f, 0f), MissionFlowController.ResolvePowerPrioritiesObjectiveId);
            AddQuestMarker(CreateNpc(parent, "Greer", "Поговорить с Greer", dialogueAssets["D06_GREER_WORKSHOP"], new Vector3(8f, 1f, 3f), materials.RustMetal, materials), "Greer", questManager, new Vector3(0f, 2.35f, 0f), MissionFlowController.ResolvePowerPrioritiesObjectiveId);
            AddQuestMarker(CreateNpc(parent, "Lysa", "Поговорить с Lysa", dialogueAssets["D07_LYSA_RELAY"], new Vector3(0f, 1f, 13.3f), materials.TechBlue, materials), "Lysa", questManager, new Vector3(0f, 2.35f, 0f), MissionFlowController.ResolvePowerPrioritiesObjectiveId);
            AddQuestMarker(CreateNpc(parent, "PartyReserveDiscussion", "Обсудить резерв отряда", dialogueAssets["D08_PARTY_RESERVE"], new Vector3(2.8f, 1f, -6f), materials.ColdConcrete, materials), "Резерв отряда", questManager, new Vector3(0f, 2.35f, 0f), MissionFlowController.ResolvePowerPrioritiesObjectiveId);
        }

        private static GameObject CreateNpc(
            Transform parent,
            string objectName,
            string prompt,
            DialogueData dialogueData,
            Vector3 position,
            Material material,
            MinimalMaterials materials)
        {
            var npcObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            npcObject.name = objectName;
            npcObject.transform.SetParent(parent);
            npcObject.transform.position = position;

            var renderer = npcObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            var npcDialogue = npcObject.AddComponent<NPCDialogueInteractable>();
            npcDialogue.Configure(prompt, "[Клик]", 5f);
            npcDialogue.SetDialogue(dialogueData);
            CreateNpcRoleProp(npcObject.transform, objectName, materials);
            return npcObject;
        }

        private static void CreateNpcRoleProp(Transform parent, string objectName, MinimalMaterials materials)
        {
            switch (objectName)
            {
                case "Mara":
                    CreateChildCube(parent, "Leader_Clipboard", new Vector3(0.44f, 1.25f, 0.18f), new Vector3(0.08f, 0.42f, 0.28f), materials.DirtyPlastic);
                    CreateChildCube(parent, "Neutral_Shoulder_Mark", new Vector3(-0.4f, 1.55f, 0f), new Vector3(0.18f, 0.18f, 0.32f), materials.ColdConcrete);
                    break;
                case "Hale":
                    CreateChildCube(parent, "Defense_Shoulder_Plate", new Vector3(0f, 1.55f, 0.5f), new Vector3(0.72f, 0.18f, 0.14f), materials.DangerRed);
                    CreateChildCube(parent, "Defense_Radio", new Vector3(-0.42f, 1.15f, 0f), new Vector3(0.16f, 0.38f, 0.18f), materials.RustMetal);
                    break;
                case "Ivo":
                    CreateChildCube(parent, "Farm_Tray", new Vector3(0f, 0.82f, 0.55f), new Vector3(0.72f, 0.12f, 0.28f), materials.DeadVegetation);
                    break;
                case "Sela":
                    CreateChildCube(parent, "Medic_Case", new Vector3(0.46f, 0.9f, 0f), new Vector3(0.22f, 0.36f, 0.28f), materials.DirtyPlastic);
                    CreateChildCube(parent, "Medic_Case_Mark", new Vector3(0.58f, 0.9f, 0f), new Vector3(0.04f, 0.22f, 0.12f), materials.TechBlue);
                    break;
                case "Edda":
                    CreateChildCube(parent, "Stage_Sash", new Vector3(0f, 1.45f, 0.5f), new Vector3(0.18f, 0.65f, 0.08f), materials.WarmLightSource);
                    break;
                case "Greer":
                    CreateChildCube(parent, "Toolbox", new Vector3(-0.45f, 0.75f, 0f), new Vector3(0.26f, 0.34f, 0.32f), materials.RustMetal);
                    CreateChildCube(parent, "Toolbox_Light", new Vector3(-0.58f, 0.84f, 0f), new Vector3(0.04f, 0.1f, 0.18f), materials.WarmLightSource);
                    break;
                case "Lysa":
                    CreateChildCube(parent, "Relay_Antenna_Pack", new Vector3(0f, 1.2f, -0.48f), new Vector3(0.38f, 0.5f, 0.2f), materials.TechBlue);
                    CreateChildCube(parent, "Relay_Antenna_Rod", new Vector3(0.25f, 1.85f, -0.15f), new Vector3(0.06f, 0.8f, 0.06f), materials.TechBlue);
                    break;
                case "PartyReserveDiscussion":
                    CreateChildCube(parent, "Reserve_Cell", new Vector3(0f, 0.75f, 0.55f), new Vector3(0.45f, 0.28f, 0.18f), materials.TechBlue);
                    break;
            }
        }

        private static QuestMarkerTarget AddQuestMarker(GameObject target, string label, QuestManager questManager, Vector3 offset, params string[] objectiveIds)
        {
            if (target == null)
            {
                return null;
            }

            var marker = target.AddComponent<QuestMarkerTarget>();
            marker.SetQuestManager(questManager);
            marker.Configure(label, objectiveIds, offset, new Color(1f, 0.86f, 0.22f));
            return marker;
        }

        private static Dictionary<string, AbilityData> EnsureAbilityAssets()
        {
            EnsureAssetFolder(AbilityDataPath);

            var abilities = new Dictionary<string, AbilityData>
            {
                ["dax_repair_burst"] = EnsureAbilityDataAsset(
                    "SO_Ability_Dax_RepairBurst",
                    "dax_repair_burst",
                    "Repair Burst",
                    "Чинит Generator G-17.",
                    8f,
                    14,
                    AbilityTargetType.Interactable,
                    AbilityEffectType.RepairGenerator,
                    75,
                    7f,
                    0f),
                ["dax_suppressive_shot"] = EnsureAbilityDataAsset(
                    "SO_Ability_Dax_SuppressiveShot",
                    "dax_suppressive_shot",
                    "Suppressive Shot",
                    "Сильный выстрел по врагу.",
                    6f,
                    12,
                    AbilityTargetType.Enemy,
                    AbilityEffectType.DamageEnemy,
                    38,
                    9f,
                    0f),
                ["nyra_shock_pulse"] = EnsureAbilityDataAsset(
                    "SO_Ability_Nyra_ShockPulse",
                    "nyra_shock_pulse",
                    "Shock Pulse",
                    "Урон и короткий сбой врага.",
                    7f,
                    16,
                    AbilityTargetType.Enemy,
                    AbilityEffectType.ShockEnemy,
                    30,
                    9f,
                    2.5f),
                ["nyra_overload_panel"] = EnsureAbilityDataAsset(
                    "SO_Ability_Nyra_OverloadPanel",
                    "nyra_overload_panel",
                    "Overload Panel",
                    "Прототип: второй импульс по ближайшей цели.",
                    9f,
                    18,
                    AbilityTargetType.Enemy,
                    AbilityEffectType.ShockEnemy,
                    36,
                    9f,
                    1.5f),
                ["cormac_emergency_heal"] = EnsureAbilityDataAsset(
                    "SO_Ability_Cormac_EmergencyHeal",
                    "cormac_emergency_heal",
                    "Emergency Heal",
                    "Лечит самого раненого союзника.",
                    9f,
                    16,
                    AbilityTargetType.Ally,
                    AbilityEffectType.HealAlly,
                    45,
                    8f,
                    0f),
                ["cormac_field_stabilize"] = EnsureAbilityDataAsset(
                    "SO_Ability_Cormac_FieldStabilize",
                    "cormac_field_stabilize",
                    "Field Stabilize",
                    "Небольшая стабилизация HP.",
                    7f,
                    12,
                    AbilityTargetType.Ally,
                    AbilityEffectType.StabilizeAlly,
                    28,
                    8f,
                    0f)
            };

            return abilities;
        }

        private static AbilityData EnsureAbilityDataAsset(
            string assetName,
            string id,
            string displayName,
            string description,
            float cooldown,
            int energyCost,
            AbilityTargetType targetType,
            AbilityEffectType effectType,
            int powerAmount,
            float range,
            float stunDuration)
        {
            var path = $"{AbilityDataPath}/{assetName}.asset";
            var abilityData = AssetDatabase.LoadAssetAtPath<AbilityData>(path);

            if (abilityData == null)
            {
                abilityData = ScriptableObject.CreateInstance<AbilityData>();
                AssetDatabase.CreateAsset(abilityData, path);
            }

            abilityData.Id = id;
            abilityData.DisplayName = displayName;
            abilityData.Description = description;
            abilityData.Cooldown = cooldown;
            abilityData.EnergyCost = energyCost;
            abilityData.TargetType = targetType;
            abilityData.EffectType = effectType;
            abilityData.PowerAmount = powerAmount;
            abilityData.Range = range;
            abilityData.StunDuration = stunDuration;
            EditorUtility.SetDirty(abilityData);
            return abilityData;
        }

        private static List<AbilityData> GetAbilities(Dictionary<string, AbilityData> abilityAssets, params string[] ids)
        {
            var abilities = new List<AbilityData>();
            if (abilityAssets == null || ids == null)
            {
                return abilities;
            }

            for (var i = 0; i < ids.Length; i++)
            {
                if (abilityAssets.TryGetValue(ids[i], out var ability) && ability != null)
                {
                    abilities.Add(ability);
                }
            }

            return abilities;
        }

        private static void EnsureAssetFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            var parent = System.IO.Path.GetDirectoryName(folderPath)?.Replace("\\", "/");
            var folderName = System.IO.Path.GetFileName(folderPath);
            if (!string.IsNullOrEmpty(parent) && !string.IsNullOrEmpty(folderName))
            {
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }

        private static CharacterData EnsureCharacterDataAsset(
            string assetName,
            string id,
            string displayName,
            string role,
            int maxHealth,
            int maxEnergy,
            int attackDamage,
            float attackRange,
            float attackCooldown,
            List<AbilityData> abilities)
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
            characterData.AttackDamage = attackDamage;
            characterData.AttackRange = attackRange;
            characterData.AttackCooldown = attackCooldown;
            characterData.Abilities.Clear();
            if (abilities != null)
            {
                characterData.Abilities.AddRange(abilities);
            }
            EditorUtility.SetDirty(characterData);

            return characterData;
        }

        private static List<EnemyData> EnsureEnemyDataAssets()
        {
            var scavenger = EnsureEnemyDataAsset(
                "SO_Enemy_RaiderScavenger",
                "raider_scavenger",
                "Raider Scavenger",
                70,
                7,
                5.8f,
                2.2f,
                2.35f,
                EnemyRole.Scavenger);
            var gunner = EnsureEnemyDataAsset(
                "SO_Enemy_RaiderGunner",
                "raider_gunner",
                "Raider Gunner",
                95,
                12,
                7.5f,
                2.6f,
                1.9f,
                EnemyRole.Gunner);
            var runner = EnsureEnemyDataAsset(
                "SO_Enemy_RaiderRunner",
                "raider_runner",
                "Raider Runner",
                65,
                8,
                3.2f,
                1.8f,
                3.5f,
                EnemyRole.Runner);
            var saboteur = EnsureEnemyDataAsset(
                "SO_Enemy_RaiderSaboteur",
                "raider_saboteur",
                "Raider Saboteur",
                85,
                16,
                2.6f,
                2.4f,
                2.85f,
                EnemyRole.Saboteur);

            return new List<EnemyData>
            {
                scavenger,
                scavenger,
                gunner,
                runner,
                saboteur
            };
        }

        private static EnemyData EnsureEnemyDataAsset(
            string assetName,
            string id,
            string displayName,
            int maxHealth,
            int damage,
            float attackRange,
            float attackCooldown,
            float moveSpeed,
            EnemyRole role)
        {
            var path = $"{EnemyDataPath}/{assetName}.asset";
            var enemyData = AssetDatabase.LoadAssetAtPath<EnemyData>(path);

            if (enemyData == null)
            {
                enemyData = ScriptableObject.CreateInstance<EnemyData>();
                AssetDatabase.CreateAsset(enemyData, path);
            }

            enemyData.Id = id;
            enemyData.DisplayName = displayName;
            enemyData.MaxHealth = maxHealth;
            enemyData.Damage = damage;
            enemyData.AttackRange = attackRange;
            enemyData.AttackCooldown = attackCooldown;
            enemyData.MoveSpeed = moveSpeed;
            enemyData.Role = role;
            EditorUtility.SetDirty(enemyData);
            return enemyData;
        }

        private static List<PowerSystemData> EnsurePowerSystemAssets()
        {
            return new List<PowerSystemData>
            {
                EnsurePowerSystemAsset(
                    "SO_Power_WaterFilters",
                    "water_filters",
                    "Water Filters",
                    "Стабильное питание для фильтров воды и насосов качества.",
                    35,
                    "Чистая вода, меньше болезней, Dr. Sela получает аргумент, который можно измерить жизнями.",
                    "Hydroponic Farm остается без нужного питания, и Ivo теряет часть будущего урожая.",
                    "Dr. Sela / Cormac",
                    new[] { "Ivo" },
                    PowerChoice.WaterFilters),
                EnsurePowerSystemAsset(
                    "SO_Power_HydroponicFarm",
                    "hydroponic_farm",
                    "Hydroponic Farm",
                    "Питание для ламп, насосов и питательной линии фермы.",
                    40,
                    "Рассада выживет, а у Riverside останется шанс на еду после ближайших запасов.",
                    "Фильтры воды останутся нестабильными, и Dr. Sela будет считать это отложенной катастрофой.",
                    "Ivo / Nyra",
                    new[] { "Dr. Sela" },
                    PowerChoice.HydroponicFarm),
                EnsurePowerSystemAsset(
                    "SO_Power_DefenseGrid",
                    "defense_grid",
                    "Defense Grid",
                    "Питание электрозабора, прожекторов и базовой защиты периметра.",
                    30,
                    "Hale получает рабочий периметр, и Riverside становится сложнее взять налетом.",
                    "Сцена и гражданские системы снова покажутся вторичными.",
                    "Hale",
                    new[] { "Edda" },
                    PowerChoice.DefenseGrid),
                EnsurePowerSystemAsset(
                    "SO_Power_PublicStage",
                    "public_stage",
                    "Public Stage",
                    "Свет для общей сцены, собраний и публичного порядка.",
                    15,
                    "Edda получает место, где люди снова видят друг друга, а не только дефицит.",
                    "Hale и прагматики назовут это светом, который не держит ворота.",
                    "Edda",
                    new[] { "Hale", "прагматики" },
                    PowerChoice.PublicStage),
                EnsurePowerSystemAsset(
                    "SO_Power_Workshop",
                    "workshop",
                    "Workshop",
                    "Питание для зарядной стойки, инструментов и ремонтного стенда.",
                    20,
                    "Greer сможет чинить то, что держит Riverside живым дольше одного вечера.",
                    "Sela и Ivo будут видеть в этом ремонт вместо немедленного спасения воды или еды.",
                    "Greer / Dax",
                    new[] { "Dr. Sela", "Ivo" },
                    PowerChoice.Workshop),
                EnsurePowerSystemAsset(
                    "SO_Power_RelayStation",
                    "relay_station",
                    "Relay Station",
                    "Питание релейной антенны и терминала связи.",
                    25,
                    "Lysa получает шанс поймать Blind Protocol и доказать, что Riverside не один.",
                    "Mara и Sela увидят риск потратить свет на тишину.",
                    "Lysa / Nyra",
                    new[] { "Mara", "Dr. Sela" },
                    PowerChoice.RelayStation),
                EnsurePowerSystemAsset(
                    "SO_Power_PartyReserve",
                    "party_reserve",
                    "Party Reserve",
                    "Часть стабильной мощности уходит в переносные ячейки отряда.",
                    20,
                    "Отряд получает заряд на будущую угрозу и больше свободы за пределами Riverside.",
                    "Riverside видит, что внешний отряд забрал свет себе. Cormac не сможет назвать это чистым решением.",
                    "Dax / Nyra",
                    new[] { "почти все местные", "Cormac" },
                    PowerChoice.PartyReserve),
                EnsurePowerSystemAsset(
                    "SO_Power_SplitLoad",
                    "split_load",
                    "Split Load",
                    "Осторожная компромиссная схема без полного питания одной системы.",
                    100,
                    "Никто не получает победу, но Riverside видит попытку удержать поселение целым.",
                    "Каждая группа получает меньше, чем просила, и ни одна проблема не решена полностью.",
                    "Mara",
                    new[] { "все недовольны частично" },
                    PowerChoice.SplitLoad)
            };
        }

        private static PowerSystemData EnsurePowerSystemAsset(
            string assetName,
            string id,
            string displayName,
            string description,
            int requiredPower,
            string benefitDescription,
            string downsideDescription,
            string supportingNpcId,
            string[] opposingNpcIds,
            PowerChoice powerChoice)
        {
            var path = $"{PowerSystemDataPath}/{assetName}.asset";
            var powerSystemData = AssetDatabase.LoadAssetAtPath<PowerSystemData>(path);

            if (powerSystemData == null)
            {
                powerSystemData = ScriptableObject.CreateInstance<PowerSystemData>();
                AssetDatabase.CreateAsset(powerSystemData, path);
            }

            powerSystemData.Id = id;
            powerSystemData.DisplayName = displayName;
            powerSystemData.Description = description;
            powerSystemData.RequiredPower = requiredPower;
            powerSystemData.BenefitDescription = benefitDescription;
            powerSystemData.DownsideDescription = downsideDescription;
            powerSystemData.SupportingNPCId = supportingNpcId;
            powerSystemData.OpposingNPCIds.Clear();
            powerSystemData.OpposingNPCIds.AddRange(opposingNpcIds);
            powerSystemData.PowerChoice = powerChoice;
            EditorUtility.SetDirty(powerSystemData);
            return powerSystemData;
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
            questData.Description = "Основная миссия Riverside: восстановить питание, защитить генератор и принять финальное решение.";
            questData.Objectives.Clear();
            AddObjective(questData, MissionFlowController.ReachShelterObjectiveId, "Добраться до убежища Riverside");
            AddObjective(questData, MissionFlowController.SpeakWithMaraObjectiveId, "Поговорить с Mara Vey");
            AddObjective(questData, MissionFlowController.ResolvePowerPrioritiesObjectiveId, "Узнать энергетические приоритеты");
            AddObjective(questData, MissionFlowController.InspectKeySystemsObjectiveId, "Осмотреть ключевые системы: 0/6");
            AddObjective(questData, MissionFlowController.FindBreakerModulesObjectiveId, "Найти 2 breaker-модуля: 0/2");
            AddObjective(questData, MissionFlowController.RepairGeneratorLineObjectiveId, "Починить Generator G-17");
            AddObjective(questData, MissionFlowController.StartGeneratorObjectiveId, "Запустить Generator G-17");
            AddObjective(questData, MissionFlowController.DefendGeneratorObjectiveId, "Защитить Generator G-17");
            AddObjective(questData, MissionFlowController.AllocatePowerObjectiveId, "Распределить питание");
            AddObjective(questData, MissionFlowController.FaceConsequencesObjectiveId, "Посмотреть последствия");
            AddObjective(questData, MissionFlowController.LeaveRiversideObjectiveId, "Завершить миссию");

            EditorUtility.SetDirty(questData);
            return questData;
        }

        private static void AddObjective(QuestData questData, string id, string description)
        {
            questData.Objectives.Add(new ObjectiveData
            {
                Id = id,
                Description = description,
                Status = ObjectiveStatus.Hidden
            });
        }

        private static Dictionary<string, DialogueData> EnsureDialogueAssets(QuestData questData)
        {
            return new Dictionary<string, DialogueData>
            {
                ["D01_MARA_INTRO"] = EnsureDialogueAsset(
                    "SO_Dialogue_Mara_Intro",
                    "D01_MARA_INTRO",
                    "Mara",
                    "mara",
                    "Mara",
                    "Вы та самая внешняя группа. В темноте все звучит дорого.",
                    new[]
                    {
                        "Нам сказали, вы контролируете проход.",
                        "Мы слышали, у вас проблема с генератором.",
                        "Мы не работаем бесплатно."
                    },
                    "G-17 еще можно запустить. Но когда генератор заработает, у нас будет одно чистое решение и шесть злых очередей.",
                    "Nyra: Сколько стабильной мощности?",
                    new[]
                    {
                        "Кто претендует на энергию?",
                        "Почему нельзя разделить нагрузку?",
                        "Где breaker-модули?"
                    },
                    new[]
                    {
                        SetFlag("has_met_mara"),
                        SetFlag("power_priorities_started"),
                        CompleteObjective(MissionFlowController.SpeakWithMaraObjectiveId),
                        ActivateObjective(MissionFlowController.ResolvePowerPrioritiesObjectiveId)
                    }),
                ["D02_HALE_DEFENSE"] = EnsureDialogueAsset(
                    "SO_Dialogue_Hale_Defense",
                    "D02_HALE_DEFENSE",
                    "Hale",
                    "hale",
                    "Hale",
                    "Не трогайте проволоку. Мертвое не всегда бесполезное.",
                    new[] { "Ты хочешь ее запитать.", "Это твой вступительный аргумент?", "Мы не собирались лезть через твой забор." },
                    "Дайте мне забор - и остальные получат время быть правыми. Не дадите - узнаем, кто умеет спорить в темноте.",
                    "Nyra: Коробка может стать клеткой.",
                    new[] { "Почему оборона должна быть первой?", "Насколько близко рейдеры?", "Мы услышали тебя." },
                    new[] { SetFlag("talked_to_hale") }),
                ["D03_SELA_WATER"] = EnsureDialogueAsset(
                    "SO_Dialogue_Sela_Water",
                    "D03_SELA_WATER",
                    "Sela",
                    "sela",
                    "Sela",
                    "Не смотрите так. Эта вода только притворялась чистой.",
                    new[] { "Она выглядела нормально.", "Вы врач?", "Cormac, что видишь?" },
                    "Когда будете выбирать, не думайте о трубах. Думайте о том, сколько людей уже носят плохую воду внутри себя.",
                    "Cormac: Ни запаха, ни цвета. Это хуже, чем грязь.",
                    new[] { "Кипячение не помогает?", "Насколько все плохо?", "Мы учтем фильтры." },
                    new[] { SetFlag("talked_to_sela") }),
                ["D04_IVO_FARM"] = EnsureDialogueAsset(
                    "SO_Dialogue_Ivo_Farm",
                    "D04_IVO_FARM",
                    "Ivo",
                    "ivo",
                    "Ivo",
                    "Осторожнее. Пол здесь наполовину кабель, наполовину надежда.",
                    new[] { "Это ферма?", "Выглядит хрупко.", "Почему не выращивать снаружи?" },
                    "Когда будете стоять у панели, запомните: голод терпелив. Поэтому люди его недооценивают.",
                    "Nyra: Растения умирают достаточно вежливо, чтобы ты успел почувствовать себя виноватым.",
                    new[] { "Сколько ферма продержится без питания?", "Села говорит, люди болеют сейчас.", "Мы услышали аргумент фермы." },
                    new[] { SetFlag("talked_to_ivo") }),
                ["D05_EDDA_STAGE"] = EnsureDialogueAsset(
                    "SO_Dialogue_Edda_Stage",
                    "D05_EDDA_STAGE",
                    "Edda",
                    "edda",
                    "Edda",
                    "Осторожнее с третьей ступенью. Она визжит во время речей и врет во время трагедий.",
                    new[] { "Ты Эдда.", "Ты хочешь энергию для сцены?", "У нас нет времени на театр." },
                    "Темнота не только прячет врагов. Она превращает соседей в незнакомцев.",
                    "Dax: Почти ненавижу, что понял это.",
                    new[] { "Мораль не чистит воду.", "Сцена дает тебе влияние.", "Мы запомним сцену." },
                    new[] { SetFlag("talked_to_edda") }),
                ["D06_GREER_WORKSHOP"] = EnsureDialogueAsset(
                    "SO_Dialogue_Greer_Workshop",
                    "D06_GREER_WORKSHOP",
                    "Greer",
                    "greer",
                    "Greer",
                    "Если пришли спросить, что мне нужно, смотрите на стену. Если почему - смотрите на что угодно другое.",
                    new[] { "Ты ведешь список.", "Что ты можешь починить?", "Dax, что думаешь?" },
                    "Выбирайте то, с чем сможете спать. Но когда оно сломается - а оно сломается - вспомните, кто просил свет до дыма.",
                    "Dax: Он оскорбляет в единицах измерения.",
                    new[] { "Сильный аргумент.", "Ты получишь контроль над ремонтом.", "Мы услышали мастерскую." },
                    new[] { SetFlag("talked_to_greer") }),
                ["D07_LYSA_RELAY"] = EnsureDialogueAsset(
                    "SO_Dialogue_Lysa_Relay",
                    "D07_LYSA_RELAY",
                    "Lysa",
                    "lysa",
                    "Lysa",
                    "Секунду не говорите. Под шумом есть несущая.",
                    new[] { "Мы насчет реле.", "Ты слушаешь помехи?", "Nyra?" },
                    "Если дадите мне питание, я могу не услышать ничего. Но если не дадите, мы никогда не узнаем, ждало ли это ничего ответа.",
                    "Nyra: Она права. Под шумом есть несущая.",
                    new[] { "Ты про Blind Protocol.", "Оно может связаться с другим поселением?", "Мы услышали реле." },
                    new[] { SetFlag("talked_to_lysa") }),
                ["D08_PARTY_RESERVE"] = EnsureDialogueAsset(
                    "SO_Dialogue_Party_Reserve",
                    "D08_PARTY_RESERVE",
                    "Party Reserve",
                    "nyra",
                    "Nyra",
                    "Мы можем взять резервный заряд.",
                    new[] { "На что пойдет резерв?", "Сколько потеряет Riverside?", "Можно сделать это незаметно?" },
                    "Я оставлю ячейки подключенными, но не активными. Решение за тобой.",
                    "Cormac: Плохая фраза, чтобы на ней закончить.",
                    new[] { "Энергия остается здесь.", "Запомним этот вариант." },
                    Array.Empty<DialogueEffect>())
            };
        }

        private static DialogueData EnsureDialogueAsset(
            string assetName,
            string id,
            string speakerName,
            string speakerId,
            string startSpeakerName,
            string startText,
            string[] openingChoices,
            string argumentText,
            string companionComment,
            string[] exitChoices,
            DialogueEffect[] exitEffects)
        {
            var path = $"{DialogueDataPath}/{assetName}.asset";
            var dialogueData = AssetDatabase.LoadAssetAtPath<DialogueData>(path);

            if (dialogueData == null)
            {
                dialogueData = ScriptableObject.CreateInstance<DialogueData>();
                AssetDatabase.CreateAsset(dialogueData, path);
            }

            dialogueData.Id = id;
            dialogueData.SpeakerName = speakerName;
            dialogueData.StartNodeId = "start";
            dialogueData.Nodes.Clear();

            var startNode = new DialogueNode
            {
                NodeId = "start",
                SpeakerId = speakerId,
                SpeakerName = startSpeakerName,
                Text = startText
            };
            foreach (var choiceText in openingChoices)
            {
                startNode.Choices.Add(new DialogueChoice
                {
                    Text = choiceText,
                    NextNodeId = "argument"
                });
            }

            var argumentNode = new DialogueNode
            {
                NodeId = "argument",
                SpeakerId = speakerId,
                SpeakerName = speakerName,
                Text = argumentText,
                CompanionComment = companionComment
            };
            foreach (var choiceText in exitChoices)
            {
                argumentNode.Choices.Add(new DialogueChoice
                {
                    Text = choiceText,
                    Effects = new List<DialogueEffect>(exitEffects)
                });
            }

            dialogueData.Nodes.Add(startNode);
            dialogueData.Nodes.Add(argumentNode);
            EditorUtility.SetDirty(dialogueData);
            return dialogueData;
        }

        private static DialogueEffect SetFlag(string flag)
        {
            return new DialogueEffect
            {
                Type = DialogueEffectType.SetMissionFlag,
                Key = flag,
                BoolValue = true
            };
        }

        private static DialogueEffect StartQuest(QuestData questData)
        {
            return new DialogueEffect
            {
                Type = DialogueEffectType.StartQuest,
                Quest = questData
            };
        }

        private static DialogueEffect ActivateObjective(string objectiveId)
        {
            return new DialogueEffect
            {
                Type = DialogueEffectType.ActivateObjective,
                Key = objectiveId
            };
        }

        private static DialogueEffect CompleteObjective(string objectiveId)
        {
            return new DialogueEffect
            {
                Type = DialogueEffectType.CompleteObjective,
                Key = objectiveId
            };
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
