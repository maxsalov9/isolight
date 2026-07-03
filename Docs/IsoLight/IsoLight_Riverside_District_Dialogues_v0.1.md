# IsoLight — Riverside District Dialogues v0.1

**Игра:** IsoLight  
**Документ:** Dialogue Script / Mission Dialogue Content  
**Миссия:** Riverside District  
**Версия:** 0.1  
**Назначение:** прописать игровые диалоги первой миссии для будущей Dialogue System и Codex-реализации.

---

# 1. Общие правила

## 1.1. Стиль

Диалоги должны быть оригинальными и работать как психологические дуэли:

- NPC не просто сообщает информацию, а пытается склонить игрока к своему решению.
- Каждый важный персонаж говорит правду, но не всю.
- Вежливость часто звучит опаснее угрозы.
- Бытовые детали раскрывают масштаб катастрофы.
- После разговора игрок должен сомневаться сильнее, а не получать очевидный правильный ответ.

---

## 1.2. Язык игровых реплик

Внутриигровые реплики в этом документе написаны на английском, потому что рабочее название, UI и слоган игры уже зафиксированы на английском.

Комментарии, условия и заметки оставлены на русском.

---

## 1.3. Условные обозначения

```text
[DAX]      реплика Dax
[NYRA]     реплика Nyra
[CORMAC]   реплика Cormac
[PLAYER]   варианты ответа игрока
[EFFECT]   эффект диалога
[FLAG]     изменение флага
[REL]      изменение отношений
[QUEST]    изменение квеста
[NOTE]     комментарий для разработчика / сценариста
```

---

# 2. Важные переменные

## 2.1. Mission flags

```text
has_met_guard
has_met_mara
power_priorities_started

talked_to_hale
talked_to_ivo
talked_to_sela
talked_to_edda
talked_to_greer
talked_to_lysa

inspected_water_filters
inspected_hydroponic_farm
inspected_defense_gate
inspected_public_stage
inspected_workshop
inspected_relay_station

found_blind_protocol_log
breaker_modules_collected
generator_repaired
generator_started
generator_defended

party_reserve_discussed
rook_contacted

final_power_choice
mission_completed
```

---

## 2.2. Relationship variables

```text
riverside_trust
defense_support
farm_support
medical_support
stage_support
workshop_support
relay_support

dax_approval
nyra_approval
cormac_approval
```

---

# 3. Dialogue Flow Overview

## 3.1. Основная последовательность

```text
D00_Guard_Entrance
  -> D01_Mara_Intro
    -> unlock Power Priorities
      -> D02_Hale_Defense
      -> D03_Sela_Water
      -> D04_Ivo_Farm
      -> D05_Edda_Stage
      -> D06_Greer_Workshop
      -> D07_Lysa_Relay
        -> D08_Party_Reserve
          -> D09_Rook_PreCombat
            -> Combat Barks
              -> D10_Power_Allocation_Confirmation
                -> Final Reaction Dialogue by choice
```

---

# 4. D00 — Entrance Guard

## Dialogue ID

`D00_GUARD_ENTRANCE`

## Location

Arrival Street / Riverside Gate

## Characters

- Guard
- Dax
- Player

## Conditions

```text
has_met_guard == false
```

## Purpose

Ввести игрока в тон поселения: сухой юмор, дефицит воды, недоверие к чужакам.

---

## Dialogue

**Guard:**

> If you're here for water, we're dry.  
> If you're here for shelter, we're full.  
> If you're here for trouble, take a number.

**[DAX]:**

> That's almost welcoming.

**Guard:**

> It was, before the filter died.

**[PLAYER OPTIONS]:**

1. `We're looking for a way through.`
2. `What happened to the filter?`
3. `Who runs this place?`
4. `We can help, if the price is right.`

---

### Option 1 — We're looking for a way through.

**Guard:**

> Everyone is. Roads are a funny thing now. They only go somewhere if the people along them let you keep walking.

**[DAX]:**

> That's a long way to say “talk to the boss.”

**Guard:**

> Mara Vey. Shelter hall. Don't touch the water barrels on the way in.

**[EFFECT]**

```text
has_met_guard = true
unlock marker: Local Shelter / Mara
```

---

### Option 2 — What happened to the filter?

**Guard:**

> Same thing that happens to everything with a wire in it. It remembered it was old.

**[CORMAC]:**

> People drinking untreated water?

**Guard:**

> People drinking water they call treated because calling it poison makes dinner awkward.

**[EFFECT]**

```text
has_met_guard = true
optional note: water_problem_hint = true
unlock marker: Local Shelter / Mara
```

---

### Option 3 — Who runs this place?

**Guard:**

> Depends who you ask. Mara keeps it standing. Hale keeps the gate shut. Sela keeps the sick quiet. Edda keeps the rest from becoming sick.

**[NYRA]:**

> And the generator?

**Guard:**

> The generator keeps us all honest.

**[EFFECT]**

```text
has_met_guard = true
unlock marker: Local Shelter / Mara
```

---

### Option 4 — We can help, if the price is right.

**Guard:**

> Careful. Around here the price is usually the part people understand. It's the “right” that gets everyone killed.

**[DAX]:**

> I like him.

**Guard:**

> You won't, if you stay long enough.

**[EFFECT]**

```text
has_met_guard = true
riverside_trust = medium
unlock marker: Local Shelter / Mara
```

---

# 5. D01 — Mara Intro

## Dialogue ID

`D01_MARA_INTRO`

## Location

Local Shelter

## Characters

- Mara Vey
- Dax
- Nyra
- Cormac
- Player

## Conditions

```text
has_met_guard == true
has_met_mara == false
```

## Purpose

Запустить миссию, объяснить, что генератор можно запустить, но главная проблема — распределение энергии.

---

## Scene Note

Mara стоит у старой доски распределения. На ней мелом написаны системы: WATER, FARM, GATE, RELAY, STAGE, WORKSHOP. Рядом висят ключи, кабели и карточки с именами людей.

---

## Dialogue

**Mara:**

> You're the outside crew.

**[DAX]:**

> Depends how expensive that sounds.

**Mara:**

> Everything sounds expensive when the lights are off.

**[PLAYER OPTIONS]:**

1. `We were told you control the route.`
2. `We heard you have a generator problem.`
3. `People outside say Riverside is dying.`
4. `We don't work for free.`

---

### Option 1 — We were told you control the route.

**Mara:**

> Control is a generous word. We know which streets don't cough up bodies in the morning. That passes for a map now.

**[NYRA]:**

> We need the north crossing.

**Mara:**

