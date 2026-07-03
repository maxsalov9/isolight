# IsoLight — Codex MVP Backlog v0.1

**Игра:** IsoLight  
**Документ:** Codex MVP Backlog  
**Версия:** 0.1  
**Цель документа:** разбить Unity-прототип IsoLight на маленькие реалистичные задачи для Codex.

---

# 1. Как использовать этот backlog

Этот документ нужен, чтобы Codex работал пошагово, а не пытался сделать всю игру сразу.

Правильный подход:

1. Дать Codex контекст проекта.
2. Дать ему TechSpec.
3. Дать одну задачу из backlog.
4. Проверить результат.
5. Зафиксировать рабочее состояние.
6. Перейти к следующей задаче.

---

# 2. Общие правила для Codex

## 2.1. Делать

- Делать маленькие рабочие шаги.
- Сначала делать placeholder-версию.
- Писать читаемый код.
- Делить системы на понятные компоненты.
- Использовать простые ScriptableObjects для данных.
- Держать все игровые скрипты в namespace `IsoLight`.
- После каждой задачи проверять, что проект компилируется.
- Не ломать уже работающие системы.

## 2.2. Не делать

- Не делать полный open world.
- Не делать multiplayer.
- Не делать сложную экономику.
- Не делать сложный inventory.
- Не делать procedural generation.
- Не делать финальную графику.
- Не делать сложную RPG-прокачку.
- Не делать сложную симуляцию электросети.
- Не переписывать архитектуру без причины.
- Не добавлять внешние платные ассеты без запроса.

---

# 3. Приоритеты

## P0 — обязательно для первого playable prototype

Без этих задач прототип не считается играбельным.

## P1 — желательно для vertical slice

Улучшают ощущение игры и делают миссию ближе к финальной.

## P2 — polish / later

Можно отложить.

---

# 4. Roadmap по фазам

| Фаза | Название | Цель |
|---|---|---|
| Phase 0 | Project Setup | создать структуру проекта |
| Phase 1 | Exploration Core | камера, перемещение, отряд |
| Phase 2 | Interaction & Quest | интерактивные объекты и квесты |
| Phase 3 | Dialogue | базовая система диалогов |
| Phase 4 | Power Politics | приоритеты энергии и отношения |
| Phase 5 | Combat | бой у генератора |
| Phase 6 | Mission Flow | собрать миссию целиком |
| Phase 7 | UI/Feedback | сделать прототип читаемым |
| Phase 8 | Debug & Polish | инструменты тестирования и правки |

---

# 5. Phase 0 — Project Setup

---

## TASK 0.1 — Create Unity Project Structure

**Priority:** P0  
**Goal:** создать базовую структуру папок проекта.

### Create folders

```text
Assets/
  IsoLight/
    Art/
      Characters/
      Environment/
      Props/
      UI/
      VFX/
    Audio/
      Music/
      SFX/
      VO/
    Data/
      Characters/
      NPCs/
      Dialogues/
      Quests/
      PowerSystems/
      Enemies/
      Items/
    Materials/
    Prefabs/
      Characters/
      Enemies/
      NPCs/
      Interactables/
      UI/
      Environment/
    Scenes/
      Prototype/
    Scripts/
      Core/
      Camera/
      Characters/
      Party/
      Input/
      Interaction/
      Dialogue/
      Quests/
      Combat/
      Enemies/
      Power/
      Relationships/
      UI/
      Save/
      Utilities/
    Settings/
```

### Acceptance Criteria

- папки созданы;
- структура соответствует TechSpec;
- нет лишних экспериментальных папок;
- проект компилируется.

---

## TASK 0.2 — Create Main Prototype Scene

**Priority:** P0  
**Goal:** создать сцену `Riverside_Prototype`.

### Files

```text
Assets/IsoLight/Scenes/Prototype/Riverside_Prototype.unity
```

### Scene objects

