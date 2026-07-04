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
            var dialogueAssets = EnsureDialogueAssets(questData);

            var dialoguePanel = CreatePlaceholderUI(uiRoot.transform, questManager);
            CreateQuestInteractables(interactablesRoot.transform);
            CreatePlaceholderNpcs(npcsRoot.transform, dialogueAssets);

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

        private static void CreateQuestInteractables(Transform parent)
        {
            CreateBreakerModule(parent, "BreakerModule_A", new Vector3(-5f, 0.25f, 4f));
            CreateBreakerModule(parent, "BreakerModule_B", new Vector3(5f, 0.25f, 4f));

            CreateInspectableSystem(
                parent,
                "Filter Station",
                PowerSystemType.WaterFilters,
                "Water Filters inspected.",
                new Vector3(-8f, 0.5f, 8f),
                new Color(0.2f, 0.55f, 0.95f));
            CreateInspectableSystem(
                parent,
                "Hydroponic Farm",
                PowerSystemType.HydroponicFarm,
                "Hydroponic Farm inspected.",
                new Vector3(-4f, 0.5f, 8f),
                new Color(0.28f, 0.72f, 0.3f));
            CreateInspectableSystem(
                parent,
                "Defense Gate",
                PowerSystemType.DefenseGate,
                "Defense Gate inspected.",
                new Vector3(0f, 0.5f, 8f),
                new Color(0.65f, 0.65f, 0.7f));
            CreateInspectableSystem(
                parent,
                "Public Stage",
                PowerSystemType.PublicStage,
                "Public Stage inspected.",
                new Vector3(4f, 0.5f, 8f),
                new Color(0.85f, 0.55f, 0.25f));
            CreateInspectableSystem(
                parent,
                "Workshop",
                PowerSystemType.Workshop,
                "Workshop inspected.",
                new Vector3(8f, 0.5f, 8f),
                new Color(0.55f, 0.42f, 0.32f));
            CreateInspectableSystem(
                parent,
                "Relay Station",
                PowerSystemType.RelayStation,
                "Relay Station inspected.",
                new Vector3(0f, 0.5f, 12f),
                new Color(0.55f, 0.45f, 0.9f));

            CreateSwitchRoomConsole(parent, new Vector3(0f, 0.6f, -8f));
        }

        private static void CreateBreakerModule(Transform parent, string name, Vector3 position)
        {
            var moduleObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            moduleObject.name = name;
            moduleObject.transform.SetParent(parent);
            moduleObject.transform.position = position;
            moduleObject.transform.localScale = new Vector3(0.8f, 0.35f, 0.8f);

            var renderer = moduleObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = CreateRuntimeMaterial($"MAT_Runtime_{name}", new Color(0.9f, 0.75f, 0.2f));
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
            Color color)
        {
            var systemObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            systemObject.name = displayName;
            systemObject.transform.SetParent(parent);
            systemObject.transform.position = position;
            systemObject.transform.localScale = new Vector3(1.4f, 1f, 1.4f);

            var renderer = systemObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = CreateRuntimeMaterial($"MAT_Runtime_{displayName.Replace(" ", string.Empty)}", color);
            }

            var inspectable = systemObject.AddComponent<InspectablePowerSystem>();
            inspectable.Configure($"Inspect {displayName}", "[Click]", 5f);
            inspectable.ConfigurePowerSystem(systemType, inspectionMessage);
        }

        private static void CreateSwitchRoomConsole(Transform parent, Vector3 position)
        {
            var consoleObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            consoleObject.name = "SwitchRoomConsole";
            consoleObject.transform.SetParent(parent);
            consoleObject.transform.position = position;
            consoleObject.transform.localScale = new Vector3(1.8f, 1.2f, 0.8f);

            var renderer = consoleObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = CreateRuntimeMaterial("MAT_Runtime_SwitchRoomConsole", new Color(0.25f, 0.5f, 0.65f));
            }

            var console = consoleObject.AddComponent<SwitchRoomConsole>();
            console.Configure("Use Switch Room Console", "[Click]", 5f);
        }

        private static void CreatePlaceholderNpcs(Transform parent, Dictionary<string, DialogueData> dialogueAssets)
        {
            CreateNpc(parent, "Mara", "Talk to Mara", dialogueAssets["D01_MARA_INTRO"], new Vector3(-8f, 1f, -4f), new Color(0.7f, 0.65f, 0.55f));
            CreateNpc(parent, "Hale", "Talk to Hale", dialogueAssets["D02_HALE_DEFENSE"], new Vector3(-6f, 1f, -4f), new Color(0.5f, 0.55f, 0.62f));
            CreateNpc(parent, "Ivo", "Talk to Ivo", dialogueAssets["D04_IVO_FARM"], new Vector3(-4f, 1f, -4f), new Color(0.35f, 0.7f, 0.35f));
            CreateNpc(parent, "Sela", "Talk to Sela", dialogueAssets["D03_SELA_WATER"], new Vector3(-2f, 1f, -4f), new Color(0.35f, 0.65f, 0.8f));
            CreateNpc(parent, "Edda", "Talk to Edda", dialogueAssets["D05_EDDA_STAGE"], new Vector3(0f, 1f, -4f), new Color(0.85f, 0.55f, 0.35f));
            CreateNpc(parent, "Greer", "Talk to Greer", dialogueAssets["D06_GREER_WORKSHOP"], new Vector3(2f, 1f, -4f), new Color(0.55f, 0.42f, 0.3f));
            CreateNpc(parent, "Lysa", "Talk to Lysa", dialogueAssets["D07_LYSA_RELAY"], new Vector3(4f, 1f, -4f), new Color(0.55f, 0.45f, 0.9f));
            CreateNpc(parent, "PartyReserveDiscussion", "Discuss Party Reserve", dialogueAssets["D08_PARTY_RESERVE"], new Vector3(6f, 1f, -4f), new Color(0.25f, 0.7f, 0.75f));
        }

        private static void CreateNpc(Transform parent, string objectName, string prompt, DialogueData dialogueData, Vector3 position, Color color)
        {
            var npcObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            npcObject.name = objectName;
            npcObject.transform.SetParent(parent);
            npcObject.transform.position = position;

            var renderer = npcObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = CreateRuntimeMaterial($"MAT_Runtime_NPC_{objectName}", color);
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