> Then I need a generator that doesn't sound like it's praying for death.

---

### Option 2 — We heard you have a generator problem.

**Mara:**

> No. We have a people problem. The generator is just where everyone points.

**[CORMAC]:**

> People problem?

**Mara:**

> Filter wants power. Farm wants power. Gate wants power. Relay wants power. Workshop wants power. Even the stage wants power.

**[DAX]:**

> The stage?

**Mara:**

> You'll laugh less after you meet Edda.

---

### Option 3 — People outside say Riverside is dying.

**Mara:**

> People outside love a clean sentence. “Riverside is dying.” Easy to carry. Harder to say: Riverside is arguing over which part gets to live first.

---

### Option 4 — We don't work for free.

**Mara:**

> Nobody does. Some just call the payment something prettier.

**[DAX]:**

> We can work with ugly words.

**Mara:**

> Good. We have plenty.

---

## Continue Node

**Mara:**

> G-17 can still run. Greer says it needs breaker modules. Nyra there can probably tell if he's lying.  
> But when it starts, we get one clean decision before the whole settlement tears itself into committees.

**[NYRA]:**

> How much stable output?

**Mara:**

> Enough to make one group grateful and six groups angry.

**[PLAYER OPTIONS]:**

1. `Who gets a claim?`
2. `Why not split the load?`
3. `Where are the breaker modules?`
4. `What happens if we walk away?`

---

### Option 1 — Who gets a claim?

**Mara:**

> Hale wants the fence. Sela wants water. Ivo wants the farm. Lysa wants the relay. Greer wants the workshop. Edda wants the stage.

**[DAX]:**

> And you?

**Mara:**

> I want tomorrow morning to happen.

**[EFFECT]**

```text
power_priorities_started = true
[QUEST] Start: Restore Power to Riverside
[QUEST] Objective: Resolve Riverside's Power Priorities
unlock NPC markers: Hale, Sela, Ivo, Edda, Greer, Lysa
```

---

### Option 2 — Why not split the load?

**Mara:**

> Because wires don't care about fairness. Split it wrong and everything flickers. People hate flicker. It gives them time to imagine who stole the steady light.

**[NYRA]:**

> Overload risk?

**Mara:**

> Greer says yes. Lysa says maybe. Hale says if we keep talking, Ashline decides for us.

**[EFFECT]**

```text
power_priorities_started = true
unlock Power Systems inspection
```

---

### Option 3 — Where are the breaker modules?

**Mara:**

> One in the flooded block, if the cabinet hasn't been stripped. One near the old farm line.  
> There was a third. Ashline took it, or Greer sold it, depending on which rumor you're tired of.

**[DAX]:**

> I prefer problems that come in pairs.

**[EFFECT]**

```text
[QUEST] Objective: Find 2 Breaker Modules
```

---

### Option 4 — What happens if we walk away?

**Mara:**

> Same thing that happens when anyone walks away. We talk about them like cowards until we need them again.

**[CORMAC]:**

> And the sick?

**Mara:**

> They don't get the luxury of opinions.

**[EFFECT]**

```text
power_priorities_started = true
riverside_trust = medium
```

---

## Exit

**Mara:**

> Talk to them. All of them, if you have the patience.  
> Then fix the generator.  
> Then make the decision I'll be blamed for.

**[EFFECT]**

```text
has_met_mara = true
power_priorities_started = true
[QUEST] Active: Resolve Riverside's Power Priorities
```

---

# 6. D02 — Captain Hale / Defense Grid

## Dialogue ID

`D02_HALE_DEFENSE`

## Location

Defense Gate

## Characters

- Captain Varric Hale
- Dax
- Nyra
- Player

## Conditions

```text
power_priorities_started == true
talked_to_hale == false
```

## Purpose

Представить аргумент обороны: без электрозабора всё остальное достанется рейдерам. Показать скрытый мотив: Hale хочет усилить власть.

---

## Scene Note

Hale стоит у обесточенного забора. На проволоке висят старые предупреждающие таблички. Один прожектор направлен вниз, как слепой глаз.

---

## Dialogue

**Hale:**

> Don't touch the wire.

**[PLAYER OPTIONS]:**

1. `It's dead.`
2. `You want it powered.`
3. `That's your opening argument?`
4. `We're not here to climb your fence.`

---

### Option 1 — It's dead.

**Hale:**

> So are most things that still teach people manners.

**[DAX]:**

> I know a few machines like that.

**Hale:**

> Then you know dead isn't useless.

---

### Option 2 — You want it powered.

**Hale:**

> I want the people behind it to sleep without rehearsing their last words.

---

### Option 3 — That's your opening argument?

**Hale:**

> My opening argument is the fence. My closing argument is what happens when it stays dark.

---

### Option 4 — We're not here to climb your fence.

**Hale:**

> Nobody is, until the water runs out on their side.

---

## Continue Node

**Hale:**

> Mara sent you to hear everybody's prayer. Doctor wants filters. Farmer wants grow lights. Edda wants a stage, because apparently applause scares bullets now.

**[PLAYER OPTIONS]:**

1. `Why should defense come first?`
2. `How close are the raiders?`
3. `People say you want control.`
4. `What exactly does the fence power?`

---

### Option 1 — Why should defense come first?

**Hale:**

> Because water can be stolen. Food can be burned. Medicine can be traded at gunpoint.  
> Defense is the box the rest survives in.

**[NYRA]:**

> A box can become a cage.

**Hale:**

> Only if the people inside forget why it has walls.

**[EFFECT]**

```text
defense_argument_heard = true
```

---

### Option 2 — How close are the raiders?

**Hale:**

> Close enough that our children know the sound of Ashline engines. Far enough that people still pretend the stage matters more than the gate.

**[DAX]:**

> That's not a distance.

**Hale:**

> It's the only distance that counts.

**[EFFECT]**

```text
ashline_threat_confirmed = true
```

---

### Option 3 — People say you want control.

**Hale:**

> People say that when they dislike the person holding the key.  
> They say “thank you” when the same key locks danger outside.

**[PLAYER OPTIONS]:**

1. `And if you lock people inside?`
2. `That doesn't answer the question.`
3. `Fair enough.`

#### 3.1 — And if you lock people inside?

**Hale:**

> Then someone should take the key from me. Preferably before they need the gate open.

**[NYRA]:**

> You make tyranny sound like a maintenance issue.

**Hale:**

> Most disasters are.

**[EFFECT]**

```text
hale_power_motive_revealed = true
defense_support = medium
```

#### 3.2 — That doesn't answer the question.

