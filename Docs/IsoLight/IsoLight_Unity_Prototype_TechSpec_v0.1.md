# IsoLight — Unity Prototype TechSpec v0.1

**Игра:** IsoLight  
**Документ:** Unity Prototype Technical Specification  
**Версия:** 0.1  
**Цель документа:** описать техническую архитектуру первого Unity-прототипа, чтобы Codex мог начать разработку MVP/vertical slice по миссии **Riverside District**.

---

# 1. Цель прототипа

## 1.1. Что мы хотим доказать

Первый Unity-прототип должен доказать, что основной игровой цикл IsoLight работает:

> Игрок приходит в район → общается с NPC → узнает конфликт вокруг энергии → исследует локацию → чинит генератор → отбивает атаку → распределяет питание → видит последствия.

---

## 1.2. Что должно быть в первом playable prototype

Минимальная версия должна включать:

- изометрическую камеру;
- point-and-click перемещение;
- отряд из 3 персонажей;
- выбор активного персонажа;
- интерактивные объекты;
- простую квестовую систему;
- NPC и диалоги;
- генератор G-17;
- поиск breaker-модулей;
- бой с рейдерами;
- защиту генератора;
- Power Allocation Board;
- систему отношений;
- сохранение результата выбора в runtime state;
- placeholder-графику.

---

## 1.3. Что НЕ входит в первый прототип

На первом этапе НЕ делать:

- полноценную кампанию;
- большой открытый мир;
- сложный inventory system;
- полноценную RPG-прокачку;
- полноценный stealth;
- продвинутую симуляцию электросети;
- сложную экономику;
- продвинутую физику;
- multiplayer;
- кат-сцены;
- полную озвучку;
- финальный арт;
- финальные анимации.

---

# 2. Рекомендуемый движок и подход

## 2.1. Движок

**Unity 3D**

Рекомендуемый подход:

- 3D-сцена;
- изометрическая камера;
- low/mid-poly placeholder окружение;
- NavMesh для перемещения;
- ScriptableObjects для данных;
- простые MonoBehaviour-компоненты для механик;
- UI на uGUI или UI Toolkit.

Для первого прототипа проще использовать **uGUI**, потому что быстрее собрать HUD, диалоги и кнопки.

---

## 2.2. Визуальный стиль прототипа

На первом этапе арт может быть placeholder.

Важно:

- сцена должна быть читаемой;
- интерактивные объекты должны выделяться;
- зоны должны быть понятны;
- укрытия должны быть видны;
- объекты энергии должны иметь визуальный акцент.

Не нужно сразу делать красивый финальный Riverside.  
Нужно сделать функциональный blockout.

---

# 3. Project Structure

## 3.1. Рекомендуемая структура папок

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

---

## 3.2. Главная сцена

```text
Assets/IsoLight/Scenes/Prototype/Riverside_Prototype.unity
```

Эта сцена должна содержать:

- level blockout;
- NavMesh;
- player party;
- NPC;
- interactables;
- enemy spawn points;
- generator yard;
- switch room;
- UI canvas;
- managers.

---

# 4. Naming Conventions

## 4.1. C# namespace

```csharp
namespace IsoLight
```

Для подсистем:

```csharp
IsoLight.Core
IsoLight.Characters
IsoLight.Party
IsoLight.Interaction
IsoLight.Dialogue
IsoLight.Quests
IsoLight.Combat
IsoLight.Enemies
IsoLight.Power
IsoLight.Relationships
IsoLight.UI
IsoLight.Save
```

---

## 4.2. Префиксы файлов

```text
SO_       ScriptableObject assets
PF_       Prefabs
UI_       UI prefabs
MAT_      Materials
SFX_      Sound effects
MUS_      Music
```

Примеры:

```text
SO_Character_Dax.asset
SO_NPC_Mara.asset
SO_Quest_RestorePowerToRiverside.asset
PF_Player_Dax.prefab
PF_Generator_G17.prefab
UI_DialoguePanel.prefab
```