```text
_Game
  GameManager
  InputManager
  CameraManager
  PartyManager
  QuestManager
  DialogueManager
  CombatManager
  PowerManager
  RelationshipManager
  SaveManager
  UIManager

_Level
_UI
_PlayerParty
_NPCs
_Enemies
_Interactables
```

### Acceptance Criteria

- сцена создана;
- все основные root objects присутствуют;
- сцена открывается без ошибок;
- проект компилируется.

---

## TASK 0.3 — Create Core Enums and Runtime State

**Priority:** P0  
**Goal:** создать базовые enum и runtime state.

### Create files

```text
Assets/IsoLight/Scripts/Core/GameMode.cs
Assets/IsoLight/Scripts/Core/MissionState.cs
Assets/IsoLight/Scripts/Power/PowerChoice.cs
Assets/IsoLight/Scripts/Relationships/RelationshipLevel.cs
Assets/IsoLight/Scripts/Relationships/RelationshipState.cs
```

### Required enums

```csharp
GameMode
PowerChoice
RelationshipLevel
```

### Required classes

```csharp
MissionState
RelationshipState
```

### Acceptance Criteria

- все классы и enum созданы;
- namespace используется корректно;
- проект компилируется;
- GameManager может хранить MissionState.

---

## TASK 0.4 — Create Basic Managers

**Priority:** P0  
**Goal:** создать пустые manager-компоненты.

### Create files

```text
GameManager.cs
InputManager.cs
CameraManager.cs
PartyManager.cs
QuestManager.cs
DialogueManager.cs
CombatManager.cs
PowerManager.cs
RelationshipManager.cs
SaveManager.cs
UIManager.cs
```

### Acceptance Criteria

- каждый manager существует как MonoBehaviour;
- manager-компоненты можно добавить на GameObject;
- GameManager хранит текущий GameMode;
- нет runtime errors.

---

# 6. Phase 1 — Exploration Core

---

## TASK 1.1 — Isometric Camera Controller

**Priority:** P0  
**Goal:** сделать изометрическую камеру с follow и zoom.

### Create files

```text
Assets/IsoLight/Scripts/Camera/IsometricCameraController.cs
```

### Features

- follow target;
- zoom with mouse wheel;
- optional WASD pan;
- smooth movement;
- min/max zoom.

### Acceptance Criteria

- камера смотрит на сцену сверху под углом;
- камера может следовать за персонажем;
- колесо мыши меняет zoom;
- движение плавное;
- нет рывков и ошибок.

---

## TASK 1.2 — Create Player Character Data

**Priority:** P0  
**Goal:** создать `CharacterData` ScriptableObject.

### Create files

```text
Assets/IsoLight/Scripts/Characters/CharacterData.cs
```

### Fields

```csharp
Id
DisplayName
Role
Portrait
MaxHealth
MaxEnergy
Abilities
```

### Create assets

```text
SO_Character_Dax
SO_Character_Nyra
SO_Character_Cormac
```

### Acceptance Criteria

- CharacterData создается через Unity Create Asset menu;
- assets для Dax/Nyra/Cormac созданы;
- данные можно редактировать в Inspector;
- проект компилируется.

---

## TASK 1.3 — Create PlayerCharacter Component

**Priority:** P0  
**Goal:** сделать компонент игрокового персонажа.

### Create files

```text
Assets/IsoLight/Scripts/Characters/PlayerCharacter.cs
```

### Features

- ссылка на CharacterData;
- CurrentHealth;
- CurrentEnergy;
- selection state;
- receive damage;
- heal;
- basic events for UI.

### Acceptance Criteria

- PlayerCharacter можно повесить на prefab;
- HP и Energy инициализируются из CharacterData;
- есть методы TakeDamage и Heal;
- проект компилируется.

---

## TASK 1.4 — Point-and-Click Movement

**Priority:** P0  
**Goal:** сделать перемещение активного персонажа по клику.

### Create files

```text
Assets/IsoLight/Scripts/Input/ClickToMoveController.cs
```

### Requirements

- использовать NavMeshAgent;
- левый клик по земле отправляет активного персонажа в точку;
- клик игнорируется во время Dialogue / PowerAllocation / Paused.

