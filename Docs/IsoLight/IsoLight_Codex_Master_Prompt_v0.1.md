# IsoLight — Codex Master Prompt v0.1

**Игра:** IsoLight  
**Документ:** Codex Master Prompt  
**Версия:** 0.1  
**Цель:** главный промт для старта разработки Unity-прототипа через Codex.

---

# 1. Как использовать этот файл

Этот файл нужно дать Codex вместе с проектными документами:

1. `IsoLight_Core_Concept_v0.2.md`
2. `IsoLight_GDD_v0.1.md`
3. `IsoLight_Riverside_District_Mission_v0.2.md`
4. `IsoLight_Dialogue_Style_Bible_v0.1.md`
5. `IsoLight_Unity_Prototype_TechSpec_v0.1.md`
6. `IsoLight_Codex_MVP_Backlog_v0.1.md`
7. `IsoLight_Codex_Master_Prompt_v0.1.md`

Codex должен сначала прочитать документы, понять масштаб MVP и только потом выполнять конкретный batch задач.

---

# 2. Master Prompt для Codex

```text
You are working on a Unity prototype for a game called IsoLight.

IsoLight is an isometric post-apocalyptic RPG / tactical action RPG.

Core idea:
After an unclear catastrophe, civilization is fragmented. Open water is unsafe, soil is unreliable, and surviving settlements depend on electricity for water filtration, hydroponic farms, medicine, communication, defense, workshops and social order.

Tagline:
Every light is an isle.

The first prototype is not the full game.
The goal is a playable vertical-slice prototype of the first mission:
Riverside District.

The key gameplay loop:
talk → investigate → repair → defend → allocate power → face consequences

Important design documents are attached:
- IsoLight_Core_Concept_v0.2.md
- IsoLight_GDD_v0.1.md
- IsoLight_Riverside_District_Mission_v0.2.md
- IsoLight_Dialogue_Style_Bible_v0.1.md
- IsoLight_Unity_Prototype_TechSpec_v0.1.md
- IsoLight_Codex_MVP_Backlog_v0.1.md

Before writing code:
1. Read the TechSpec.
2. Read the MVP Backlog.
3. Follow the backlog task order.
4. Do not implement systems that are not requested in the current task batch.
5. Prefer simple working placeholder systems over complex abstractions.
6. Keep the project compiling after every batch.
7. Report exactly what you changed and how to test it.

Technical target:
- Unity 3D.
- Use C#.
- Use namespace IsoLight and sub-namespaces where appropriate.
- Use placeholder assets.
- Prefer ScriptableObjects for game data.
- Prefer MonoBehaviour components for prototype behavior.
- Use NavMeshAgent for movement.
- Use uGUI for prototype UI unless the existing project already uses UI Toolkit.
- Do not add paid assets or external packages unless explicitly requested.
- Do not build multiplayer, procedural generation, advanced inventory, full RPG progression, open world, final art, final animation, or full voiceover.

If the Unity project already exists:
- Work inside the existing structure.
- Do not rename unrelated folders.
- Do not delete existing work.
- Add files in the IsoLight folder structure described in the TechSpec.

If the Unity project does not exist:
- Create the recommended folder structure under Assets/IsoLight.
- Add scripts and editor utilities required to generate the prototype scene.
- If creating .unity scenes directly is not practical, create an Editor script that can generate the prototype scene and root objects from a Unity menu command.

Development principle:
gray boxes > playable mission > readable UI > simple combat > consequences > better art

The prototype should become playable as early as possible.
```

---

# 3. Rules for Codex

## 3.1. Always do

```text
- Keep changes small and testable.
- Implement one batch at a time.
- Use clear names.
- Keep scripts in the correct folders.
- Use the IsoLight namespace.
- Keep the Unity project compiling.
- Add comments only where they explain non-obvious logic.
- Use placeholder primitives if art assets are missing.
- Report assumptions.
- Report how to test the result in Unity.
```

---

## 3.2. Never do without permission