**Hale:**

> No. It answers the better one.

#### 3.3 — Fair enough.

**Hale:**

> Fair is what people say when they still believe the rules are watching.

---

### Option 4 — What exactly does the fence power?

**Hale:**

> Fence charge. Two floodlights. Gate lock. Siren if the line holds.  
> Enough to make a night attack expensive.

**[DAX]:**

> Expensive doesn't mean impossible.

**Hale:**

> Nothing good ever does.

**[EFFECT]**

```text
inspected_defense_gate_hint = true
```

---

## Exit

**Hale:**

> Give me the fence, and the rest get time to be right.  
> Don't, and we'll find out which of them can argue in the dark.

**[EFFECT]**

```text
talked_to_hale = true
defense_support = medium
```

---

# 7. D03 — Dr. Sela / Water Filters

## Dialogue ID

`D03_SELA_WATER`

## Location

Filter Station

## Characters

- Dr. Sela Orin
- Cormac
- Player

## Conditions

```text
power_priorities_started == true
talked_to_sela == false
```

## Purpose

Показать проблему воды: кипячение недостаточно, чистая на вид вода опасна.

---

## Scene Note

Sela выливает прозрачную воду в слив. Рядом стоят канистры с маркировкой AMBER / RED / SAFE? На столе — списки симптомов, а не имена.

---

## Dialogue

**Sela:**

> Don't look offended. The water was only pretending to be clean.

**[PLAYER OPTIONS]:**

1. `It looked fine.`
2. `Why waste it?`
3. `You're the doctor?`
4. `Cormac, what do you see?`

---

### Option 1 — It looked fine.

**Sela:**

> So do most lies, from a distance.

---

### Option 2 — Why waste it?

**Sela:**

> Waste is when you pour out something useful. This was an argument waiting for a stomach.

---

### Option 3 — You're the doctor?

**Sela:**

> On mornings when people live. On other mornings I'm just the person who writes slower than death arrives.

---

### Option 4 — Cormac, what do you see?

**[CORMAC]:**

> No smell. No color. That's worse than mud.

**Sela:**

> Your medic has excellent pessimism.

---

## Continue Node

**Sela:**

> Everyone boils. Everyone prays over the steam. Everyone feels civilized for a minute.  
> Then they come here with shaking hands and ask why fire didn't save them.

**[PLAYER OPTIONS]:**

1. `Boiling doesn't help?`
2. `What's in the water?`
3. `How bad is it?`
4. `Why should filters come before the farm?`

---

### Option 1 — Boiling doesn't help?

**Sela:**

> It helps against things polite enough to die from heat.  
> Metals don't care. Toxins don't care. Whatever the factories left in the river has no manners at all.

**[CORMAC]:**

> She's right.

**[EFFECT]**

```text
water_boiling_not_enough_learned = true
```

---

### Option 2 — What's in the water?

**Sela:**

> Which sample? The river gives us a different answer every week.  
> Last week: heavy metals. This week: algae bloom and something our strips refuse to name.

**[PLAYER]:**

1. `Refuse to name?`
2. `That's not reassuring.`
3. `Can Nyra analyze it?`

#### 2.1 — Refuse to name?

**Sela:**

> The old strips were printed before the world learned new ways to poison itself.

**[NYRA]:**

> I can check the sensor logs.

**[EFFECT]**

```text
unlock optional inspect: Filter Sensor Logs
```

---

### Option 3 — How bad is it?

**Sela:**

> Officially? Manageable.  
> Honestly? I stopped writing names on the water list. I write symptoms now. Faster.

**[CORMAC]:**

> How many?

**Sela:**

> Sick? Twenty-three. Admitting it? Eleven.

**[EFFECT]**

```text
sela_hidden_numbers_revealed = true
medical_support = medium
```

---

### Option 4 — Why should filters come before the farm?

**Sela:**

> Because the future is a luxury purchased by people who survive the present.

**[PLAYER]:**

1. `Ivo says the farm saves the winter.`
2. `Hale says the fence saves everything.`
3. `Edda says morale keeps people together.`

#### 4.1 — Ivo says the farm saves the winter.

**Sela:**

> I like Ivo. He thinks in seasons. Disease thinks in hours.

#### 4.2 — Hale says the fence saves everything.

**Sela:**

> Hale sees enemies outside the gate. I see what people carry in by the cup.

#### 4.3 — Edda says morale keeps people together.

**Sela:**

> Then let her perform in the clinic. People appreciate tragedy more when it's not in their blood.

---

## Exit

**Sela:**

> When you choose, don't think about pipes. Think about how many people here are already carrying bad water inside them.

**[EFFECT]**

```text
talked_to_sela = true
medical_support = medium
```

---

# 8. D04 — Ivo Renn / Hydroponic Farm

## Dialogue ID

`D04_IVO_FARM`

## Location

Hydroponic Room

## Characters

- Ivo Renn
- Nyra
- Cormac
- Player

## Conditions

```text
power_priorities_started == true
talked_to_ivo == false
```

## Purpose

Показать аргумент еды и долгосрочного выживания. Выявить, что ферма уже повреждена сильнее, чем Ivo признается.

---

## Scene Note

Ферма освещена аварийным тусклым светом. Часть лотков пустая. Несколько растений зеленые, но выглядят вялыми. Ivo проверяет насос, который не качает раствор.

---

## Dialogue

**Ivo:**

> Step carefully. Half the floor is cable, and the other half is hope.

**[PLAYER OPTIONS]:**

1. `This is the farm?`
2. `Looks fragile.`
3. `Mara says you want priority power.`
4. `Why not grow outside?`

---

### Option 1 — This is the farm?

**Ivo:**

> No. This is the argument that keeps losing to louder arguments.

---

### Option 2 — Looks fragile.

**Ivo:**

> Everything alive is fragile. The dead are wonderfully durable.

---

### Option 3 — Mara says you want priority power.

**Ivo:**

> Mara says many things in the tone of someone already preparing to apologize.

---

### Option 4 — Why not grow outside?

**Ivo:**

> We tried. The soil gave us leaves that looked edible and roots that tested like battery waste.

**[NYRA]:**

> Accumulation?

**Ivo:**

> That's the polite word for “the ground remembers everything we did to it.”

**[EFFECT]**

```text
soil_unsafe_learned = true
```

---

## Continue Node

**Ivo:**

> The filters save the week. The fence saves the night.  
> This saves the winter.

**[PLAYER OPTIONS]:**

1. `How long can the farm last without power?`
2. `Sela says people are sick now.`
3. `Are you hiding crop losses?`
4. `What do you need exactly?`