### Acceptance Criteria

- активный персонаж идет к месту клика;
- персонаж не проходит сквозь препятствия;
- движение работает только в Exploration mode;
- нет ошибок при клике в UI.

---

## TASK 1.5 — Party Selection

**Priority:** P0  
**Goal:** сделать выбор активного персонажа.

### Update files

```text
PartyManager.cs
PlayerCharacter.cs
```

### Features

- хранить список персонажей;
- выбрать Dax/Nyra/Cormac;
- клавиши 1/2/3 выбирают персонажа;
- selected character имеет selection ring.

### Acceptance Criteria

- можно переключать персонажей клавишами;
- активный персонаж подсвечивается;
- клик движения применяется к активному персонажу;
- UI может получить active character.

---

## TASK 1.6 — Party Follow

**Priority:** P1  
**Goal:** сделать, чтобы неактивные персонажи следовали за активным.

### Create files

```text
Assets/IsoLight/Scripts/Party/PartyFollowController.cs
```

### Acceptance Criteria

- неактивные персонажи следуют за активным;
- они держат небольшую дистанцию;
- не мешают активному персонажу;
- можно временно отключить follow в combat mode.

---

# 7. Phase 2 — Interaction & Quest

---

## TASK 2.1 — Interactable Interface

**Priority:** P0  
**Goal:** создать базовый интерфейс интерактивных объектов.

### Create files

```text
Assets/IsoLight/Scripts/Interaction/IInteractable.cs
Assets/IsoLight/Scripts/Interaction/InteractableObject.cs
```

### Features

- InteractionName;
- CanInteract;
- Interact;
- interaction distance;
- highlight on hover;
- prompt text.

### Acceptance Criteria

- любой объект может стать interactable;
- при наведении появляется highlight;
- при клике вызывается Interact;
- interaction respects distance.

---

## TASK 2.2 — Interaction Prompt UI

**Priority:** P0  
**Goal:** показать подсказку взаимодействия.

### Create files

```text
Assets/IsoLight/Scripts/UI/InteractionPromptUI.cs
```

### UI

```text
[E] Inspect Generator
[Click] Talk to Mara
```

### Acceptance Criteria

- prompt появляется при наведении;
- prompt скрывается, когда игрок уводит мышь;
- prompt не появляется в Dialogue / Combat / PowerAllocation;
- текст берется из InteractionName.

---

## TASK 2.3 — Quest Data Structures

**Priority:** P0  
**Goal:** создать базовые структуры квеста.

### Create files

```text
Assets/IsoLight/Scripts/Quests/QuestData.cs
Assets/IsoLight/Scripts/Quests/ObjectiveData.cs
Assets/IsoLight/Scripts/Quests/ObjectiveStatus.cs
```

### Acceptance Criteria

- QuestData создается как ScriptableObject;
- можно задать список objectives;
- ObjectiveStatus работает;
- проект компилируется.

---

## TASK 2.4 — Quest Manager and Objective Panel

**Priority:** P0  
**Goal:** сделать запуск и обновление objective.

### Create / update files

```text
QuestManager.cs
Assets/IsoLight/Scripts/UI/ObjectivePanelUI.cs
```

### Features

- StartQuest;
- ActivateObjective;
- CompleteObjective;
- update UI.

### Acceptance Criteria

- на экране видна текущая цель;
- цель меняется через QuestManager;
- можно завершить objective;
- нет ошибок при отсутствии активного квеста.

---

## TASK 2.5 — Breaker Module Pickup

**Priority:** P0  
**Goal:** сделать pickup-модули для генератора.

### Create files

```text
Assets/IsoLight/Scripts/Interaction/BreakerModulePickup.cs
```

### Requirements

- при interact увеличивать `BreakerModulesCollected`;
- после подбора скрывать объект;
- обновлять objective.

### Acceptance Criteria

- игрок может подобрать модуль;
- счетчик меняется 0/2 → 1/2 → 2/2;
- после 2 модулей открывается objective Repair Generator Line.