---

# 5. Scene Managers

В сцене должен быть объект:

```text
_Game
```

Внутри:

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
```

---

## 5.1. GameManager

### Назначение

Центральная точка состояния прототипа.

### Ответственность

- хранить глобальное состояние миссии;
- переключать режимы игры;
- давать доступ к другим менеджерам;
- запускать начальную сцену;
- отслеживать состояние прототипа.

### Game Modes

```csharp
public enum GameMode
{
    Exploration,
    Dialogue,
    Combat,
    PowerAllocation,
    Paused
}
```

---

## 5.2. InputManager

### Назначение

Обработка ввода игрока.

### Должен поддерживать

- левый клик по земле — движение;
- левый клик по интерактивному объекту — interact;
- левый клик по персонажу — выбрать персонажа;
- клавиши 1/2/3 — выбрать Dax/Nyra/Cormac;
- клавиши способностей;
- Esc — pause / close panel.

---

## 5.3. UIManager

### Назначение

Центральный менеджер UI.

### Должен управлять

- HUD;
- Dialogue Panel;
- Objective Panel;
- Power Allocation Board;
- Combat UI;
- Interaction Prompt;
- End Result Panel.

---

# 6. Core Game State

## 6.1. Mission State

Создать класс:

```csharp
MissionState
```

Пример данных:

```csharp
public class MissionState
{
    public bool HasMetMara;
    public bool HasStartedPowerPrioritiesQuest;

    public bool TalkedToHale;
    public bool TalkedToIvo;
    public bool TalkedToSela;
    public bool TalkedToEdda;
    public bool TalkedToGreer;
    public bool TalkedToLysa;

    public bool InspectedWaterFilters;
    public bool InspectedHydroponicFarm;
    public bool InspectedDefenseGate;
    public bool InspectedPublicStage;
    public bool InspectedWorkshop;
    public bool InspectedRelayStation;

    public int BreakerModulesCollected;
    public bool GeneratorRepaired;
    public bool GeneratorStarted;
    public bool GeneratorDefended;