---

### Option 1 — How long can the farm last without power?

**Ivo:**

> Depends which lie you prefer.  
> The official answer is two days.  
> The honest answer is that some of it has already started dying.

**[NYRA]:**

> Started?

**Ivo:**

> Plants die politely. They stay green long enough to make you feel responsible.

**[EFFECT]**

```text
ivo_crop_loss_revealed = true
farm_support = medium
```

---

### Option 2 — Sela says people are sick now.

**Ivo:**

> Sela is right. That's the worst part.  
> She's right now. I'm right later. Riverside can't afford both of us, so everyone pretends time is a moral category.

**[CORMAC]:**

> Sick people may not reach later.

**Ivo:**

> Hungry people won't either.

---

### Option 3 — Are you hiding crop losses?

**Ivo:**

> I'm delaying panic. People prefer that term when the lie protects breakfast.

**[PLAYER OPTIONS]:**

1. `How much is gone?`
2. `Does Mara know?`
3. `That's manipulation.`

#### 3.1 — How much is gone?

**Ivo:**

> Upper trays are gone. Seedlings in the east rack are pretending. If the nutrient loop stops again, I stop pretending with them.

#### 3.2 — Does Mara know?

**Ivo:**

> Mara knows enough to worry and not enough to act. That's leadership.

#### 3.3 — That's manipulation.

**Ivo:**

> Yes. Mine comes with vegetables.

**[EFFECT]**

```text
ivo_hidden_motive_revealed = true
```

---

### Option 4 — What do you need exactly?

**Ivo:**

> Pump. Grow lights. Ventilation. Seed locker if the line holds.  
> I don't need comfort. I need continuity.

**[NYRA]:**

> Forty percent stable output, maybe less if we bypass the upper racks.

**Ivo:**

> I heard “maybe.” I choose to love that word.

**[EFFECT]**

```text
farm_power_requirement_known = true
```

---

## Exit

**Ivo:**

> When you stand at the board, remember this: hunger is patient. That's why people underestimate it.

**[EFFECT]**

```text
talked_to_ivo = true
farm_support = medium
```

---

# 9. D05 — Edda Vale / Public Stage

## Dialogue ID

`D05_EDDA_STAGE`

## Location

Public Stage

## Characters

- Edda Vale
- Dax
- Cormac
- Player

## Conditions

```text
power_priorities_started == true
talked_to_edda == false
```

## Purpose

Сделать вариант сцены убедительным: это не развлечение, а мораль, собрания, новости, политический центр.

---

## Scene Note

Старая сцена в бывшем культурном центре. Пыльный прожектор, потрескавшийся микрофон, детские рисунки, афиша собрания. На стене расписание: water notices, children reading, open council.

---

## Dialogue

**Edda:**

> Mind the third step. It squeals during speeches and lies during tragedies.

**[PLAYER OPTIONS]:**

1. `You're Edda.`
2. `You want power for a stage?`
3. `This place still holds shows?`
4. `We don't have time for theater.`

---

### Option 1 — You're Edda.

**Edda:**

> Some evenings. Other evenings I'm crowd control with better posture.

---

### Option 2 — You want power for a stage?

**Edda:**

> I want power for the room where people remember they're not only stomachs with opinions.

---

### Option 3 — This place still holds shows?

**Edda:**

> Shows, councils, warnings, funerals, lessons, apologies.  
> We discovered after the world ended that all of those need lighting.

---

### Option 4 — We don't have time for theater.

**Edda:**

> Nobody has time for meaning until they lose it. Then they spend the rest of their lives trying to buy it back with canned beans.

**[DAX]:**

> I almost hate that I understood that.

---

## Continue Node

**Edda:**

> Hale will tell you walls keep people alive. Sela will tell you water does. Ivo will nominate lettuce for sainthood.  
> They are all right. That's the boring tragedy.

**[PLAYER OPTIONS]:**

1. `Then why should the stage get power?`
2. `Morale doesn't clean water.`
3. `This stage gives you influence.`
4. `What happens here when the lights come on?`

---

### Option 1 — Then why should the stage get power?

**Edda:**

> Because people who only survive become very efficient at becoming cruel.

**[CORMAC]:**

> Hope doesn't lower fever.

**Edda:**

> No. But it can make a mother bring her child to the clinic before she decides the clinic has nothing left for her.

---

### Option 2 — Morale doesn't clean water.

**Edda:**

> Neither does panic. Yet panic has excellent distribution.

---

### Option 3 — This stage gives you influence.

**Edda:**

> Yes.

**[PLAYER OPTIONS]:**

1. `That's honest.`
2. `That's dangerous.`
3. `You want politics with a spotlight.`

#### 3.1 — That's honest.

**Edda:**

> Honesty is easiest when the accusation is accurate.

#### 3.2 — That's dangerous.

**Edda:**

> So is silence. People fill it with whatever voice scares them least.

#### 3.3 — You want politics with a spotlight.

**Edda:**

> Politics already has a spotlight. Mine just flickers less than Hale's.

**[EFFECT]**

```text
edda_influence_revealed = true
```

---

### Option 4 — What happens here when the lights come on?

**Edda:**

> Children learn letters. Adults hear water rules. Mara gets shouted at in public, which is healthier than being stabbed in private.  
> And sometimes, for ten minutes, people watch someone else suffer beautifully and feel less alone.

**[DAX]:**

> That's a complicated generator request.

**Edda:**

> Simple requests are for simple worlds.

---

## Exit

**Edda:**

> When you choose, remember: darkness doesn't only hide enemies. It makes neighbors into strangers.

**[EFFECT]**

```text
talked_to_edda = true
stage_support = medium
```

---

# 10. D06 — Tomas Greer / Workshop

## Dialogue ID

`D06_GREER_WORKSHOP`

## Location

Workshop

## Characters

- Tomas Greer
- Dax
- Player

## Conditions

```text
power_priorities_started == true
talked_to_greer == false
```

## Purpose

Показать мастерскую как “мультипликатор ремонта”: не решает одну проблему, но позволяет чинить остальные. Показать скрытый мотив: власть через монополию на ремонт.

---

## Scene Note

Мастерская в бывшем гараже. Стена с надписью “WAITING ON POWER”. Список ремонтов: pump relay, UV housing, gate battery, field purifier, radio packs.

---

## Dialogue

**Greer:**

> If you're here to ask what I need, look at the wall. If you're here to ask why, look at anything else.

**[PLAYER OPTIONS]:**

