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
        private const string DialogueDataPath = "Assets/IsoLight/Data/Dialogues";
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
            var dialogueManager = CreateManager<DialogueManager>("DialogueManager", gameRoot.transform);
            var combatManager = CreateManager<CombatManager>("CombatManager", gameRoot.transform);
            var powerManager = CreateManager<PowerManager>("PowerManager", gameRoot.transform);
            var relationshipManager = CreateManager<RelationshipManager>("RelationshipManager", gameRoot.transform);
            var saveManager = CreateManager<SaveManager>("SaveManager", gameRoot.transform);
            var uiManager = CreateManager<UIManager>("UIManager", gameRoot.transform);

            CreateGround(levelRoot.transform, materials);
            CreateLevelBlockout(levelRoot.transform, materials);
            TryBuildNavMesh(levelRoot);
            CreateSceneLighting(materials);

            var cameraController = CreateMainCamera();
            var clickToMoveController = inputManager.gameObject.AddComponent<ClickToMoveController>();
            var partyCharacters = CreatePlaceholderParty(playerPartyRoot.transform, materials);
            var questData = EnsureQuestDataAsset();
            var dialogueAssets = EnsureDialogueAssets(questData);

            var dialoguePanel = CreatePlaceholderUI(uiRoot.transform, questManager);
            CreateQuestInteractables(interactablesRoot.transform, materials);
            CreatePlaceholderNpcs(npcsRoot.transform, dialogueAssets, materials);

            partyManager.SetPartyMembers(partyCharacters);
            questManager.SetStartupQuest(questData, "find_breaker_modules");
            inputManager.SetReferences(gameManager, partyManager);
            cameraManager.SetReferences(partyManager, cameraController);
            clickToMoveController.SetReferences(gameManager, partyManager, UnityEngine.Camera.main);
            dialogueManager.SetReferences(gameManager, questManager, dialoguePanel);

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

        private static MinimalMaterials EnsureMinimalMaterials()
        {
            return new MinimalMaterials
            {
                WetAsphaltDark = EnsureMaterialAsset("MAT_WetAsphalt_Dark", new Color(0.03f, 0.045f, 0.06f)),
                ColdConcrete = EnsureMaterialAsset("MAT_ColdConcrete", new Color(0.38f, 0.42f, 0.45f)),
                RustMetal = EnsureMaterialAsset("MAT_RustMetal", new Color(0.38f, 0.2f, 0.12f)),
                DirtyPlastic = EnsureMaterialAsset("MAT_DirtyPlastic", new Color(0.68f, 0.66f, 0.58f)),
                DeadVegetation = EnsureMaterialAsset("MAT_DeadVegetation", new Color(0.24f, 0.35f, 0.22f)),
                WarmLightSource = EnsureMaterialAsset("MAT_WarmLightSource", new Color(1f, 0.72f, 0.24f), true),
                TechBlue = EnsureMaterialAsset("MAT_TechBlue", new Color(0.18f, 0.65f, 1f), true),
                DangerRed = EnsureMaterialAsset("MAT_DangerRed", new Color(0.65f, 0.08f, 0.06f), true)
            };
        }

        private static Material EnsureMaterialAsset(string materialName, Color color, bool emissive = false)
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

            if (emissive)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 1.25f);
            }
            else
            {
                material.DisableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", Color.black);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static Shader GetPrototypeShader()
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            return shader != null ? shader : Shader.Find("Standard");
        }

        private static DialoguePanelUI CreatePlaceholderUI(Transform parent, QuestManager questManager)
        {
            var promptObject = new GameObject("InteractionPromptUI");
            promptObject.transform.SetParent(parent);
            promptObject.AddComponent<InteractionPromptUI>();

            var notificationObject = new GameObject("NotificationUI");
            notificationObject.transform.SetParent(parent);
            notificationObject.AddComponent<NotificationUI>();

            var objectiveObject = new GameObject("ObjectivePanelUI");
            objectiveObject.transform.SetParent(parent);
            var objectivePanel = objectiveObject.AddComponent<ObjectivePanelUI>();
            objectivePanel.SetQuestManager(questManager);

            var dialogueObject = new GameObject("DialoguePanelUI");
            dialogueObject.transform.SetParent(parent);
            return dialogueObject.AddComponent<DialoguePanelUI>();
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

            CreateColdRuins(blockoutRoot.transform, materials);
            CreateFilterStationProps(blockoutRoot.transform, materials);
            CreateHydroponicFarmProps(blockoutRoot.transform, materials);
            CreateDefenseGateProps(blockoutRoot.transform, materials);
            CreatePublicStageProps(blockoutRoot.transform, materials);
            CreateWorkshopProps(blockoutRoot.transform, materials);
            CreateRelayStationProps(blockoutRoot.transform, materials);
            CreateGeneratorYardProps(blockoutRoot.transform, materials);
        }

        private static void CreateZoneBase(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            CreateCube(parent, $"Zone_{name}", position, scale, material, false);
        }

        private static void CreatePath(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            CreateCube(parent, $"Path_{name}", position, scale, material, false);
        }

        private static void CreateColdRuins(Transform parent, MinimalMaterials materials)
        {
            CreateCube(parent, "Shelter_BackWall", new Vector3(0f, 1.6f, -8.7f), new Vector3(14f, 3.2f, 0.45f), materials.ColdConcrete, false);
            CreateCube(parent, "Shelter_LeftWall", new Vector3(-7.2f, 1.3f, -5.5f), new Vector3(0.45f, 2.6f, 5f), materials.ColdConcrete, false);
            CreateCube(parent, "Broken_Block_West", new Vector3(-15f, 1.2f, 0f), new Vector3(1.2f, 2.4f, 12f), materials.ColdConcrete, false);
            CreateCube(parent, "Broken_Block_East", new Vector3(15f, 1.2f, 2f), new Vector3(1.2f, 2.4f, 12f), materials.ColdConcrete, false);
            CreateCube(parent, "WaterWarning_Sign", new Vector3(-2.8f, 1.3f, -13.2f), new Vector3(1.8f, 1.1f, 0.12f), materials.DangerRed, false);
        }

        private static void CreateFilterStationProps(Transform parent, MinimalMaterials materials)
        {
            CreateCylinder(parent, "Filter_Tank_A", new Vector3(-11.2f, 1f, 8.6f), new Vector3(1.1f, 1.8f, 1.1f), materials.DirtyPlastic, false);
            CreateCylinder(parent, "Filter_Tank_B", new Vector3(-9.8f, 1f, 8.6f), new Vector3(1.1f, 1.8f, 1.1f), materials.DirtyPlastic, false);
            CreateCube(parent, "Filter_Terminal_Blue", new Vector3(-8.5f, 0.55f, 8.4f), new Vector3(0.8f, 1.1f, 0.45f), materials.TechBlue, false);
            CreateCube(parent, "Filter_Pipe_Long", new Vector3(-10.4f, 1.15f, 7f), new Vector3(3.3f, 0.18f, 0.18f), materials.RustMetal, false);
            CreateCylinder(parent, "Filter_Canister_A", new Vector3(-11.6f, 0.35f, 6.8f), new Vector3(0.35f, 0.7f, 0.35f), materials.DirtyPlastic, false);
            CreateCylinder(parent, "Filter_Canister_B", new Vector3(-8.7f, 0.35f, 6.8f), new Vector3(0.35f, 0.7f, 0.35f), materials.DirtyPlastic, false);
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
        }

        private static void CreateFence(Transform parent, MinimalMaterials materials, float x)
        {
            for (var i = 0; i < 4; i++)
            {
                CreateCube(parent, $"Defense_Fence_{x}_{i}", new Vector3(x, 0.65f, 7.2f + i * 1.2f), new Vector3(0.1f, 1.3f, 0.1f), materials.RustMetal, false);
            }
        }

        private static void CreatePublicStageProps(Transform parent, MinimalMaterials materials)
        {
            CreateCube(parent, "Stage_Platform", new Vector3(8f, 0.35f, -2.2f), new Vector3(4.4f, 0.7f, 2.4f), materials.RustMetal, false);
            CreateCylinder(parent, "Stage_MicrophoneStand", new Vector3(8f, 1.25f, -1.5f), new Vector3(0.08f, 1.6f, 0.08f), materials.RustMetal, false);
            CreateSphere(parent, "Stage_Microphone", new Vector3(8f, 2.1f, -1.5f), new Vector3(0.18f, 0.18f, 0.18f), materials.DirtyPlastic, false);
            CreateCube(parent, "Stage_Spotlight_Warm", new Vector3(6f, 1.6f, -4.1f), new Vector3(0.5f, 0.5f, 0.7f), materials.WarmLightSource, false);
            CreateCube(parent, "Stage_Bench_A", new Vector3(6.8f, 0.35f, 0.7f), new Vector3(2.2f, 0.25f, 0.45f), materials.ColdConcrete, false);
            CreateCube(parent, "Stage_Bench_B", new Vector3(9.2f, 0.35f, 0.7f), new Vector3(2.2f, 0.25f, 0.45f), materials.ColdConcrete, false);
        }

        private static void CreateWorkshopProps(Transform parent, MinimalMaterials materials)
        {
            CreateCube(parent, "Workshop_Bench", new Vector3(9f, 0.75f, 5f), new Vector3(3.5f, 0.35f, 1.2f), materials.RustMetal, false);
            CreateCube(parent, "Workshop_ToolWall", new Vector3(11.2f, 1.4f, 5f), new Vector3(0.25f, 2.2f, 2.8f), materials.ColdConcrete, false);
            CreateCube(parent, "Workshop_Crate_A", new Vector3(7f, 0.4f, 6.4f), new Vector3(0.8f, 0.8f, 0.8f), materials.DirtyPlastic, false);
            CreateCube(parent, "Workshop_Crate_B", new Vector3(7.9f, 0.4f, 6.4f), new Vector3(0.8f, 0.8f, 0.8f), materials.RustMetal, false);
            CreateCube(parent, "Workshop_ChargeRack", new Vector3(10.6f, 0.9f, 3.4f), new Vector3(0.7f, 1.8f, 0.5f), materials.WarmLightSource, false);
            CreateCube(parent, "Workshop_CableRun", new Vector3(9f, 0.08f, 3.5f), new Vector3(3.5f, 0.08f, 0.18f), materials.TechBlue, false);
        }

        private static void CreateRelayStationProps(Transform parent, MinimalMaterials materials)
        {
            CreateCube(parent, "Relay_Terminal_Blue", new Vector3(-1.2f, 0.65f, 14.4f), new Vector3(1.2f, 1.3f, 0.65f), materials.TechBlue, false);
            CreateCylinder(parent, "Relay_Antenna_Mast", new Vector3(1.2f, 2.2f, 15.1f), new Vector3(0.12f, 4.4f, 0.12f), materials.RustMetal, false);
            CreateCube(parent, "Relay_Antenna_Crossbar", new Vector3(1.2f, 3.9f, 15.1f), new Vector3(2f, 0.08f, 0.08f), materials.RustMetal, false);
            CreateCube(parent, "Relay_BlueBeacon", new Vector3(1.2f, 4.4f, 15.1f), new Vector3(0.32f, 0.32f, 0.32f), materials.TechBlue, false);
            CreateCube(parent, "Relay_CableCabinet", new Vector3(2.4f, 0.75f, 14.2f), new Vector3(0.8f, 1.5f, 0.7f), materials.ColdConcrete, false);
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
            RenderSettings.ambientLight = new Color(0.08f, 0.11f, 0.14f);

            var sunObject = new GameObject("Cold Evening Directional Light");
            var sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(0.45f, 0.56f, 0.68f);
            sun.intensity = 0.45f;
            sunObject.transform.rotation = Quaternion.Euler(55f, -30f, 0f);

            CreatePointLight("Shelter_Warm_Light", new Vector3(0f, 3.4f, -6.2f), new Color(1f, 0.68f, 0.35f), 2.5f, 9f);
            CreatePointLight("Filter_Cold_Light", new Vector3(-10f, 2.8f, 8f), new Color(0.45f, 0.8f, 1f), 2.2f, 7f);
            CreatePointLight("Farm_Grow_Light", new Vector3(-4f, 2.6f, 8f), new Color(0.9f, 0.75f, 0.35f), 2.6f, 7f);
            CreatePointLight("Workshop_Rust_Light", new Vector3(9f, 2.8f, 5f), new Color(1f, 0.45f, 0.2f), 2.4f, 7f);
            CreatePointLight("Relay_Blue_Light", new Vector3(0f, 3.2f, 15f), new Color(0.25f, 0.65f, 1f), 2.8f, 8f);
            CreatePointLight("Generator_Warning_Light", new Vector3(0f, 3.2f, 3f), new Color(1f, 0.12f, 0.08f), 1.9f, 7f);

            CreateSpotLight("Defense_Left_Searchlight", new Vector3(0.5f, 3.4f, 10.5f), Quaternion.Euler(68f, 165f, 0f), new Color(1f, 0.86f, 0.62f), 2.5f, 12f);
            CreateSpotLight("Defense_Right_Searchlight", new Vector3(3.5f, 3.4f, 10.5f), Quaternion.Euler(68f, 195f, 0f), new Color(1f, 0.86f, 0.62f), 2.5f, 12f);
            CreateSpotLight("Stage_Old_Spotlight", new Vector3(6.2f, 2.7f, -4f), Quaternion.Euler(62f, 30f, 0f), new Color(1f, 0.7f, 0.35f), 2.2f, 10f);

            _ = materials;
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
            cameraObject.transform.position = new Vector3(-8f, 10f, -8f);
            cameraObject.transform.rotation = Quaternion.Euler(60f, 45f, 0f);

            var camera = cameraObject.AddComponent<UnityEngine.Camera>();
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 200f;

            cameraObject.AddComponent<AudioListener>();
            return cameraObject.AddComponent<IsometricCameraController>();
        }

        private static List<PlayerCharacter> CreatePlaceholderParty(Transform parent, MinimalMaterials materials)
        {
            return new List<PlayerCharacter>
            {
                CreatePlaceholderCharacter(
                    parent,
                    EnsureCharacterDataAsset("SO_Character_Dax", "dax", "Dax", "Scavenger", 100, 50),
                    new Vector3(-2f, 0f, 0f),
                    materials.WarmLightSource,
                    materials),
                CreatePlaceholderCharacter(
                    parent,
                    EnsureCharacterDataAsset("SO_Character_Nyra", "nyra", "Nyra", "Tech", 85, 80),
                    new Vector3(0f, 0f, 0f),
                    materials.TechBlue,
                    materials),
                CreatePlaceholderCharacter(
                    parent,
                    EnsureCharacterDataAsset("SO_Character_Cormac", "cormac", "Cormac", "Medic", 95, 65),
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

            playerCharacter.SetSelectionIndicator(selectionRing);
            return playerCharacter;
        }

        private static void CreateQuestInteractables(Transform parent, MinimalMaterials materials)
        {
            CreateBreakerModule(parent, "BreakerModule_A", new Vector3(-5f, 0.25f, 4f), materials);
            CreateBreakerModule(parent, "BreakerModule_B", new Vector3(5f, 0.25f, 4f), materials);

            CreateInspectableSystem(
                parent,
                "Filter Station",
                PowerSystemType.WaterFilters,
                "Water Filters inspected.",
                new Vector3(-8f, 0.5f, 8f),
                materials.TechBlue);
            CreateInspectableSystem(
                parent,
                "Hydroponic Farm",
                PowerSystemType.HydroponicFarm,
                "Hydroponic Farm inspected.",
                new Vector3(-4f, 0.5f, 8f),
                materials.DeadVegetation);
            CreateInspectableSystem(
                parent,
                "Defense Gate",
                PowerSystemType.DefenseGate,
                "Defense Gate inspected.",
                new Vector3(0f, 0.5f, 8f),
                materials.DangerRed);
            CreateInspectableSystem(
                parent,
                "Public Stage",
                PowerSystemType.PublicStage,
                "Public Stage inspected.",
                new Vector3(4f, 0.5f, 8f),
                materials.WarmLightSource);
            CreateInspectableSystem(
                parent,
                "Workshop",
                PowerSystemType.Workshop,
                "Workshop inspected.",
                new Vector3(8f, 0.5f, 8f),
                materials.RustMetal);
            CreateInspectableSystem(
                parent,
                "Relay Station",
                PowerSystemType.RelayStation,
                "Relay Station inspected.",
                new Vector3(0f, 0.5f, 12f),
                materials.TechBlue);

            CreateSwitchRoomConsole(parent, new Vector3(0f, 0.6f, -8f), materials);
        }

        private static void CreateBreakerModule(Transform parent, string name, Vector3 position, MinimalMaterials materials)
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
            pickup.Configure("Pick Up Breaker Module", "[Click]", 4.5f);
        }

        private static void CreateInspectableSystem(
            Transform parent,
            string displayName,
            PowerSystemType systemType,
            string inspectionMessage,
            Vector3 position,
            Material material)
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
            inspectable.Configure($"Inspect {displayName}", "[Click]", 5f);
            inspectable.ConfigurePowerSystem(systemType, inspectionMessage);
        }

        private static void CreateSwitchRoomConsole(Transform parent, Vector3 position, MinimalMaterials materials)
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
            console.Configure("Use Switch Room Console", "[Click]", 5f);
        }

        private static void CreatePlaceholderNpcs(Transform parent, Dictionary<string, DialogueData> dialogueAssets, MinimalMaterials materials)
        {
            CreateNpc(parent, "Mara", "Talk to Mara", dialogueAssets["D01_MARA_INTRO"], new Vector3(0f, 1f, -6f), materials.DirtyPlastic);
            CreateNpc(parent, "Hale", "Talk to Hale", dialogueAssets["D02_HALE_DEFENSE"], new Vector3(2f, 1f, 7f), materials.DangerRed);
            CreateNpc(parent, "Ivo", "Talk to Ivo", dialogueAssets["D04_IVO_FARM"], new Vector3(-4.4f, 1f, 6f), materials.DeadVegetation);
            CreateNpc(parent, "Sela", "Talk to Sela", dialogueAssets["D03_SELA_WATER"], new Vector3(-10f, 1f, 6f), materials.TechBlue);
            CreateNpc(parent, "Edda", "Talk to Edda", dialogueAssets["D05_EDDA_STAGE"], new Vector3(8f, 1f, -4.2f), materials.WarmLightSource);
            CreateNpc(parent, "Greer", "Talk to Greer", dialogueAssets["D06_GREER_WORKSHOP"], new Vector3(8f, 1f, 3f), materials.RustMetal);
            CreateNpc(parent, "Lysa", "Talk to Lysa", dialogueAssets["D07_LYSA_RELAY"], new Vector3(0f, 1f, 13.3f), materials.TechBlue);
            CreateNpc(parent, "PartyReserveDiscussion", "Discuss Party Reserve", dialogueAssets["D08_PARTY_RESERVE"], new Vector3(2.8f, 1f, -6f), materials.ColdConcrete);
        }

        private static void CreateNpc(Transform parent, string objectName, string prompt, DialogueData dialogueData, Vector3 position, Material material)
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
            npcDialogue.Configure(prompt, "[Click]", 5f);
            npcDialogue.SetDialogue(dialogueData);
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
                Id = "resolve_power_priorities",
                Description = "Resolve Riverside's Power Priorities",
                Status = ObjectiveStatus.Hidden
            });
            questData.Objectives.Add(new ObjectiveData
            {
                Id = "inspect_key_systems",
                Description = "Inspect Key Systems: 0/6",
                Status = ObjectiveStatus.Hidden
            });
            questData.Objectives.Add(new ObjectiveData
            {
                Id = "find_breaker_modules",
                Description = "Find 2 Breaker Modules: 0/2",
                Status = ObjectiveStatus.Hidden
            });
            questData.Objectives.Add(new ObjectiveData
            {
                Id = "repair_generator_line",
                Description = "Repair Generator Line",
                Status = ObjectiveStatus.Hidden
            });
            questData.Objectives.Add(new ObjectiveData
            {
                Id = "allocate_power",
                Description = "Allocate Power",
                Status = ObjectiveStatus.Hidden
            });

            EditorUtility.SetDirty(questData);
            return questData;
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
                        StartQuest(questData),
                        ActivateObjective("resolve_power_priorities")
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