---

## TASK 2.6 — Inspectable Power Systems

**Priority:** P0  
**Goal:** сделать объекты осмотра для систем поселения.

### Create files

```text
Assets/IsoLight/Scripts/Interaction/InspectablePowerSystem.cs
```

### Objects

- Filter Station;
- Hydroponic Room;
- Defense Gate;
- Public Stage;
- Workshop;
- Relay Station.

### Acceptance Criteria

- каждый объект можно inspect;
- после inspect ставится соответствующий flag в MissionState;
- objective Inspect Key Systems обновляется;
- можно показать short description panel.

---

# 8. Phase 3 — Dialogue

---

## TASK 3.1 — Dialogue Data Structures

**Priority:** P0  
**Goal:** создать структуры диалогов.

### Create files

```text
Assets/IsoLight/Scripts/Dialogue/DialogueData.cs
Assets/IsoLight/Scripts/Dialogue/DialogueNode.cs
Assets/IsoLight/Scripts/Dialogue/DialogueChoice.cs
Assets/IsoLight/Scripts/Dialogue/DialogueEffect.cs
Assets/IsoLight/Scripts/Dialogue/DialogueCondition.cs
```

### Acceptance Criteria

- DialogueData создается как ScriptableObject;
- DialogueNode хранит текст и варианты;
- DialogueChoice ведет к NextNodeId;
- проект компилируется.

---

## TASK 3.2 — Dialogue Manager

**Priority:** P0  
**Goal:** реализовать проигрывание диалогов.

### Update files

```text
DialogueManager.cs
```

### Features

- StartDialogue(DialogueData);
- Show current node;
- Show choices;
- Process choice;
- Apply effects;
- Close dialogue;
- set GameMode Dialogue / Exploration.

### Acceptance Criteria

- можно открыть диалог с NPC;
- варианты ответа кликабельны;
- диалог переходит между nodes;
- диалог закрывается;
- во время диалога персонаж не двигается.

---

## TASK 3.3 — Dialogue UI

**Priority:** P0  
**Goal:** создать UI для диалога.

### Create files

```text
Assets/IsoLight/Scripts/UI/DialoguePanelUI.cs
```

### UI elements

- speaker name;
- dialogue text;
- choice buttons;
- optional companion comment area.

### Acceptance Criteria

- текст отображается;
- варианты создаются динамически;
- кнопки вызывают выбор;
- UI скрывается после закрытия диалога.

---

## TASK 3.4 — NPC Interaction

**Priority:** P0  
**Goal:** NPC запускают привязанные диалоги.

### Create files

```text
Assets/IsoLight/Scripts/Dialogue/NPCDialogueInteractable.cs
```

### NPCs

- Mara;
- Hale;
- Ivo;
- Sela;
- Edda;
- Greer;
- Lysa.

### Acceptance Criteria

- клик по NPC открывает диалог;
- у каждого NPC может быть свой DialogueData;
- после важного диалога ставится flag `TalkedToX`.

---

## TASK 3.5 — Add Placeholder Dialogues

**Priority:** P0  
**Goal:** добавить черновые диалоги для NPC.

### Required dialogues

- Mara intro;
- Hale priority;
- Ivo priority;
- Sela priority;
- Edda priority;
- Greer priority;
- Lysa priority;
- Party Reserve discussion.

### Acceptance Criteria

- каждый NPC имеет минимум 1 рабочий диалог;
- диалоги ставят нужные mission flags;
- после Mara запускается main quest;
- после NPC priority dialogues обновляется objective.

---

# 9. Phase 4 — Power Politics

---

## TASK 4.1 — PowerSystemData

**Priority:** P0  
**Goal:** создать данные энергетических систем.

### Create files

```text
Assets/IsoLight/Scripts/Power/PowerSystemData.cs
```

### Fields

```csharp
Id
DisplayName
Description
RequiredPower
BenefitDescription
DownsideDescription
SupportingNPCId
OpposingNPCIds
PowerChoice
```