1. `You keep a list.`
2. `Mara says you want the workshop powered.`
3. `What can you fix?`
4. `Dax, thoughts?`

---

### Option 1 — You keep a list.

**Greer:**

> Lists are how civilized people scream.

---

### Option 2 — Mara says you want the workshop powered.

**Greer:**

> Mara says it like I want a warm chair. I want tools that don't die halfway through saving her settlement.

---

### Option 3 — What can you fix?

**Greer:**

> Today? Pump relay, if the coil isn't cracked. Gate battery, if Hale stops breathing on my neck. UV housing, if Sela admits she broke it overtightening the seal.

---

### Option 4 — Dax, thoughts?

**[DAX]:**

> This place gets power, everything else gets easier tomorrow.  
> Also, he knows that, which makes him expensive.

**Greer:**

> I like your scavenger. He insults with measurements.

---

## Continue Node

**Greer:**

> Give Sela power, she gets water until the filter fails.  
> Give Ivo power, he gets crops until the pump fails.  
> Give Hale power, he gets a fence until the battery fails.  
> Give me power, and when they fail, someone can do something besides pray at the casing.

**[PLAYER OPTIONS]:**

1. `You make a good case.`
2. `You also gain control over repairs.`
3. `What do you want in return?`
4. `Why shouldn't the urgent systems come first?`

---

### Option 1 — You make a good case.

**Greer:**

> I make working hinges. Cases are for people with cleaner hands.

---

### Option 2 — You also gain control over repairs.

**Greer:**

> I already have control over repairs. I don't have electricity. That's the difference between power and responsibility.

**[DAX]:**

> That's a slippery sentence.

**Greer:**

> Most useful ones are.

**[EFFECT]**

```text
greer_monopoly_revealed = true
```

---

### Option 3 — What do you want in return?

**Greer:**

> Light for the benches. Stable line for the charger. First pick of scrap from the flooded block.  
> And no speeches while I'm soldering.

---

### Option 4 — Why shouldn't the urgent systems come first?

**Greer:**

> Because urgent is a hole in the floor. It gets bigger every time you step around it.

---

## Exit

**Greer:**

> Choose whatever makes you sleep. But when it breaks, and it will, remember who asked for the light before the smoke.

**[EFFECT]**

```text
talked_to_greer = true
workshop_support = medium
```

---

# 11. D07 — Lysa Marr / Relay Station

## Dialogue ID

`D07_LYSA_RELAY`

## Location

Relay Station

## Characters

- Lysa Marr
- Nyra
- Player

## Conditions

```text
power_priorities_started == true
talked_to_lysa == false
```

## Purpose

Показать релейную станцию как путь к внешнему миру и зацепку про Blind Protocol. Показать, что Lysa одержима сигналом.

---

## Scene Note

Релейная станция почти темная. Терминал мигает остаточным зарядом. На стене старый знак сети. В динамике слышен короткий шум, похожий на дыхание радио.

---

## Dialogue

**Lysa:**

> Don't speak for a second.

**[PLAYER OPTIONS]:**

1. `Excuse me?`
2. `We're here about the relay.`
3. `Are you listening to static?`
4. `Nyra?`

---

### Option 1 — Excuse me?

**Lysa:**

> Static is shy. People aren't.

---

### Option 2 — We're here about the relay.

**Lysa:**

> Everyone is here about something else and then the relay. That's why it keeps waiting.

---

### Option 3 — Are you listening to static?

**Lysa:**

> Static doesn't repeat itself. This does.

---

### Option 4 — Nyra?

**[NYRA]:**

> She's right. There's a carrier under the noise.

**Lysa:**

> Thank you. I was beginning to miss professional fear.

---

## Continue Node

**Lysa:**

> Riverside thinks the relay is for calling help. It might be.  
> Or it might be for hearing what the old network has been saying while nobody had enough power to listen.

**[PLAYER OPTIONS]:**

1. `You mean Blind Protocol.`
2. `Can it contact another settlement?`
3. `Why should this get power before water?`
4. `What are you not telling Mara?`

---

### Option 1 — You mean Blind Protocol.

**Lysa:**

> I mean a phrase that appears in logs it had no reason to be in. I mean priority conflicts. Human override requests. Systems waiting for permission from people who are bones now.

**[NYRA]:**

> Show me the log.

**[EFFECT]**

```text
found_blind_protocol_log = true
unlock lore: Relay Fragment
```

---

### Option 2 — Can it contact another settlement?

**Lysa:**

> Maybe. North line, if the antenna holds. East if the interference clears.  
> Or it contacts whatever used to answer before people started calling silence divine.

---

### Option 3 — Why should this get power before water?

**Lysa:**

> Because water saves Riverside. Signal might save the map.

**[PLAYER]:**

1. `That's abstract.`
2. `That's dangerous.`
3. `That's exactly what Nyra wants to hear.`

#### 3.1 — That's abstract.

**Lysa:**

> So was electricity, once. Then everyone built their lives around it and acted surprised when darkness became political.

#### 3.2 — That's dangerous.

**Lysa:**

> Of course. Answers usually are. That's why people prefer warnings.

#### 3.3 — That's exactly what Nyra wants to hear.

**[NYRA]:**

> I want data. Not comfort.

**Lysa:**

> Then you'll be disappointed less often than most.

---

### Option 4 — What are you not telling Mara?

**Lysa:**

> That the relay pinged last night without power.

**[NYRA]:**

> Impossible.

**Lysa:**

> Good. I was tired of being the only one who hated that word.

**[EFFECT]**

```text
lysa_secret_ping_revealed = true
relay_support = medium
```

---

## Exit

**Lysa:**

> If you give me power, I may hear nothing.  
> But if you don't, we'll never know whether nothing was waiting to answer.

**[EFFECT]**

```text
talked_to_lysa = true
relay_support = medium
```

---

# 12. D08 — Party Reserve Discussion

## Dialogue ID

`D08_PARTY_RESERVE`

## Location

Switch Room / after generator repaired or before final allocation

## Characters

- Dax
- Nyra
- Cormac
- Mara optional
- Player

## Conditions

```text
generator_repaired == true
party_reserve_discussed == false
```

## Purpose

Объяснить Party Reserve: отряд может зарядить переносные аккумуляторы, но это забирает часть ресурса у поселения.

---

## Scene Note

На полу стоят переносные аккумуляторные ячейки отряда. Одна мигает красным. Nyra подключает кабель к панели, но не включает.

---

## Dialogue

**Nyra:**

> We can take a reserve charge.

**[CORMAC]:**

> Define “take.”

**[DAX]:**

> He means “from who.”

