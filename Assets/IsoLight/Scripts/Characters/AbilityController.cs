using IsoLight.Combat;
using IsoLight.Core;
using IsoLight.Enemies;
using IsoLight.Party;
using IsoLight.Power;
using IsoLight.UI;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace IsoLight.Characters
{
    public class AbilityController : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private PartyManager partyManager;
        [SerializeField] private CombatManager combatManager;
        [SerializeField] private GeneratorG17 generator;
        [SerializeField] private NotificationUI notificationUI;

        public void SetReferences(GameManager game, PartyManager party, CombatManager combat, GeneratorG17 targetGenerator, NotificationUI notifications)
        {
            gameManager = game;
            partyManager = party;
            combatManager = combat;
            generator = targetGenerator;
            notificationUI = notifications;
        }

        private void Awake()
        {
            CacheReferences();
        }

        private void Update()
        {
            CacheReferences();

            if (WasAbilityPressed(0))
            {
                UseAbilitySlot(0);
            }
            else if (WasAbilityPressed(1))
            {
                UseAbilitySlot(1);
            }
            else if (WasAbilityPressed(2))
            {
                UseAbilitySlot(2);
            }
        }

        public void UseAbilitySlot(int slotIndex)
        {
            CacheReferences();

            var caster = partyManager != null ? partyManager.ActiveCharacter : null;
            if (caster == null)
            {
                notificationUI?.ShowMessage("Нет активного персонажа.");
                return;
            }

            if (gameManager == null || combatManager == null || !combatManager.IsCombatActive || gameManager.CurrentGameMode != GameMode.Combat)
            {
                notificationUI?.ShowMessage("Способности доступны только во время боя.");
                return;
            }

            var abilities = caster.Abilities;
            if (slotIndex < 0 || slotIndex >= abilities.Count || abilities[slotIndex] == null)
            {
                notificationUI?.ShowMessage("Способность не назначена.");
                return;
            }

            var ability = abilities[slotIndex];
            if (!HasValidTarget(caster, ability, out var targetDescription))
            {
                notificationUI?.ShowMessage(targetDescription);
                return;
            }

            if (!caster.TryBeginAbility(ability, out var failureReason))
            {
                notificationUI?.ShowMessage(failureReason);
                return;
            }

            ApplyAbility(caster, ability);
        }

        private bool HasValidTarget(PlayerCharacter caster, AbilityData ability, out string failureReason)
        {
            failureReason = "Нет подходящей цели.";

            switch (ability.EffectType)
            {
                case AbilityEffectType.RepairGenerator:
                    if (generator == null || !generator.IsAlive || generator.CurrentHealth >= generator.MaxHealth)
                    {
                        failureReason = "Generator G-17 не нуждается в ремонте.";
                        return false;
                    }

                    if (!IsInRange(caster.transform.position, generator.transform.position, ability.Range))
                    {
                        failureReason = "Generator G-17 слишком далеко.";
                        return false;
                    }

                    return true;

                case AbilityEffectType.HealAlly:
                case AbilityEffectType.StabilizeAlly:
                    var ally = FindMostInjuredAlly(caster, ability.Range);
                    if (ally == null)
                    {
                        failureReason = "Нет раненого союзника в зоне.";
                        return false;
                    }

                    return true;

                case AbilityEffectType.DamageEnemy:
                case AbilityEffectType.ShockEnemy:
                    var enemy = FindEnemyTarget(caster, ability.Range);
                    if (enemy == null)
                    {
                        failureReason = "Нет врага в зоне.";
                        return false;
                    }

                    return true;

                default:
                    return false;
            }
        }

        private void ApplyAbility(PlayerCharacter caster, AbilityData ability)
        {
            switch (ability.EffectType)
            {
                case AbilityEffectType.RepairGenerator:
                    generator.RepairHealth(ability.PowerAmount);
                    notificationUI?.ShowMessage($"{caster.DisplayName} использует {ability.DisplayName}. Generator G-17: {generator.CurrentHealth}/{generator.MaxHealth} HP.");
                    break;

                case AbilityEffectType.DamageEnemy:
                    var damageTarget = FindEnemyTarget(caster, ability.Range);
                    damageTarget?.TakeDamage(ability.PowerAmount);
                    notificationUI?.ShowMessage($"{caster.DisplayName} использует {ability.DisplayName}.");
                    break;

                case AbilityEffectType.ShockEnemy:
                    var shockTarget = FindEnemyTarget(caster, ability.Range);
                    if (shockTarget != null)
                    {
                        shockTarget.TakeDamage(ability.PowerAmount);
                        shockTarget.GetComponent<EnemyAI>()?.StunFor(ability.StunDuration);
                    }

                    notificationUI?.ShowMessage($"{caster.DisplayName} применяет {ability.DisplayName}.");
                    break;

                case AbilityEffectType.HealAlly:
                    var healTarget = FindMostInjuredAlly(caster, ability.Range);
                    healTarget?.Heal(ability.PowerAmount);
                    notificationUI?.ShowMessage($"{caster.DisplayName} лечит союзника.");
                    break;

                case AbilityEffectType.StabilizeAlly:
                    var stabilizeTarget = FindMostInjuredAlly(caster, ability.Range);
                    stabilizeTarget?.Heal(ability.PowerAmount);
                    notificationUI?.ShowMessage($"{caster.DisplayName} стабилизирует союзника.");
                    break;
            }
        }

        private Enemy FindEnemyTarget(PlayerCharacter caster, float range)
        {
            if (caster == null || combatManager == null)
            {
                return null;
            }

            var focused = combatManager.FocusedEnemy;
            if (focused != null && focused.IsAlive && IsInRange(caster.transform.position, focused.transform.position, range))
            {
                return focused;
            }

            Enemy nearest = null;
            var nearestDistance = float.MaxValue;
            var enemies = combatManager.LivingEnemies;
            for (var i = 0; i < enemies.Count; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }

                var distance = Vector3.Distance(caster.transform.position, enemy.transform.position);
                if (distance <= range && distance < nearestDistance)
                {
                    nearest = enemy;
                    nearestDistance = distance;
                }
            }

            return nearest;
        }

        private PlayerCharacter FindMostInjuredAlly(PlayerCharacter caster, float range)
        {
            var members = partyManager?.PartyMembers;
            if (members == null || caster == null)
            {
                return null;
            }

            PlayerCharacter target = null;
            var lowestHealthRatio = 1f;
            for (var i = 0; i < members.Count; i++)
            {
                var member = members[i];
                if (member == null || !member.IsAlive || member.CurrentHealth >= member.MaxHealth)
                {
                    continue;
                }

                if (!IsInRange(caster.transform.position, member.transform.position, range))
                {
                    continue;
                }

                var ratio = member.MaxHealth > 0 ? (float)member.CurrentHealth / member.MaxHealth : 1f;
                if (ratio < lowestHealthRatio)
                {
                    target = member;
                    lowestHealthRatio = ratio;
                }
            }

            return target;
        }

        private void CacheReferences()
        {
            if (gameManager == null) gameManager = FindAnyObjectByType<GameManager>();
            if (partyManager == null) partyManager = FindAnyObjectByType<PartyManager>();
            if (combatManager == null) combatManager = FindAnyObjectByType<CombatManager>();
            if (generator == null) generator = FindAnyObjectByType<GeneratorG17>();
            if (notificationUI == null) notificationUI = FindAnyObjectByType<NotificationUI>();
        }

        private static bool IsInRange(Vector3 from, Vector3 to, float range)
        {
            return Vector3.Distance(from, to) <= Mathf.Max(0.1f, range);
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
    }
}