### Create assets

- Water Filters;
- Hydroponic Farm;
- Defense Grid;
- Public Stage;
- Workshop;
- Relay Station;
- Party Reserve;
- Split Load.

### Acceptance Criteria

- assets создаются и редактируются;
- PowerManager может получить список систем;
- RequiredPower отображается в Inspector.

---

## TASK 4.2 — Relationship Manager

**Priority:** P0  
**Goal:** реализовать простые отношения с группами.

### Update files

```text
RelationshipManager.cs
RelationshipState.cs
```

### Relationship groups

- RiversideTrust;
- DefenseSupport;
- FarmSupport;
- MedicalSupport;
- StageSupport;
- WorkshopSupport;
- RelaySupport.

### Acceptance Criteria

- можно менять RelationshipLevel;
- можно получить текущее отношение;
- выбор энергии меняет отношения;
- состояние можно показать в Result Panel.

---

## TASK 4.3 — Power Manager

**Priority:** P0  
**Goal:** реализовать выбор энергетического приоритета.

### Update files

```text
PowerManager.cs
```

### Features

- list of PowerSystemData;
- AvailableStableOutput = 100;
- ApplyPowerChoice(PowerChoice);
- update MissionState.FinalPowerChoice;
- update RelationshipState;
- trigger visual feedback.

### Acceptance Criteria

- выбор сохраняется;
- отношения меняются;
- миссия получает final choice;
- можно вызвать Result Panel.

---

## TASK 4.4 — Power Allocation Board UI

**Priority:** P0  
**Goal:** сделать финальный экран распределения энергии.

### Create files

```text
Assets/IsoLight/Scripts/UI/PowerAllocationBoardUI.cs
Assets/IsoLight/Scripts/UI/PowerSystemOptionUI.cs
```

### UI

- list of power systems;
- display required power;
- display benefit/downside;
- select button;
- confirm button;
- close disabled until after choice or explicit back allowed.

### Acceptance Criteria

- открывается из Switch Room Console;
- показывает все системы;
- выбор работает;
- confirm вызывает PowerManager;
- после confirm открывается Result Panel.

---

## TASK 4.5 — Result Panel UI

**Priority:** P0  
**Goal:** показать последствия выбора.

### Create files

```text
Assets/IsoLight/Scripts/UI/ResultPanelUI.cs
```

### Must show

- chosen power system;
- short consequence;
- relationship changes;
- mission complete message.

### Acceptance Criteria

- после выбора появляется Result Panel;
- текст соответствует выбранному варианту;
- кнопка Continue завершает миссию;
- GameMode возвращается в Exploration или MissionComplete.

---

## TASK 4.6 — Party Reserve Logic

**Priority:** P1  
**Goal:** добавить логику Party Reserve.

### Data

```csharp
int PartyPowerCellsCharge;
bool PartySelfishnessSeen;
```

### Effects

- PartyPowerCellsCharge increases;
- RiversideTrust decreases;
- Cormac approval placeholder decreases;
- unlock future terminal flag optional.

### Acceptance Criteria

- выбор PartyReserve заряжает power cells;
- Result Panel объясняет моральную цену;
- RelationshipState обновляется.

---

# 10. Phase 5 — Combat

---

## TASK 5.1 — Damageable Interface

**Priority:** P0  
**Goal:** создать единый интерфейс для объектов, получающих урон.

### Create files

```text
Assets/IsoLight/Scripts/Combat/IDamageable.cs
```

### Methods

```csharp
TakeDamage(int amount)
IsAlive
```

### Acceptance Criteria

- PlayerCharacter implements IDamageable;
- Enemy implements IDamageable;
- Generator implements IDamageable;
- проект компилируется.

---

## TASK 5.2 — Enemy Data and Enemy Component

**Priority:** P0  
**Goal:** создать enemy data и базовый enemy.

### Create files

```text
Assets/IsoLight/Scripts/Enemies/EnemyData.cs
Assets/IsoLight/Scripts/Enemies/Enemy.cs
Assets/IsoLight/Scripts/Enemies/EnemyRole.cs
```