**[PLAYER OPTIONS]:**

1. `What would the reserve power?`
2. `How much would Riverside lose?`
3. `Can we do it without telling them?`
4. `No. The power stays here.`

---

### Option 1 — What would the reserve power?

**Nyra:**

> Field purifier. Scanner. Radio pack. Maybe the terminal beyond the north bridge, if Lysa's log isn't a ghost story.

**[DAX]:**

> My toolkit too. Unless everyone prefers opening sealed doors with optimism.

**Cormac:**

> Medical cooler. One emergency cycle. Maybe two if we don't waste light pretending we're not making a choice.

**[EFFECT]**

```text
party_reserve_benefits_known = true
```

---

### Option 2 — How much would Riverside lose?

**Nyra:**

> Ten percent for minimum charge. Twenty-five for full.  
> In human terms, ask the board which argument gets quieter.

**[CORMAC]:**

> That's not human terms.

**Nyra:**

> I know.

---

### Option 3 — Can we do it without telling them?

**Nyra:**

> Technically, yes.

**[CORMAC]:**

> That's a poor sentence to end on.

**[DAX]:**

> She didn't end. She paused for ethics.

**Nyra:**

> Ethics says no. Survival says ask again later.

**[EFFECT]**

```text
party_reserve_can_be_hidden = true
cormac_approval = down_small
```

---

### Option 4 — No. The power stays here.

**Cormac:**

> Good.

**Dax:**

> Noble. Hope the next dark road appreciates it.

**Nyra:**

> We may lose access to the bridge terminal.

**[EFFECT]**

```text
party_reserve_rejected_initially = true
cormac_approval = up_small
```

---

## Continue Node

**[PLAYER OPTIONS]:**

1. `Minimum reserve might be justified.`
2. `Full reserve is too much.`
3. `If it leads to Blind Protocol, it may be worth it.`
4. `We'll decide at the board.`

---

### Option 1 — Minimum reserve might be justified.

**Cormac:**

> Justified is what people call selfish when it works.

**Dax:**

> And stupid when it doesn't.

**Nyra:**

> Ten percent could keep us alive long enough to regret it properly.

---

### Option 2 — Full reserve is too much.

**Dax:**

> Full reserve is what you take when you don't plan to be invited back.

---

### Option 3 — If it leads to Blind Protocol, it may be worth it.

**Nyra:**

> That's the argument.

**Cormac:**

> No. That's the temptation wearing a lab coat.

---

### Option 4 — We'll decide at the board.

**Dax:**

> The board won't make it easier. It just makes the guilt better organized.

---

## Exit

**Nyra:**

> I'll leave the cells connected but idle. Your call.

**[EFFECT]**

```text
party_reserve_discussed = true
unlock PowerChoice: PartyReserve
```

---

# 13. D09 — Rook Pre-Combat Contact

## Dialogue ID

`D09_ROOK_PRECOMBAT`

## Location

Generator Yard / radio or shouted from darkness

## Characters

- Rook
- Mara
- Dax
- Player

## Conditions

```text
generator_started == true
rook_contacted == false
```

## Purpose

Показать рейдеров как философскую угрозу, а не просто врагов: они считают, что ресурс должен быть у тех, кто может его удержать.

---

## Dialogue

**Rook:**

> Light travels. That's the problem with it. You turn it on, and honest people come to see who lied about owning it.

**[DAX]:**

> Ashline.

**Mara:**

> Rook.

**Rook:**

> Mara. Still holding meetings over dying machines?

**Mara:**

> Still mistaking theft for philosophy?

**Rook:**

> Philosophy is what people call theft after they build a fence around it.

**[PLAYER OPTIONS]:**

1. `Walk away, Rook.`
2. `What do you want?`
3. `You could negotiate.`
4. `Come take it, then.`

---

### Option 1 — Walk away, Rook.

**Rook:**

> Everyone says that from behind someone else's wall.

---

### Option 2 — What do you want?

**Rook:**

> The same thing they want. The same thing you want.  
> Power, with fewer speeches.

---

### Option 3 — You could negotiate.

**Rook:**

> Negotiation is when both sides think they might lose. Riverside already lost. They just keep voting on the order.

---

### Option 4 — Come take it, then.

**Rook:**

> Thank you. I prefer clear government.

---

## Continue Node

**Rook:**

> Last chance. Leave the battery stack. Take your boots and your moral injuries.  
> Stay, and you defend people who will blame you no matter what you choose.

**[PLAYER OPTIONS]:**

1. `We stay.`
2. `This generator stays with Riverside.`
3. `You're afraid they'll become stronger.`
4. `Enough talking.`

---

### Option 1 — We stay.

**Rook:**

> Then stand close to the light. Makes aiming easier.

---

### Option 2 — This generator stays with Riverside.

**Rook:**

> Generators don't stay with anyone. People stay near generators until better armed people arrive.

---

### Option 3 — You're afraid they'll become stronger.

**Rook:**

> I'm afraid they'll become hopeful. Strong people can be killed. Hopeful people make poor calculations.

---

### Option 4 — Enough talking.

**Rook:**

> Sensible. Words waste ammunition differently.

---

## Exit

**[EFFECT]**

```text
rook_contacted = true
start combat encounter
```

---

# 14. Combat Barks

## 14.1. Generator Start

**Nyra:**

> Output rising. Not stable, but alive.

**Dax:**

> Alive is generous. It sounds offended.

**Cormac:**

> Can machines be offended?

**Dax:**

> This one can.

---

## 14.2. Raiders Appear

**Rook:**

> There it is. The sound of people becoming optimistic.

**Mara:**

> Positions!

**Hale, if present:**

> Now we discuss defense.

---

## 14.3. Enemy flanking

**Dax:**

> Right side. Someone's trying to be clever.

**Nyra:**

> Failing, but moving fast.

---

## 14.4. Saboteur near generator

**Nyra:**

> One on the generator line!

**Dax:**

> Touch that cable and I start taking this personally.

**Cormac:**

> You weren't already?

**Dax:**

> I was professionally annoyed.

---

## 14.5. Generator damaged

**Dax:**

> G-17 just lost pressure.

**Nyra:**

> That's not pressure. That's thermal drift.

**Dax:**

> Great. It's dying in educated words.

---

## 14.6. Cormac heals

**Cormac:**

> Hold still.

**Dax:**

> In a gunfight?

**Cormac:**

> Make it a hobby.

---

## 14.7. Nyra Shock Pulse

**Nyra:**

> Discharging.

**Dax:**

> Friendly warning?

**Nyra:**

> For us.