```text
- Do not implement the full game.
- Do not add multiplayer.
- Do not add procedural generation.
- Do not create a large open world.
- Do not add complex survival systems.
- Do not add advanced inventory.
- Do not add final art.
- Do not add paid assets.
- Do not add external packages unless explicitly asked.
- Do not rewrite the architecture from scratch after it works.
- Do not ignore the backlog order.
```

---

# 4. Coding Standards

## 4.1. Namespace

Use:

```csharp
namespace IsoLight
```

Or for subsystems:

```csharp
namespace IsoLight.Core
namespace IsoLight.Characters
namespace IsoLight.Party
namespace IsoLight.Interaction
namespace IsoLight.Dialogue
namespace IsoLight.Quests
namespace IsoLight.Combat
namespace IsoLight.Enemies
namespace IsoLight.Power
namespace IsoLight.Relationships
namespace IsoLight.UI
namespace IsoLight.Save
namespace IsoLight.Utilities
```

---

## 4.2. File placement

Scripts must be placed in:

```text
Assets/IsoLight/Scripts/
```

Data assets must be placed in:

```text
Assets/IsoLight/Data/
```

Prefabs must be placed in:

```text
Assets/IsoLight/Prefabs/
```

Scenes must be placed in:

```text
Assets/IsoLight/Scenes/Prototype/
```

---

## 4.3. Simple prototype architecture

Use this approach:

```text
Managers hold runtime state.
ScriptableObjects hold static data.
MonoBehaviours implement scene behavior.
UI components display state.
Events connect systems where useful.
```

Avoid overengineering.

---

# 5. Current Target: First Playable MVP

## 5.1. Mission

**Riverside District**

## 5.2. Player party

- Dax — Scavenger / repairman
- Nyra — Tech specialist
- Cormac — Medic

## 5.3. Key NPCs

- Mara Vey — settlement coordinator
- Captain Varric Hale — defense chief
- Ivo Renn — hydroponic farmer
- Dr. Sela Orin — doctor
- Edda Vale — actress / public stage keeper
- Tomas Greer — mechanic
- Lysa Marr — relay technician
- Rook — raider leader

## 5.4. Key systems to prototype

- party movement;
- NPC dialogue;
- quest objectives;
- generator repair/start;
- enemy attack;
- power allocation;
- relationship changes;
- result panel.

---

# 6. First Codex Batch

## 6.1. Instruction

Start with this batch only:

```text
Implement Batch 1 — Project Skeleton.

Tasks:
- TASK 0.1 — Create Unity Project Structure
- TASK 0.2 — Create Main Prototype Scene
- TASK 0.3 — Create Core Enums and Runtime State
- TASK 0.4 — Create Basic Managers

Do not implement gameplay yet.
Do not implement movement yet.
Do not implement dialogue yet.
Do not implement combat yet.
Do not implement UI yet beyond placeholder scene root objects.

The goal of this batch is only:
- folder structure;
- prototype scene root objects;
- core enums;
- runtime state classes;
- empty manager components;
- project compiles.
```

---

## 6.2. Expected files for Batch 1

Codex should create or prepare:

```text
Assets/IsoLight/Scripts/Core/GameMode.cs
Assets/IsoLight/Scripts/Core/MissionState.cs
Assets/IsoLight/Scripts/Power/PowerChoice.cs
Assets/IsoLight/Scripts/Relationships/RelationshipLevel.cs
Assets/IsoLight/Scripts/Relationships/RelationshipState.cs

Assets/IsoLight/Scripts/Core/GameManager.cs
Assets/IsoLight/Scripts/Input/InputManager.cs
Assets/IsoLight/Scripts/Camera/CameraManager.cs
Assets/IsoLight/Scripts/Party/PartyManager.cs
Assets/IsoLight/Scripts/Quests/QuestManager.cs
Assets/IsoLight/Scripts/Dialogue/DialogueManager.cs
Assets/IsoLight/Scripts/Combat/CombatManager.cs
Assets/IsoLight/Scripts/Power/PowerManager.cs
Assets/IsoLight/Scripts/Relationships/RelationshipManager.cs
Assets/IsoLight/Scripts/Save/SaveManager.cs
Assets/IsoLight/Scripts/UI/UIManager.cs
```