### Acceptance Criteria

- EnemyData создается как ScriptableObject;
- Enemy инициализируется из EnemyData;
- Enemy имеет HP;
- Enemy может получать урон и умирать.

---

## TASK 5.3 — Basic Enemy AI

**Priority:** P0  
**Goal:** реализовать простую state machine врага.

### Create files

```text
Assets/IsoLight/Scripts/Enemies/EnemyAI.cs
Assets/IsoLight/Scripts/Enemies/EnemyState.cs
```

### States

- Idle;
- MoveToCover;
- AttackPlayer;
- AttackGenerator;
- SabotageGenerator;
- Dead.

### Acceptance Criteria

- enemy находит ближайшую цель;
- enemy двигается к атакуемой цели;
- enemy атакует через cooldown;
- Saboteur пытается идти к генератору;
- enemy умирает при HP <= 0.

---

## TASK 5.4 — Basic Player Attack

**Priority:** P0  
**Goal:** дать игроку возможность атаковать врагов.

### Features

- click enemy to attack;
- selected character attacks target if in range;
- damage applied;
- basic cooldown.

### Acceptance Criteria

- выбранный персонаж может атаковать врага;
- HP врага уменьшается;
- враг умирает;
- UI health bar обновляется.

---

## TASK 5.5 — Combat Manager

**Priority:** P0  
**Goal:** управлять боевой сценой.

### Update files

```text
CombatManager.cs
```

### Features

- StartCombat();
- spawn enemies;
- track enemies;
- track generator;
- end combat on victory;
- fail if generator destroyed.

### Acceptance Criteria

- бой стартует после запуска генератора;
- враги появляются на spawn points;
- бой завершается после смерти врагов;
- QuestManager получает событие GeneratorDefended.

---

## TASK 5.6 — Generator Object

**Priority:** P0  
**Goal:** реализовать Generator G-17.

### Create files

```text
Assets/IsoLight/Scripts/Power/GeneratorG17.cs
```

### Features

- repaired state;
- started state;
- current health;
- repair interaction;
- start interaction;
- damageable during combat.

### Acceptance Criteria

- нельзя стартовать без 2 breaker modules;
- после repair можно start;
- start вызывает combat;
- generator health visible in combat;
- generator can be damaged.

---

## TASK 5.7 — Basic Ability System

**Priority:** P1  
**Goal:** добавить простые способности персонажей.

### Create files

```text
Assets/IsoLight/Scripts/Combat/AbilityData.cs
Assets/IsoLight/Scripts/Combat/AbilityTargetType.cs
Assets/IsoLight/Scripts/Combat/AbilityRunner.cs
```

### MVP abilities

- Dax: Repair Burst;
- Nyra: Shock Pulse;
- Cormac: Emergency Heal.

### Acceptance Criteria

- ability можно вызвать кнопкой;
- ability имеет cooldown;
- ability применяет эффект;
- UI показывает cooldown.

---

# 11. Phase 6 — Mission Flow

---

## TASK 6.1 — Connect Main Quest Flow

**Priority:** P0  
**Goal:** собрать mission flow от начала до финального выбора.

### Required flow

1. Reach Shelter;
2. Talk to Mara;
3. Talk to priority NPCs;
4. Inspect systems;
5. Collect breaker modules;
6. Repair generator;
7. Start generator;
8. Defend generator;
9. Open Power Allocation Board;
10. Choose power;
11. Show result.

### Acceptance Criteria

- миссию можно пройти от начала до конца;
- objectives обновляются в правильном порядке;
- финальный выбор становится доступен только после боя;
- Result Panel завершает миссию.

---

## TASK 6.2 — Add Switch Room Console

**Priority:** P0  
**Goal:** консоль открывает Power Allocation Board после боя.

### Create / update files

```text
SwitchRoomConsole.cs
```

### Acceptance Criteria

