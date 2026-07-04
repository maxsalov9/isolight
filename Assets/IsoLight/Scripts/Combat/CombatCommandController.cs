using System;
using System.Collections.Generic;
using IsoLight.Characters;
using IsoLight.Core;
using IsoLight.Enemies;
using IsoLight.Party;
using IsoLight.Power;
using IsoLight.UI;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace IsoLight.Combat
{
    public class CombatCommandController : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private PartyManager partyManager;
        [SerializeField] private CombatManager combatManager;
        [SerializeField] private AbilityController abilityController;
        [SerializeField] private GeneratorG17 generator;
        [SerializeField] private NotificationUI notificationUI;
        [SerializeField] private UnityEngine.Camera raycastCamera;
        [SerializeField] private LayerMask raycastMask = Physics.DefaultRaycastLayers;
        [SerializeField] private float raycastDistance = 1000f;

        private readonly Dictionary<PlayerCharacter, CombatCommand> commands = new Dictionary<PlayerCharacter, CombatCommand>();
        private readonly string[] hotkeys = { "Q", "E", "R" };
        private PlayerCharacter targetingCaster;
        private AbilityData targetingAbility;
        private int targetingSlotIndex = -1;
        private string targetModeHint;

        public bool IsAbilityTargetModeActive => targetingAbility != null && targetingCaster != null;
        public string TargetingAbilityName => targetingAbility != null ? targetingAbility.DisplayName : string.Empty;
        public string TargetModeHint => targetModeHint;
        public PlayerCharacter TargetingCaster => targetingCaster;
        public AbilityData TargetingAbility => targetingAbility;
        public bool CanHandleCombatInput => combatManager != null && combatManager.IsCombatActive;

        public void SetReferences(
            GameManager game,
            PartyManager party,
            CombatManager combat,
            AbilityController abilities,
            GeneratorG17 targetGenerator,
            NotificationUI notifications,
            UnityEngine.Camera camera)
        {
            gameManager = game;
            partyManager = party;
            combatManager = combat;
            abilityController = abilities;
            generator = targetGenerator;
            notificationUI = notifications;
            raycastCamera = camera;
        }

        private void Awake()
        {
            CacheReferences();
        }

        private void Update()
        {
            CacheReferences();
            HandlePauseInput();

            if (!CanHandleCombatInput)
            {
                CancelAbilityTargetMode(false);
                commands.Clear();
                return;
            }

            HandleAbilityHotkeys();
            HandleCancelInput();

            if (IsAbilityTargetModeActive)
            {
                HandleAbilityTargetClick();
            }
            else
            {
                HandleCombatClicks();
            }

            if (!combatManager.IsTacticalPaused)
            {
                ExecuteCommands();
            }
        }

        public void BeginOrToggleAbilityTargetMode(int slotIndex)
        {
            CacheReferences();
            var caster = partyManager != null ? partyManager.ActiveCharacter : null;
            if (caster == null)
            {
                notificationUI?.ShowMessage("Нет активного персонажа.");
                return;
            }

            var abilities = caster.Abilities;
            if (slotIndex < 0 || slotIndex >= abilities.Count || abilities[slotIndex] == null)
            {
                notificationUI?.ShowMessage("Способность не назначена.");
                return;
            }

            var ability = abilities[slotIndex];
            if (IsAbilityTargetModeActive && targetingCaster == caster && targetingAbility == ability)
            {
                CancelAbilityTargetMode(true);
                return;
            }

            if (!abilityController.CanBeginTargetMode(caster, ability, out var reason))
            {
                notificationUI?.ShowMessage(reason);
                return;
            }

            targetingCaster = caster;
            targetingAbility = ability;
            targetingSlotIndex = slotIndex;
            targetModeHint = $"Выберите цель для: {ability.DisplayName}";
            notificationUI?.ShowMessage(targetModeHint);
        }

        public string GetCommandLabel(PlayerCharacter character)
        {
            return character != null && commands.TryGetValue(character, out var command)
                ? command.Label
                : string.Empty;
        }

        public Enemy GetAutoAttackTarget(PlayerCharacter character)
        {
            return character != null
                && commands.TryGetValue(character, out var command)
                && command.Type == CombatCommandType.AutoAttack
                ? command.EnemyTarget
                : null;
        }

        public IReadOnlyDictionary<PlayerCharacter, CombatCommand> Commands => commands;

        private void HandlePauseInput()
        {
            if (combatManager == null || !combatManager.IsCombatActive || !WasKeyPressed(KeyCode.Space))
            {
                return;
            }

            combatManager.ToggleTacticalPause();
            notificationUI?.ShowMessage(combatManager.IsTacticalPaused
                ? "Тактическая пауза. Назначьте команды и нажмите Space, чтобы продолжить."
                : "Тактическая пауза снята. Команды выполняются.");
        }

        private void HandleAbilityHotkeys()
        {
            for (var i = 0; i < hotkeys.Length; i++)
            {
                if (WasAbilityPressed(i))
                {
                    BeginOrToggleAbilityTargetMode(i);
                    return;
                }
            }
        }

        private void HandleCancelInput()
        {
            if (IsAbilityTargetModeActive && WasKeyPressed(KeyCode.Escape))
            {
                CancelAbilityTargetMode(true);
            }
        }

        private void HandleAbilityTargetClick()
        {
            if (!WasLeftMousePressedThisFrame() || IsPointerOverUi())
            {
                return;
            }

            if (!TryRaycastPointer(out var hit))
            {
                notificationUI?.ShowMessage("Цель не выбрана.");
                return;
            }

            var target = ResolveAbilityTarget(hit.collider);
            if (target == null)
            {
                notificationUI?.ShowMessage("Нет подходящей цели.");
                return;
            }

            var preview = abilityController.GetAbilityPreviewForTarget(targetingCaster, targetingAbility, target);
            if (!preview.IsUsable)
            {
                notificationUI?.ShowMessage(preview.UnavailableReason);
                return;
            }

            if (combatManager.IsTacticalPaused)
            {
                QueueCommand(targetingCaster, CombatCommand.UseAbility(targetingAbility, target));
                notificationUI?.ShowMessage($"{targetingCaster.DisplayName}: {targetingAbility.DisplayName} назначен. Нажмите Space, чтобы продолжить.");
            }
            else if (!abilityController.TryUseAbility(targetingCaster, targetingAbility, target, out var failureReason))
            {
                notificationUI?.ShowMessage(failureReason);
            }

            CancelAbilityTargetMode(false);
        }

        private Component ResolveAbilityTarget(Collider hitCollider)
        {
            if (hitCollider == null || targetingAbility == null)
            {
                return null;
            }

            return targetingAbility.EffectType switch
            {
                AbilityEffectType.RepairGenerator => hitCollider.GetComponentInParent<GeneratorG17>(),
                AbilityEffectType.HealAlly => hitCollider.GetComponentInParent<PlayerCharacter>(),
                AbilityEffectType.StabilizeAlly => hitCollider.GetComponentInParent<PlayerCharacter>(),
                AbilityEffectType.DamageEnemy => hitCollider.GetComponentInParent<Enemy>(),
                AbilityEffectType.ShockEnemy => hitCollider.GetComponentInParent<Enemy>(),
                _ => null
            };
        }

        private void HandleCombatClicks()
        {
            if (IsPointerOverUi() || (!WasLeftMousePressedThisFrame() && !WasRightMousePressedThisFrame()))
            {
                return;
            }

            if (!TryRaycastPointer(out var hit))
            {
                return;
            }

            var character = partyManager != null ? partyManager.ActiveCharacter : null;
            if (character == null || !character.IsAlive)
            {
                return;
            }

            var enemy = hit.collider.GetComponentInParent<Enemy>();
            if (enemy != null && enemy.IsAlive)
            {
                if (WasRightMousePressedThisFrame())
                {
                    QueueCommand(character, CombatCommand.AutoAttack(enemy));
                    combatManager.SetFocusedEnemy(enemy);
                    NotifyCommand(character, $"автоатака по {enemy.name}");
                }
                else
                {
                    QueueCommand(character, CombatCommand.SingleAttack(enemy));
                    combatManager.SetFocusedEnemy(enemy);
                    NotifyCommand(character, $"атака по {enemy.name}");
                }

                return;
            }

            if (WasLeftMousePressedThisFrame())
            {
                QueueCommand(character, CombatCommand.MoveTo(hit.point));
                NotifyCommand(character, "движение");
            }
        }

        private void QueueCommand(PlayerCharacter character, CombatCommand command)
        {
            if (character == null || command == null)
            {
                return;
            }

            commands[character] = command;
        }

        private void NotifyCommand(PlayerCharacter character, string actionText)
        {
            if (character == null)
            {
                return;
            }

            notificationUI?.ShowMessage(combatManager != null && combatManager.IsTacticalPaused
                ? $"{character.DisplayName}: {actionText}. Команда назначена. Нажмите Space, чтобы продолжить."
                : $"{character.DisplayName}: {actionText}.");
        }

        private void ExecuteCommands()
        {
            if (partyManager?.PartyMembers == null)
            {
                return;
            }

            for (var i = 0; i < partyManager.PartyMembers.Count; i++)
            {
                var character = partyManager.PartyMembers[i];
                if (character == null || !character.IsAlive || !commands.TryGetValue(character, out var command))
                {
                    continue;
                }

                ExecuteCommand(character, command);
            }
        }

        private void ExecuteCommand(PlayerCharacter character, CombatCommand command)
        {
            switch (command.Type)
            {
                case CombatCommandType.MoveTo:
                    if (Vector3.Distance(character.transform.position, command.Destination) <= 0.45f)
                    {
                        commands.Remove(character);
                    }
                    else
                    {
                        character.MoveTo(command.Destination);
                    }
                    break;

                case CombatCommandType.SingleAttack:
                    if (command.EnemyTarget == null || !command.EnemyTarget.IsAlive)
                    {
                        commands.Remove(character);
                    }
                    else if (character.TryAttack(command.EnemyTarget))
                    {
                        commands.Remove(character);
                    }
                    break;

                case CombatCommandType.AutoAttack:
                    if (command.EnemyTarget == null || !command.EnemyTarget.IsAlive)
                    {
                        commands.Remove(character);
                    }
                    else
                    {
                        character.TryAttack(command.EnemyTarget);
                    }
                    break;

                case CombatCommandType.UseAbility:
                    if (!abilityController.TryUseAbility(character, command.Ability, command.AbilityTarget, out var failureReason))
                    {
                        notificationUI?.ShowMessage(failureReason);
                    }

                    commands.Remove(character);
                    break;
            }
        }

        private void CancelAbilityTargetMode(bool notify)
        {
            if (notify && IsAbilityTargetModeActive)
            {
                notificationUI?.ShowMessage("Выбор цели отменен.");
            }

            targetingCaster = null;
            targetingAbility = null;
            targetingSlotIndex = -1;
            targetModeHint = null;
        }

        private bool TryRaycastPointer(out RaycastHit hit)
        {
            hit = default;
            if (raycastCamera == null)
            {
                raycastCamera = UnityEngine.Camera.main;
            }

            if (raycastCamera == null)
            {
                return false;
            }

            var ray = raycastCamera.ScreenPointToRay(GetPointerPosition());
            return Physics.Raycast(ray, out hit, raycastDistance, raycastMask, QueryTriggerInteraction.Ignore);
        }

        private void CacheReferences()
        {
            if (gameManager == null) gameManager = FindAnyObjectByType<GameManager>();
            if (partyManager == null) partyManager = FindAnyObjectByType<PartyManager>();
            if (combatManager == null) combatManager = FindAnyObjectByType<CombatManager>();
            if (abilityController == null) abilityController = FindAnyObjectByType<AbilityController>();
            if (generator == null) generator = FindAnyObjectByType<GeneratorG17>();
            if (notificationUI == null) notificationUI = FindAnyObjectByType<NotificationUI>();
            if (raycastCamera == null) raycastCamera = UnityEngine.Camera.main;
        }

        private static bool WasAbilityPressed(int slotIndex)
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return false;
            }

            return slotIndex switch
            {
                0 => keyboard.qKey.wasPressedThisFrame,
                1 => keyboard.eKey.wasPressedThisFrame,
                2 => keyboard.rKey.wasPressedThisFrame,
                _ => false
            };