    public PowerChoice FinalPowerChoice;
    public bool MissionCompleted;
}
```

---

## 6.2. PowerChoice enum

```csharp
public enum PowerChoice
{
    None,
    WaterFilters,
    HydroponicFarm,
    DefenseGrid,
    PublicStage,
    Workshop,
    RelayStation,
    PartyReserve,
    SplitLoad
}
```

---

# 7. Player Party System

## 7.1. Состав отряда

В прототипе отряд состоит из трех персонажей:

| ID | Name | Role |
|---|---|---|
| dax | Dax | Scavenger |
| nyra | Nyra | Tech |
| cormac | Cormac | Medic |

---

## 7.2. CharacterData

Создать ScriptableObject:

```csharp
CharacterData
```

Поля:

```csharp
public string Id;
public string DisplayName;
public string Role;
public Sprite Portrait;
public int MaxHealth;
public int MaxEnergy;
public List<AbilityData> Abilities;
```

---

## 7.3. PlayerCharacter component

Компонент на персонаже.

Ответственность:

- хранить ссылку на CharacterData;
- текущее здоровье;
- текущую энергию;
- movement;
- selection state;
- ability usage;
- receiving damage;
- healing.

---

## 7.4. PartyManager

Ответственность:

- хранить список персонажей;
- выбирать активного персонажа;
- отправлять активного персонажа к точке;
- заставлять остальных следовать;
- передавать данные в HUD;
- переключать режим Exploration / Combat.

---

## 7.5. Selection Rings

У каждого персонажа должен быть круг выбора.

Состояния:

- inactive;
- selected;
- in combat;
- downed.

На первом этапе можно сделать простые colored rings.

---

# 8. Movement System

## 8.1. Требования

Перемещение должно быть point-and-click.

Игрок кликает по земле — активный персонаж идет к точке.

На первом этапе:

- движется только активный персонаж;
- остальные следуют за ним на небольшом расстоянии;
- не нужна сложная формация.

---

## 8.2. Компоненты

- `ClickToMoveController`
- `PartyFollowController`
- `NavMeshAgent` на каждом персонаже.

---

## 8.3. Acceptance Criteria

- игрок кликает по земле;
- выбранный персонаж идет к точке;
- остальные персонажи следуют;
- персонажи не проходят через препятствия;
- персонажи останавливаются рядом с interactable object.

---

# 9. Camera System

## 9.1. Камера

Изометрическая камера сверху под углом.

Требования:

- follow party center или active character;
- zoom in/out;
- ограничение движения камеры рамками уровня;
- плавное движение.

---

## 9.2. CameraController

Поля:

```csharp
public Transform Target;
public float FollowSpeed;
public float ZoomSpeed;
public float MinZoom;
public float MaxZoom;
public Vector2 BoundsMin;
public Vector2 BoundsMax;
```

---

## 9.3. Управление

- колесо мыши — zoom;
- middle mouse drag или WASD — pan;
- optional: камера следует за active character.

---

# 10. Interaction System

## 10.1. Базовый интерфейс

Создать интерфейс:

```csharp
public interface IInteractable
{
    string InteractionName { get; }
    bool CanInteract(PlayerCharacter character);
    void Interact(PlayerCharacter character);
}
```

---

## 10.2. InteractableObject base class

Базовый класс для объектов:

```csharp
public abstract class InteractableObject : MonoBehaviour, IInteractable
```

Общие функции:

- highlight on hover;
- show interaction prompt;
- interaction distance;
- optional required character;
- optional required quest state.

---

## 10.3. Типы интерактивных объектов

| Object | Function |
|---|---|
| Generator G-17 | repair/start |
| Breaker Module | pickup |
| Filter Station | inspect |
| Hydroponic Farm | inspect |
| Defense Gate | inspect |
| Public Stage | inspect |
| Workshop | inspect |
| Relay Terminal | inspect/read log |
| Switch Room Console | open Power Allocation Board |
| NPC | start dialogue |
| Loot Container | optional pickup |

---

# 11. Quest System

## 11.1. QuestData

Создать ScriptableObject:

```csharp
QuestData
```

Поля:

```csharp
public string Id;
public string Title;
public string Description;
public List<ObjectiveData> Objectives;
```

---

## 11.2. ObjectiveData

```csharp
public string Id;
public string Description;
public ObjectiveStatus Status;
```

```csharp
public enum ObjectiveStatus
{
    Hidden,
    Active,
    Completed,
    Failed
}
```

---

## 11.3. QuestManager

Ответственность:

- запускать квесты;
- активировать objective;
- завершать objective;
- обновлять objective UI;
- реагировать на события.

---

## 11.4. Main Quest

```text
Restore Power to Riverside
```

Objectives:

1. Reach Riverside Shelter
2. Speak with Mara Vey
3. Resolve Riverside’s Power Priorities
4. Inspect Key Systems
5. Find 2 Breaker Modules
6. Repair Generator Line
7. Start Generator G-17
8. Defend Generator G-17
9. Allocate Power
10. Face the Consequences
11. Leave Riverside

---

## 11.5. Event-driven quest update

Рекомендуется использовать простую event system:

```csharp
GameEvents.OnNPCDialogueCompleted
GameEvents.OnSystemInspected
GameEvents.OnBreakerModuleCollected
GameEvents.OnGeneratorRepaired
GameEvents.OnGeneratorStarted
GameEvents.OnGeneratorDefended
GameEvents.OnPowerAllocated
```

---

# 12. Dialogue System

## 12.1. Требования для MVP

Диалоговая система должна поддерживать:

- NPC name;
- text lines;
- player response options;
- optional conditions;
- optional effects;
- companion comments;
- closing dialogue;
- starting quest events.

---

## 12.2. DialogueData

ScriptableObject:

```csharp
DialogueData
```

Поля:

```csharp
public string Id;
public string SpeakerName;
public List<DialogueNode> Nodes;
```

---

## 12.3. DialogueNode

```csharp
public string NodeId;
public string SpeakerId;
public string Text;
public List<DialogueChoice> Choices;
public List<DialogueEffect> Effects;
```

---

## 12.4. DialogueChoice

```csharp
public string Text;
public string NextNodeId;
public List<DialogueCondition> Conditions;
public List<DialogueEffect> Effects;
```

---

## 12.5. DialogueEffect examples

- set mission flag;
- start quest;
- complete objective;
- change relationship;
- reveal power priority;
- unlock inspection marker;
- add lore note.

---

## 12.6. DialogueManager

Ответственность:

- открыть Dialogue Panel;
- показать реплику;
- показать варианты;
- обработать выбор;
- применить effects;
- закрыть диалог;
- вернуть GameMode в Exploration.

---

# 13. Settlement Power Politics System

## 13.1. Назначение

Система описывает энергетические приоритеты поселения и отношения с группами.

---

## 13.2. PowerSystemData

ScriptableObject:

```csharp
PowerSystemData
```

Поля:

```csharp
public string Id;
public string DisplayName;
public string Description;
public int RequiredPower;
public string BenefitDescription;
public string DownsideDescription;
public string SupportingNPCId;
public string OpposingNPCIds;
public PowerChoice PowerChoice;
```

---

## 13.3. Системы Riverside

| ID | Display Name | Required Power |
|---|---|---:|
| water_filters | Water Filters | 35 |
| hydroponic_farm | Hydroponic Farm | 40 |
| defense_grid | Electric Fence | 30 |
| public_stage | Public Stage | 15 |
| workshop | Workshop | 20 |
| relay_station | Relay Station | 25 |
| party_reserve | Party Reserve | 10–25 |
| split_load | Split Load | variable |

---

## 13.4. RelationshipManager

Хранит отношения:

```csharp
public enum RelationshipLevel
{
    Low,
    Medium,
    High
}
```

```csharp
public class RelationshipState
{
    public RelationshipLevel RiversideTrust;
    public RelationshipLevel DefenseSupport;
    public RelationshipLevel FarmSupport;
    public RelationshipLevel MedicalSupport;
    public RelationshipLevel StageSupport;
    public RelationshipLevel WorkshopSupport;
    public RelationshipLevel RelaySupport;
}
```

---

## 13.5. PowerManager

Ответственность:

- хранить доступную мощность;
- хранить список PowerSystemData;
- открыть Power Allocation Board;
- применить выбор игрока;
- обновить MissionState;
- обновить RelationshipState;
- вызвать визуальные последствия.

---

# 14. Power Allocation Board UI

## 14.1. Назначение

Финальный экран миссии, где игрок выбирает, куда направить питание.

---

## 14.2. UI должен показывать

- Available Stable Output;
- список систем;
- требуемую мощность;
- описание эффекта;
- минус выбора;
- NPC supporter;
- NPC opposition;
- companion comment;
- overload risk;
- Confirm button.

---

## 14.3. MVP-версия

Для первой сборки можно сделать простой вариант:

Игрок выбирает **один** вариант:

- Water Filters;
- Hydroponic Farm;
- Defense Grid;
- Public Stage;
- Workshop;
- Relay Station;
- Party Reserve;
- Split Load.

После выбора:

- показывается Result Panel;
- обновляются отношения;
- миссия завершается.

---

## 14.4. Более поздняя версия

Позже можно разрешить распределять проценты вручную.

Но для MVP это не обязательно.

---

# 15. Combat System

## 15.1. MVP combat model

Первый прототип:

- real-time combat;
- без сложной тактической паузы;
- персонажи и враги стреляют на расстоянии;
- есть HP;
- есть простые способности;
- есть укрытия как visual/positioning objects;
- генератор имеет HP;
- враги могут атаковать генератор.

---

## 15.2. CombatManager

Ответственность:

- включать combat mode;
- спавнить врагов;
- отслеживать living enemies;
- отслеживать generator health;
- завершать бой;
- уведомлять QuestManager.

---

## 15.3. EnemyData

ScriptableObject:

```csharp
EnemyData
```

Поля:

```csharp
public string Id;
public string DisplayName;
public int MaxHealth;
public int Damage;
public float AttackRange;
public float AttackCooldown;
public float MoveSpeed;
public EnemyRole Role;
```

```csharp
public enum EnemyRole
{
    Scavenger,
    Gunner,
    Runner,
    Saboteur
}
```

---

## 15.4. EnemyAI

Простая state machine:

```csharp
public enum EnemyState
{
    Idle,
    MoveToCover,
    AttackPlayer,
    AttackGenerator,
    SabotageGenerator,
    Dead
}
```

---

## 15.5. Враги MVP

| Enemy | Count | Behavior |
|---|---:|---|
| Raider Scavenger | 2 | attacks party |
| Raider Gunner | 1 | stays at distance, higher damage |
| Raider Runner | 1 | flanks / attacks weak character |
| Raider Saboteur | 1 | moves to generator and damages it |

---

## 15.6. Generator Health

Генератор должен иметь HP.

```csharp
public class Generator : InteractableObject, IDamageable
{
    public int MaxHealth;
    public int CurrentHealth;
    public bool IsRepaired;
    public bool IsStarted;
}
```

Если HP генератора падает до 0:

- бой проигран;
- objective failed;
- можно показать restart panel.

Для MVP можно не делать полноценный Game Over, а просто вывести “Generator destroyed — restart encounter”.

---

# 16. Ability System

## 16.1. Для MVP

Нужны простые способности:

| Character | Ability | Effect |
|---|---|---|
| Dax | Repair Burst | чинит часть HP генератора |
| Dax | Suppressive Shot | наносит урон врагу |
| Nyra | Shock Pulse | оглушает врага |
| Nyra | Overload Panel | наносит урон рядом с электрощитком |
| Cormac | Emergency Heal | лечит союзника |
| Cormac | Field Stabilize | временно снижает получаемый урон |

---

## 16.2. AbilityData

ScriptableObject:

```csharp
public class AbilityData : ScriptableObject
{
    public string Id;
    public string DisplayName;
    public string Description;
    public Sprite Icon;
    public float Cooldown;
    public int EnergyCost;
    public AbilityTargetType TargetType;
}
```

```csharp
public enum AbilityTargetType
{
    Self,
    Ally,
    Enemy,
    Ground,
    Interactable
}
```

---

# 17. UI Requirements

## 17.1. HUD

HUD должен содержать:

- портреты Dax, Nyra, Cormac;
- HP bars;
- energy bars;
- selected character indicator;
- current objective;
- ability bar;
- interact prompt;
- generator status during combat.

---

## 17.2. Objective Panel

Показывает текущую цель.

Пример:

```text
Restore Power to Riverside
Find 2 breaker modules: 1/2
```

---

## 17.3. Dialogue Panel

Должен показывать:

- имя говорящего;
- текст;
- варианты ответа;
- кнопки выбора;
- optional companion comments.

---

## 17.4. Combat UI

Должен показывать:

- enemy health bars;
- generator health;
- ability cooldowns;
- damage feedback;
- combat objective.

---

## 17.5. Result Panel

После распределения питания показать:

- выбранную систему;
- кто доволен;
- кто недоволен;
- состояние воды/еды/обороны/связи;
- изменение trust;
- “Continue” button.

---

# 18. Level Blockout Requirements

## 18.1. Основные зоны

В сцене должны быть зоны:

1. Arrival Street;
2. Local Shelter;
3. Filter Station;
4. Hydroponic Room;
5. Defense Gate;
6. Public Stage;
7. Workshop;
8. Relay Station;
9. Generator Yard;
10. Switch Room.

---

## 18.2. Что нужно в каждой зоне

### Arrival Street

- старт отряда;
- мокрая улица;
- знак о воде;
- путь к убежищу.

### Local Shelter

- Mara;
- несколько placeholder NPC;
- центр поселения.

### Filter Station

- Dr. Sela;
- фильтры;
- резервуар;
- inspect trigger.

### Hydroponic Room

- Ivo;
- растения;
- насос;
- inspect trigger.

### Defense Gate

- Hale;
- ворота;
- электрозабор;
- прожектор;
- inspect trigger.

### Public Stage

- Edda;
- сцена;
- прожектор;
- старый микрофон;
- inspect trigger.

### Workshop

- Greer;
- инструменты;
- зарядная стойка;
- inspect trigger.

### Relay Station

- Lysa;
- терминал;
- антенна;
- Blind Protocol log.

### Generator Yard

- Generator G-17;
- укрытия;
- enemy spawn points;
- combat arena.

### Switch Room

- console;
- Power Allocation Board trigger.

---

# 19. Visual Feedback Requirements

## 19.1. Interactable highlights

Интерактивные объекты должны подсвечиваться при наведении.

Состояния:

- normal;
- hover;
- unavailable;
- completed.

---

## 19.2. Quest markers

Для MVP можно использовать простые world-space icons:

- `!` for NPC;
- gear icon for generator;
- magnifier for inspect;
- module icon for breaker module;
- lightning icon for power systems.

---

## 19.3. Power result feedback

После финального выбора должны включиться визуальные элементы:

| Choice | Visual Change |
|---|---|
| Water Filters | blue/white lights at filter station |
| Hydroponic Farm | warm grow lights in farm |
| Defense Grid | fence sparks / floodlights |
| Public Stage | stage light turns on |
| Workshop | sparks/tools/lamp |
| Relay Station | antenna light / radio signal |
| Party Reserve | portable cells glow |
| Split Load | several weak flickering lights |

---

# 20. Audio Requirements

Для первого прототипа можно использовать placeholder audio.

Минимум:

- click sound;
- interact sound;
- dialogue open sound;
- generator start;
- generator hum;
- gunshot;
- enemy hit;
- UI confirm;
- power allocation confirm;
- ambient loop.

---

# 21. Save / Runtime State

## 21.1. Для MVP

Не нужен полноценный save/load.

Достаточно runtime state в GameManager.

Но структура должна быть готова к сохранению.

---

## 21.2. SaveData draft

```csharp
public class SaveData
{
    public MissionState MissionState;
    public RelationshipState RelationshipState;
    public List<string> CompletedQuestIds;
    public List<string> FoundLoreIds;
}
```

---

# 22. Data Setup for Prototype

## 22.1. Создать ScriptableObjects

### Characters

- SO_Character_Dax
- SO_Character_Nyra
- SO_Character_Cormac

### NPCs

- SO_NPC_Mara
- SO_NPC_Hale
- SO_NPC_Ivo
- SO_NPC_Sela
- SO_NPC_Edda
- SO_NPC_Greer
- SO_NPC_Lysa

### Enemies

- SO_Enemy_RaiderScavenger
- SO_Enemy_RaiderGunner
- SO_Enemy_RaiderRunner
- SO_Enemy_RaiderSaboteur

### Power Systems

- SO_Power_WaterFilters
- SO_Power_HydroponicFarm
- SO_Power_DefenseGrid
- SO_Power_PublicStage
- SO_Power_Workshop
- SO_Power_RelayStation
- SO_Power_PartyReserve
- SO_Power_SplitLoad

### Quest

- SO_Quest_RestorePowerToRiverside

---

# 23. Prototype Flow

## 23.1. Full intended flow

1. Scene loads.
2. Party spawns at Arrival Street.
3. Objective: Reach Riverside Shelter.
4. Player meets Mara.
5. Quest Power Priorities starts.
6. Player talks to NPCs.
7. Player inspects systems.
8. Player collects 2 breaker modules.
9. Player repairs generator.
10. Player starts generator.
11. Combat begins.
12. Player defeats raiders.
13. Switch Room unlocks.
14. Player opens Power Allocation Board.
15. Player chooses final power allocation.
16. Result Panel appears.
17. Mission complete.

---

## 23.2. Shortest debug flow

Для тестирования нужна возможность быстро пройти прототип:

- debug button: collect all breaker modules;
- debug button: start combat;
- debug button: win combat;
- debug button: open Power Allocation Board.

---

# 24. Acceptance Criteria

## 24.1. Exploration

- персонажи двигаются по клику;
- камера следует за отрядом;
- NPC интерактивны;
- объекты подсвечиваются;
- objective panel обновляется.

---

## 24.2. Dialogue

- диалог открывается по клику на NPC;
- варианты ответа работают;
- диалог может менять mission flags;
- после разговора objective обновляется.

---

## 24.3. Quest

- квест запускается после Mara;
- breaker modules считаются;
- генератор нельзя запустить без модулей;
- после запуска начинается бой;
- после боя открывается Switch Room;
- после выбора питания миссия завершается.

---

## 24.4. Combat

- враги спавнятся;
- враги атакуют игрока или генератор;
- игрок может наносить урон;
- HP обновляется;
- враги умирают;
- бой завершается при победе;
- генератор может быть поврежден.

---

## 24.5. Power Allocation

- доска открывается после боя;
- игрок может выбрать вариант;
- выбор сохраняется в MissionState;
- отношения обновляются;
- появляется Result Panel;
- визуальный результат включается в сцене.

---

# 25. Suggested Implementation Order

## Phase 1 — Foundation

1. Create Unity project structure.
2. Create Riverside_Prototype scene.
3. Add managers.
4. Add camera.
5. Add player party placeholders.
6. Add click-to-move.

---

## Phase 2 — Interaction and Quest

1. Add IInteractable.
2. Add NPC interaction.
3. Add quest manager.
4. Add objective panel.
5. Add breaker module pickups.
6. Add generator repair/start.

---

## Phase 3 — Dialogue

1. Add dialogue data structures.
2. Add dialogue UI.
3. Add Mara dialogue.
4. Add NPC priority dialogues as placeholders.
5. Connect dialogue effects to mission flags.

---

## Phase 4 — Power Politics

1. Add PowerSystemData.
2. Add RelationshipState.
3. Add PowerManager.
4. Add Power Allocation Board.
5. Add result panel.

---

## Phase 5 — Combat

1. Add EnemyData.
2. Add EnemyAI.
3. Add CombatManager.
4. Add enemy spawns.
5. Add generator health.
6. Add win/lose condition.

---

## Phase 6 — Polish for Vertical Slice

1. Add visual feedback.
2. Add placeholder sounds.
3. Add better UI styling.
4. Add debug tools.
5. Add result variations.
6. Fix bugs.

---

# 26. Codex Rules

When Codex starts implementation:

1. Build in small steps.
2. Do not implement the whole game at once.
3. Prefer simple working systems over complex abstractions.
4. Keep code readable.
5. Use placeholder assets.
6. Do not add multiplayer.
7. Do not add advanced inventory.
8. Do not add procedural generation.
9. Do not overbuild dialogue system.
10. Every implemented feature must have clear acceptance criteria.

---

# 27. Next Required File

After this TechSpec, create:

## IsoLight_Codex_MVP_Backlog_v0.1.md

That file should break this TechSpec into concrete Codex tasks:

- task title;
- goal;
- files to create/edit;
- implementation notes;
- acceptance criteria;
- what not to do.

---

# 28. Short Summary

This prototype is not the full IsoLight game.

It is a functional vertical slice proving that the main idea works:

- a settlement needs power;
- different NPCs fight for priority;
- the player investigates;
- the player restores power;
- enemies attack;
- the player distributes energy;
- the settlement changes.

The first build should feel simple, readable and playable.

The emotional goal:

> The player should not ask “which option is correct?”  
> The player should ask “who am I willing to leave in the dark?”