- до боя консоль недоступна;
- после победы консоль активна;
- interact открывает Power Allocation Board;
- objective Allocate Power active.

---

## TASK 6.3 — Visual Result States

**Priority:** P1  
**Goal:** сделать видимое изменение сцены после выбора.

### Visual changes

- Water Filters: blue/white lights;
- Farm: warm grow lights;
- Defense: floodlights/fence sparks;
- Stage: spotlight on stage;
- Workshop: workbench light;
- Relay: antenna blinking;
- PartyReserve: power cells glow;
- SplitLoad: several weak flickering lights.

### Acceptance Criteria

- после выбора включается соответствующий объект;
- другие объекты остаются выключенными или тусклыми;
- игрок визуально понимает последствие.

---

# 12. Phase 7 — UI / Feedback

---

## TASK 7.1 — Character HUD

**Priority:** P0  
**Goal:** показать состояние отряда.

### Create files

```text
Assets/IsoLight/Scripts/UI/CharacterHUDPanel.cs
Assets/IsoLight/Scripts/UI/PartyHUDUI.cs
```

### Must show

- portrait placeholder;
- name;
- role;
- HP bar;
- energy bar;
- selected indicator.

### Acceptance Criteria

- HUD показывает 3 персонажа;
- выбранный персонаж подсвечен;
- HP bar обновляется при уроне;
- персонаж с 0 HP отображается как downed.

---

## TASK 7.2 — Enemy Health Bars

**Priority:** P0  
**Goal:** показать HP врагов.

### Create files

```text
Assets/IsoLight/Scripts/UI/EnemyHealthBarUI.cs
```

### Acceptance Criteria

- health bar над врагом;
- уменьшается при уроне;
- скрывается при смерти.

---

## TASK 7.3 — Generator Status UI

**Priority:** P0  
**Goal:** показать состояние генератора в бою.

### Create files

```text
Assets/IsoLight/Scripts/UI/GeneratorStatusUI.cs
```

### Must show

- Generator HP;
- repaired/started state;
- warning when under attack.

### Acceptance Criteria

- UI появляется во время боя;
- HP генератора обновляется;
- warning появляется при атаке Saboteur.

---

## TASK 7.4 — Basic Minimap Placeholder

**Priority:** P2  
**Goal:** добавить простую миникарту или заглушку.

### Acceptance Criteria

- в UI есть minimap panel;
- можно показывать простые markers;
- если сложно, оставить static placeholder.

---

## TASK 7.5 — Notification Toasts

**Priority:** P1  
**Goal:** добавить небольшие уведомления.

### Examples

```text
Breaker Module collected 1/2
Water Filters inspected
Generator repaired
Riverside Trust decreased
```

### Acceptance Criteria

- toast появляется на 2–3 секунды;
- не ломает gameplay;
- вызывается из managers.

---

# 13. Phase 8 — Debug & Polish

---

## TASK 8.1 — Debug Panel

**Priority:** P1  
**Goal:** добавить debug panel для тестирования прототипа.

### Debug actions

- Collect all breaker modules;
- Start generator;
- Start combat;
- Win combat;
- Open Power Allocation Board;
- Reset mission state.

### Acceptance Criteria

- debug panel можно включить клавишей;
- debug actions работают;
- debug panel можно отключить в build settings later.

---

## TASK 8.2 — Basic Audio Hooks

**Priority:** P2  
**Goal:** добавить точки вызова звуков.

### Events

- click;
- interact;
- dialogue open;
- generator start;
- combat start;
- gunshot;
- power allocated;
- result panel.

### Acceptance Criteria

- AudioManager существует;
- можно назначить AudioClip в Inspector;
- если клипов нет, ошибок нет.

---

## TASK 8.3 — Placeholder Materials and Readability

**Priority:** P1  
**Goal:** улучшить читаемость placeholder-сцены.

### Requirements

- интерактивные объекты заметны;
- путь игрока читается;
- боевой двор не перегружен;
- укрытия видны;
- NPC отличаются цветом или иконкой.

### Acceptance Criteria