---

## 14.8. Enemy defeated

**Rook's Raider:**

> Light's not worth this!

**Dax:**

> First honest energy policy I've heard today.

---

## 14.9. Combat Victory

**Nyra:**

> Generator survived.

**Dax:**

> I want that noted as a personal favor from me to civilization.

**Cormac:**

> People are still alive.

**Dax:**

> Fine. Put that first.

---

# 15. D10 — Power Allocation Confirmation

## Dialogue ID

`D10_POWER_ALLOCATION_CONFIRMATION`

## Location

Switch Room

## Characters

- Mara
- Dax
- Nyra
- Cormac
- Player

## Conditions

```text
generator_defended == true
final_power_choice == None
```

## Purpose

Предфинальное напряжение перед выбором энергии.

---

## Dialogue

**Mara:**

> The board is live.

**[NYRA]:**

> Stable output is lower than promised.

**Mara:**

> Promises age poorly here.

**[DAX]:**

> We can route one major system. Maybe one minor if we enjoy electrical fires.

**[CORMAC]:**

> Everyone outside knows?

**Mara:**

> Everyone outside has already decided what kind of person you are. Now you get to disappoint them accurately.

**[PLAYER OPTIONS]:**

1. `Open the board.`
2. `Any last advice?`
3. `What would you choose, Mara?`
4. `Can we still walk away?`

---

### Option 1 — Open the board.

**Mara:**

> Then choose cleanly. Dirty choices get worse when you pretend they're clean.

**[EFFECT]**

```text
open Power Allocation Board
```

---

### Option 2 — Any last advice?

**Mara:**

> Don't choose the option that makes you feel innocent. We don't have one.

---

### Option 3 — What would you choose, Mara?

**Mara:**

> On a good day? Water.  
> On a frightened day? The gate.  
> On a day when I remember being young? The stage.  
> That's why I'm not touching the board.

---

### Option 4 — Can we still walk away?

**Mara:**

> Yes. That's the cruel thing about doors. They work both ways.

---

# 16. Final Reaction — Water Filters

## Dialogue ID

`D11_FINAL_WATER_FILTERS`

## Conditions

```text
final_power_choice == WaterFilters
```

---

**System Message:**

> Power routed to Water Filters.

**Sela:**

> Listen.

**[PLAYER]:**

> To what?

**Sela:**

> The pump.  
> That sound means tomorrow's water doesn't need an apology.

**Cormac:**

> Good choice.

**Ivo:**

> Good for the morning.

**Sela:**

> People keep dismissing mornings until they stop getting them.

**Mara:**

> Riverside will remember this. The thirsty part of it, at least.

**[EFFECT]**

```text
riverside_water_status = clean
medical_support = high
farm_support = low
riverside_trust = high
cormac_approval = up
```

---

# 17. Final Reaction — Hydroponic Farm

## Dialogue ID

`D12_FINAL_HYDROPONIC_FARM`

## Conditions

```text
final_power_choice == HydroponicFarm
```

---

**System Message:**

> Power routed to Hydroponic Farm.

**Ivo:**

> Wait. Hear that?

**[DAX]:**

> Pump.

**Ivo:**

> No. Future, badly maintained.

**Sela:**

> I have patients who needed the present.

**Ivo:**

> And I have children who will need next month.

**Cormac:**

> Both of you are right. That's the part that makes this ugly.

**Mara:**

> Riverside will eat later. If it lasts long enough to be grateful.

**[EFFECT]**

```text
riverside_food_status = stable
farm_support = high
medical_support = low
riverside_trust = medium
nyra_approval = up_small
```

---

# 18. Final Reaction — Defense Grid

## Dialogue ID

`D13_FINAL_DEFENSE_GRID`

## Conditions

```text
final_power_choice == DefenseGrid
```

---

**System Message:**

> Power routed to Defense Grid.

**Hale:**

> Floodlights online. Fence charge rising.

**Edda:**

> Wonderful. Now we can see our fear more clearly.

**Hale:**

> Fear with visibility is called preparation.

**Mara:**

> Hale gets his wall.

**Hale:**

> Riverside gets a night.

**Cormac:**

> And the sick?

**Hale:**

> They get to be sick behind a locked gate.

**[EFFECT]**

```text
riverside_defense_status = strong
defense_support = high
stage_support = low
medical_support = low
hale_influence = high
riverside_trust = medium
```

---

# 19. Final Reaction — Public Stage

## Dialogue ID

`D14_FINAL_PUBLIC_STAGE`

## Conditions

```text
final_power_choice == PublicStage
```

---

**System Message:**

> Power routed to Public Stage.

**Edda:**

> Don't look so surprised. A light over people is still a light.

**Hale:**

> Ashline will be moved by the performance.

**Edda:**

> They might. If they had better seats.

**Cormac:**

> People are coming in.

**Mara:**

> Of course they are. When a room lights up, everyone wants to know what kind of truth is being sold.

**Edda:**

> Not sold. Shared.  
> Usually after being argued with.

**Dax:**

> I give it ten minutes before someone mentions water.

**Edda:**

> Good. Then the light is working.

**[EFFECT]**

```text
riverside_morale = high
stage_support = high
defense_support = low
riverside_trust = medium
future_social_quests = unlocked
```

---

# 20. Final Reaction — Workshop

## Dialogue ID

`D15_FINAL_WORKSHOP`

## Conditions

```text
final_power_choice == Workshop
```

---

**System Message:**

> Power routed to Workshop.

**Greer:**

> Finally.

**Dax:**

> That's gratitude?

**Greer:**

> That's efficiency. Gratitude takes longer and fixes less.

**Sela:**

> I assume the filter gets first repair.

**Greer:**

> The filter, the gate battery, the pump relay, and whatever Dax broke by looking at it.

**Dax:**

> I respect a man who insults in work orders.

**Mara:**

> You gave Riverside tools. Now we find out who gets to hold them.

**[EFFECT]**

```text
workshop_support = high
upgrade_access = unlocked
dax_approval = up
medical_support = medium_low
farm_support = medium_low
riverside_trust = medium
```

---

# 21. Final Reaction — Relay Station

## Dialogue ID

`D16_FINAL_RELAY_STATION`

## Conditions

```text
final_power_choice == RelayStation
```

---

**System Message:**

> Power routed to Relay Station.

**Lysa:**

> Carrier locked.

**Nyra:**

> That's not local.

**Mara:**

> Tell me it's help.

**Lysa:**

> I can't.

**Dax:**

> That's a terrible start.

**Relay Voice:**