#else
            return slotIndex switch
            {
                0 => UnityEngine.Input.GetKeyDown(KeyCode.Q),
                1 => UnityEngine.Input.GetKeyDown(KeyCode.E),
                2 => UnityEngine.Input.GetKeyDown(KeyCode.R),
                _ => false
            };
#endif
        }

        private static bool WasLeftMousePressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
            return UnityEngine.Input.GetMouseButtonDown(0);
#endif
        }

        private static bool WasRightMousePressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
#else
            return UnityEngine.Input.GetMouseButtonDown(1);
#endif
        }

        private static bool WasKeyPressed(KeyCode keyCode)
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return false;
            }

            return keyCode switch
            {
                KeyCode.Space => keyboard.spaceKey.wasPressedThisFrame,
                KeyCode.Escape => keyboard.escapeKey.wasPressedThisFrame,
                _ => false
            };
#else
            return UnityEngine.Input.GetKeyDown(keyCode);
#endif
        }

        private static Vector3 GetPointerPosition()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector3.zero;
#else
            return UnityEngine.Input.mousePosition;
#endif
        }

        private static bool IsPointerOverUi()
        {
            var eventSystemType = Type.GetType("UnityEngine.EventSystems.EventSystem, UnityEngine.UI");
            var currentProperty = eventSystemType?.GetProperty("current");
            var currentEventSystem = currentProperty?.GetValue(null);
            var isPointerOverMethod = eventSystemType?.GetMethod("IsPointerOverGameObject", Type.EmptyTypes);

            return currentEventSystem != null
                && isPointerOverMethod != null
                && (bool)isPointerOverMethod.Invoke(currentEventSystem, null);
        }
    }
}