If direct scene creation is difficult, create:

```text
Assets/IsoLight/Scripts/Editor/IsoLightPrototypeSceneBuilder.cs
```

This editor script should create:

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

And save the scene as:

```text
Assets/IsoLight/Scenes/Prototype/Riverside_Prototype.unity
```

---

## 6.3. Acceptance Criteria for Batch 1

Batch 1 is complete only if:

```text
- The folder structure exists.
- The C# scripts compile.
- The core enums exist.
- MissionState exists.
- RelationshipState exists.
- Empty manager scripts exist.
- The prototype scene can be created or opened.
- The scene has the required root objects.
- No gameplay has been implemented yet.
```

---

# 7. Reporting Format for Codex

After finishing each batch, report in this format:

```text
Batch completed:
[Batch name]

Files created:
- ...

Files modified:
- ...

Scene changes:
- ...

Assumptions:
- ...

How to test:
1. ...
2. ...
3. ...

What is intentionally not implemented yet:
- ...

Next recommended batch:
- ...
```

---

# 8. Batch Prompts

These prompts can be sent to Codex one by one.

---

## Batch 1 Prompt — Project Skeleton

```text
Read:
- IsoLight_Unity_Prototype_TechSpec_v0.1.md
- IsoLight_Codex_MVP_Backlog_v0.1.md
- IsoLight_Codex_Master_Prompt_v0.1.md

Implement Batch 1 only:
- TASK 0.1
- TASK 0.2
- TASK 0.3
- TASK 0.4

Do not implement gameplay yet.

Create the Unity folder structure, core enums, MissionState, RelationshipState and empty managers.

If creating the Unity scene directly is not practical, create an Editor script named IsoLightPrototypeSceneBuilder that generates the Riverside_Prototype scene and required root objects.

Keep the project compiling.
Report using the required reporting format.
```

---

## Batch 2 Prompt — Movement

```text
Implement Batch 2 — Movement.

Tasks:
- TASK 1.1 — Isometric Camera Controller
- TASK 1.2 — Create Player Character Data
- TASK 1.3 — Create PlayerCharacter Component
- TASK 1.4 — Point-and-Click Movement
- TASK 1.5 — Party Selection

Do not implement interaction, dialogue, combat or power allocation yet.

Use placeholder capsule/cube characters if no art exists.
Use NavMeshAgent for movement.
Create Dax, Nyra and Cormac placeholder player prefabs if possible.
Make the camera follow the active character or party center.
Keep the project compiling.
Report using the required reporting format.
```

---

## Batch 3 Prompt — Interaction and Quest Foundation

```text
Implement Batch 3 — Interaction and Quest Foundation.

Tasks:
- TASK 2.1 — Interactable Interface
- TASK 2.2 — Interaction Prompt UI
- TASK 2.3 — Quest Data Structures
- TASK 2.4 — Quest Manager and Objective Panel

Do not implement dialogue, combat or power allocation yet.

Create a simple interaction system that supports hover highlight and click interaction.
Create a simple quest manager and objective panel.
Use placeholder UI.

Keep the project compiling.
Report using the required reporting format.
```

---

## Batch 4 Prompt — Quest Objects

```text
Implement Batch 4 — Quest Objects.

Tasks:
- TASK 2.5 — Breaker Module Pickup
- TASK 2.6 — Inspectable Power Systems
- TASK 6.2 — Add Switch Room Console

Add placeholder interactable objects:
- Breaker Modules
- Filter Station
- Hydroponic Farm
- Defense Gate
- Public Stage
- Workshop
- Relay Station
- Switch Room Console

The console should remain locked until the generator is defended.
Do not implement the Power Allocation Board yet.

Keep the project compiling.
Report using the required reporting format.
```

---

## Batch 5 Prompt — Dialogue