> ...Blind Protocol active...  
> ...priority conflict unresolved...  
> ...human override required...

**Cormac:**

> Human override for what?

**Nyra:**

> That's the question.

**Mara:**

> No. The question is whether the answer was worth the water we didn't clean.

**[EFFECT]**

```text
riverside_signal_status = online
relay_support = high
found_blind_protocol_log = true
new_route_unlocked = true
nyra_approval = up
medical_support = low
farm_support = low
riverside_trust = low_medium
```

---

# 22. Final Reaction — Party Reserve

## Dialogue ID

`D17_FINAL_PARTY_RESERVE`

## Conditions

```text
final_power_choice == PartyReserve
```

---

**System Message:**

> Power routed to Party Reserve.

**Portable Cells:**

> Charge cycle started.

**Mara:**

> That's a clean phrase for taking light with you.

**Dax:**

> It's enough to keep us alive past the north bridge.

**Cormac:**

> And what did it cost them?

**Nyra:**

> Maybe the answer to why the bridge matters.

**Mara:**

> Answers are easy to carry. People are heavier.

**[PLAYER OPTIONS]:**

1. `We'll use it to help more people.`
2. `We need it to survive.`
3. `There may be data about the Catastrophe.`
4. `Say what you want. The choice is made.`

---

### Option 1 — We'll use it to help more people.

**Mara:**

> That's what every departing hero says. The ones who return usually say less.

**[REL]**

```text
riverside_trust = low_medium
```

---

### Option 2 — We need it to survive.

**Mara:**

> Honest, at least. Ugly, but honest.

**[DAX]:**

> Ugly gets you further than dead.

**[REL]**

```text
dax_approval = up
cormac_approval = down
```

---

### Option 3 — There may be data about the Catastrophe.

**Nyra:**

> There is a terminal beyond the bridge. If it holds a Blind Protocol fragment—

**Cormac:**

> Then we bought a maybe with their water.

**[REL]**

```text
nyra_approval = up
cormac_approval = down
```

---

### Option 4 — Say what you want. The choice is made.

**Mara:**

> It is. That's the one mercy of bad decisions. They stop asking permission.

**[REL]**

```text
riverside_trust = low
```

---

## Exit

**Cormac:**

> Remember what we spend it on.

**Dax:**

> I remember everything that keeps me alive.

**Cormac:**

> That's not the same thing.

**[EFFECT]**

```text
party_power_cells = charged
party_selfishness_seen = true
riverside_trust = low
cormac_approval = down
```

---

# 23. Final Reaction — Split Load

## Dialogue ID

`D18_FINAL_SPLIT_LOAD`

## Conditions

```text
final_power_choice == SplitLoad
```

---

**System Message:**

> Power split across multiple systems. Grid strain detected.

**Nyra:**

> It's holding.

**Dax:**

> That's not a diagnosis. That's a prayer with numbers.

**Mara:**

> Filters?

**Sela:**

> Weak flow.

**Ivo:**

> Farm lights at half.

**Hale:**

> Fence won't stop a determined dog.

**Edda:**

> But the hall has enough light for people to see each other complain.

**Greer:**

> Everything works just well enough to break later.

**Mara:**

> Then Riverside remains itself.

**[EFFECT]**

```text
riverside_water_status = limited
riverside_food_status = unstable
riverside_signal_status = weak
riverside_morale = medium
overload_risk = true
riverside_trust = medium_high
```

---

# 24. Post-Mission Mara Exit

## Dialogue ID

`D19_MARA_EXIT`

## Conditions

```text
mission_completed == true
```

---

**Mara:**

> You should leave before people decide whether to thank you or practice hating you.

**[PLAYER OPTIONS]:**

1. `Will Riverside hold?`
2. `Do you think we chose wrong?`
3. `We may come back.`
4. `Good luck, Mara.`

---

### Option 1 — Will Riverside hold?

**Mara:**

> Riverside is held together by bad wiring and stubborn people.  
> You improved one of those. Maybe.

---

### Option 2 — Do you think we chose wrong?

**Mara:**

> I think wrong is a word people use when they can still imagine clean choices.

---

### Option 3 — We may come back.

**Mara:**

> Then bring parts. Or water. Or a story that doesn't end with “we did our best.”

---

### Option 4 — Good luck, Mara.

**Mara:**

> Luck is what people call infrastructure when they don't see the maintenance.

---

## Exit Line

**Mara:**

> Follow the north road until the lamps stop. After that, count your batteries and don't trust quiet water.

**[EFFECT]**

```text
mission_completed = true
unlock next route: North Bridge
```

---

# 25. Dialogue Implementation Notes for Codex

## 25.1. Minimum first implementation

Для первой версии Dialogue System не нужно реализовывать все ветки сразу.

Минимум:

- Guard entrance;
- Mara intro;
- 4 NPC priorities:
  - Hale;
  - Sela;
  - Ivo;
  - Edda;
- Party Reserve;
- Rook pre-combat;
- Final reactions:
  - Water;
  - Farm;
  - Defense;
  - Stage;
  - Party Reserve;
  - Split Load.

Greer и Lysa можно добавить во второй итерации, если нужно сократить scope.

---

## 25.2. DialogueData conversion

Каждый диалог должен быть конвертируем в структуру:

```text
DialogueData
  Id
  Nodes[]
    NodeId
    SpeakerId
    Text
    Choices[]
      Text
      NextNodeId
      Conditions[]
      Effects[]
```

---

## 25.3. Suggested speaker IDs

```text
player
dax
nyra
cormac
mara
hale
ivo
sela
edda
greer
lysa
rook
guard
system
relay_voice
portable_cells
```

---

## 25.4. Suggested effects

```text
SetFlag(flagName, true/false)
SetRelationship(group, level)
ChangeApproval(character, amount)
StartQuest(questId)
CompleteObjective(objectiveId)
UnlockMarker(markerId)
UnlockPowerChoice(choiceId)
OpenPowerAllocationBoard()
StartCombat(encounterId)
CompleteMission()
```

---

# 26. Краткое резюме

Диалоги Riverside District должны превращать миссию из технического задания “почини генератор” в живую драму поселения.

Каждый персонаж предлагает не просто систему для питания, а свою модель будущего:

- Hale — выжить через безопасность;
- Sela — выжить через чистую воду;
- Ivo — выжить через еду;
- Edda — выжить через общность и смысл;
- Greer — выжить через ремонт;
- Lysa — выжить через связь и правду;
- отряд — выжить через собственный резерв.

Игрок не выбирает правильный вариант.

Игрок выбирает, чья правда получит свет.