- игрок понимает, куда идти;
- объекты легко найти;
- NPC не теряются в сцене.

---

# 14. Minimal Playable Build Checklist

Прототип считается минимально готовым, если:

## Exploration

- [ ] камера работает;
- [ ] персонажи двигаются;
- [ ] можно выбрать Dax/Nyra/Cormac;
- [ ] NPC интерактивны;
- [ ] объекты подсвечиваются.

## Quest

- [ ] квест начинается после Mara;
- [ ] objectives обновляются;
- [ ] breaker modules собираются;
- [ ] генератор чинится;
- [ ] генератор запускается.

## Dialogue

- [ ] Mara имеет рабочий диалог;
- [ ] Hale/Ivo/Sela/Edda имеют рабочие priority dialogues;
- [ ] диалоги ставят flags;
- [ ] UI диалогов работает.

## Combat

- [ ] после запуска генератора начинается бой;
- [ ] враги спавнятся;
- [ ] враги атакуют;
- [ ] игрок атакует;
- [ ] генератор можно повредить;
- [ ] бой завершается победой.

## Power Allocation

- [ ] после боя открывается Switch Room;
- [ ] Power Allocation Board показывает варианты;
- [ ] игрок выбирает вариант;
- [ ] выбор сохраняется;
- [ ] Result Panel показывает последствия.

## Completion

- [ ] миссию можно пройти от старта до конца;
- [ ] нет критических ошибок в консоли;
- [ ] результат выбора хранится в MissionState.

---

# 15. Recommended First Codex Task

Самая первая задача для Codex:

```text
Read the project documents:
- IsoLight_Unity_Prototype_TechSpec_v0.1.md
- IsoLight_Codex_MVP_Backlog_v0.1.md

Then implement TASK 0.1, TASK 0.2, TASK 0.3 and TASK 0.4 only.

Do not implement gameplay yet.
Only create the Unity folder structure, prototype scene root objects, core enums, runtime state classes and empty managers.

After implementation, report:
- created folders
- created scripts
- created scene objects
- any assumptions
- how to test that the project compiles
```

---

# 16. Suggested Task Batches for Codex

Чтобы Codex не перегружался, задачи лучше давать пакетами.

## Batch 1 — Project Skeleton

- TASK 0.1
- TASK 0.2
- TASK 0.3
- TASK 0.4

## Batch 2 — Movement

- TASK 1.1
- TASK 1.2
- TASK 1.3
- TASK 1.4
- TASK 1.5

## Batch 3 — Interaction

- TASK 2.1
- TASK 2.2
- TASK 2.3
- TASK 2.4

## Batch 4 — Quest Objects

- TASK 2.5
- TASK 2.6
- TASK 6.2

## Batch 5 — Dialogue

- TASK 3.1
- TASK 3.2
- TASK 3.3
- TASK 3.4
- TASK 3.5

## Batch 6 — Power Politics

- TASK 4.1
- TASK 4.2
- TASK 4.3
- TASK 4.4
- TASK 4.5
- TASK 4.6

## Batch 7 — Combat

- TASK 5.1
- TASK 5.2
- TASK 5.3
- TASK 5.4
- TASK 5.5
- TASK 5.6

## Batch 8 — Mission Flow

- TASK 6.1
- TASK 6.3
- TASK 7.1
- TASK 7.2
- TASK 7.3

## Batch 9 — Debug / Polish

- TASK 7.5
- TASK 8.1
- TASK 8.2
- TASK 8.3

---

# 17. Important Development Principle

The prototype should become playable as early as possible.

Preferred order:

```text
gray boxes > playable mission > readable UI > simple combat > consequences > better art
```

Not:

```text
perfect architecture > final art > full systems > maybe playable later
```

---

# 18. Final Note

Codex should always optimize for a working prototype.

If there is a choice between:

- a beautiful unfinished system;
- an ugly working system;

choose the ugly working system.

IsoLight first needs to prove that this loop is interesting:

> talk → investigate → repair → defend → allocate power → face consequences