```text
Implement Batch 5 — Dialogue.

Tasks:
- TASK 3.1 — Dialogue Data Structures
- TASK 3.2 — Dialogue Manager
- TASK 3.3 — Dialogue UI
- TASK 3.4 — NPC Interaction
- TASK 3.5 — Add Placeholder Dialogues

Use the Dialogue Style Bible for tone, but do not overbuild the system.
The first implementation can use simple ScriptableObject dialogue data or a lightweight serializable structure.

Required NPC placeholder dialogues:
- Mara intro
- Hale priority
- Ivo priority
- Sela priority
- Edda priority
- Greer priority
- Lysa priority
- Party Reserve discussion placeholder

Dialogues should be able to set MissionState flags.

Keep the project compiling.
Report using the required reporting format.
```

---

## Batch 6 Prompt — Power Politics

```text
Implement Batch 6 — Power Politics.

Tasks:
- TASK 4.1 — PowerSystemData
- TASK 4.2 — Relationship Manager
- TASK 4.3 — Power Manager
- TASK 4.4 — Power Allocation Board UI
- TASK 4.5 — Result Panel UI
- TASK 4.6 — Party Reserve Logic

For MVP, the player chooses one power option:
- Water Filters
- Hydroponic Farm
- Defense Grid
- Public Stage
- Workshop
- Relay Station
- Party Reserve
- Split Load

After choosing:
- save MissionState.FinalPowerChoice;
- update RelationshipState;
- show Result Panel;
- mark mission complete.

Keep the project compiling.
Report using the required reporting format.
```

---

## Batch 7 Prompt — Combat

```text
Implement Batch 7 — Combat.

Tasks:
- TASK 5.1 — Damageable Interface
- TASK 5.2 — Enemy Data and Enemy Component
- TASK 5.3 — Basic Enemy AI
- TASK 5.4 — Basic Player Attack
- TASK 5.5 — Combat Manager
- TASK 5.6 — Generator Object

Create a basic real-time combat encounter around Generator G-17.
Enemies should spawn after generator start.
Enemies can attack the party or the generator.
The player can attack enemies with the selected character.
The battle ends when all enemies are defeated.
If the generator is destroyed, show a simple failure state.

Keep the project compiling.
Report using the required reporting format.
```

---

## Batch 8 Prompt — Mission Flow and Core UI

```text
Implement Batch 8 — Mission Flow and Core UI.

Tasks:
- TASK 6.1 — Connect Main Quest Flow
- TASK 6.3 — Visual Result States
- TASK 7.1 — Character HUD
- TASK 7.2 — Enemy Health Bars
- TASK 7.3 — Generator Status UI

Connect the mission from start to finish:
Mara dialogue → priority NPCs → inspections → breaker modules → generator repair → combat → power allocation → result.

Add readable HUD elements for party, enemies and generator.

Keep visual feedback simple.
Use placeholder objects and lights.

Keep the project compiling.
Report using the required reporting format.
```

---

## Batch 9 Prompt — Debug and Polish

```text
Implement Batch 9 — Debug and Polish.

Tasks:
- TASK 7.5 — Notification Toasts
- TASK 8.1 — Debug Panel
- TASK 8.2 — Basic Audio Hooks
- TASK 8.3 — Placeholder Materials and Readability

Add debug shortcuts:
- collect all breaker modules;
- start generator;
- start combat;
- win combat;
- open Power Allocation Board;
- reset mission state.

Improve readability of the placeholder scene.
Do not add final art.
Do not add paid assets.

Keep the project compiling.
Report using the required reporting format.
```

---

# 9. Minimal Success Definition

The prototype is successful when a player can:

```text
1. Start in Riverside District.
2. Move the party.
3. Talk to Mara.
4. Talk to at least four priority NPCs.
5. Inspect key power systems.
6. Collect 2 breaker modules.
7. Repair and start Generator G-17.
8. Fight raiders.
9. Protect the generator.
10. Open the Power Allocation Board.
11. Choose where the power goes.
12. See the consequences.
```

---

# 10. Final Reminder for Codex

Do not chase the full dream of IsoLight yet.

The current mission is to build a playable proof of the core idea:

> In a world where electricity means water, food, safety and hope, the player must choose who gets the light.

Make it work first.

Make it beautiful later.
