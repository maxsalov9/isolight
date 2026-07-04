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
    public class AbilityTargetPreview
    {
        public AbilityData Ability;
        public Component Target;
        public string TargetLabel;
        public string UnavailableReason;
        public float CooldownRemaining;
        public bool IsUsable;
        public bool HasTarget => Target != null;
    }

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
            var preview = GetAbilityPreview(slotIndex);
            if (preview.Ability == null)
            {
                notificationUI?.ShowMessage(preview.UnavailableReason);
                return;
            }

            if (!preview.IsUsable)
            {
                notificationUI?.ShowMessage(preview.UnavailableReason);
                return;
            }

            if (!caster.TryBeginAbility(preview.Ability, out var failureReason))
            {
                notificationUI?.ShowMessage(failureReason);
                return;
            }

            ApplyAbility(caster, preview);
        }

        public AbilityTargetPreview GetAbilityPreview(int slotIndex)
        {
            CacheReferences();

            var preview = new AbilityTargetPreview();
            var caster = partyManager != null ? partyManager.ActiveCharacter : null;
            if (caster == null)
            {
                preview.UnavailableReason = "Нет активного персонажа.";
                return preview;
            }

            var abilities = caster.Abilities;
            if (slotIndex < 0 || slotIndex >= abilities.Count || abilities[slotIndex] == null)
            {
                preview.UnavailableReason = "Способность не назначена.";
                return preview;
            }

            return GetAbilityPreview(caster, abilities[slotIndex]);
        }

        public AbilityTargetPreview GetAbilityPreview(PlayerCharacter caster, AbilityData ability)
        {
            var preview = new AbilityTargetPreview
            {
                Ability = ability,
                CooldownRemaining = caster != null ? caster.GetAbilityCooldownRemaining(ability) : 0f
            };

            if (caster == null)
            {
                preview.UnavailableReason = "Нет активного персонажа.";
                return preview;
            }

            if (ability == null)
            {
                preview.UnavailableReason = "Способность не назначена.";
                return preview;
            }

            if (gameManager == null || combatManager == null || !combatManager.IsCombatActive || gameManager.CurrentGameMode != GameMode.Combat)
            {
                preview.UnavailableReason = "Только в бою";
                return preview;
            }

            if (!caster.HasEnoughEnergy(ability))
            {
                preview.UnavailableReason = "Недостаточно энергии";
                ResolveTarget(caster, ability, preview);
                return preview;
            }

            if (preview.CooldownRemaining > 0f)
            {
                preview.UnavailableReason = "Способность перезаряжается";
                ResolveTarget(caster, ability, preview);
                return preview;
            }

            ResolveTarget(caster, ability, preview);
            preview.IsUsable = preview.Target != null;
            return preview;
        }

        private void ResolveTarget(PlayerCharacter caster, AbilityData ability, AbilityTargetPreview preview)
        {
            switch (ability.EffectType)
            {
                case AbilityEffectType.RepairGenerator:
                    ResolveGeneratorTarget(caster, ability, preview);
                    break;
                case AbilityEffectType.HealAlly:
                    ResolveHealTarget(caster, ability, preview);
                    break;
                case AbilityEffectType.StabilizeAlly:
                    ResolveStabilizeTarget(caster, ability, preview);
                    break;
                case AbilityEffectType.DamageEnemy:
                case AbilityEffectType.ShockEnemy:
                    ResolveEnemyTarget(caster, ability, preview);
                    break;
                default:
                    preview.UnavailableReason = "Нет подходящей цели";
                    break;
            }
        }

        private void ResolveGeneratorTarget(PlayerCharacter caster, AbilityData ability, AbilityTargetPreview preview)
        {
            if (generator == null || !generator.IsAlive)
            {
                preview.UnavailableReason = "Нет подходящей цели";
                return;
            }

            if (generator.CurrentHealth >= generator.MaxHealth)
            {
                preview.UnavailableReason = "Генератор не поврежден";
                return;
            }

            if (!IsInRange(caster.transform.position, generator.transform.position, ability.Range))
            {
                preview.UnavailableReason = "Generator G-17 слишком далеко";
                return;
            }

            preview.Target = generator;
            preview.TargetLabel = "Generator G-17";
        }

        private void ResolveEnemyTarget(PlayerCharacter caster, AbilityData ability, AbilityTargetPreview preview)
        {
            var enemy = FindEnemyTarget(caster, ability.Range);
            if (enemy == null)
            {
                preview.UnavailableReason = "Нет подходящей цели";
                return;
            }

            preview.Target = enemy;
            preview.TargetLabel = enemy.name;
        }

        private void ResolveHealTarget(PlayerCharacter caster, AbilityData ability, AbilityTargetPreview preview)
        {
            var ally = FindMostInjuredAlly(caster, ability.Range);
            if (ally == null)
            {
                preview.UnavailableReason = "Нет раненых союзников";
                return;
            }

            preview.Target = ally;
            preview.TargetLabel = ally.DisplayName;
        }

        private void ResolveStabilizeTarget(PlayerCharacter caster, AbilityData ability, AbilityTargetPreview preview)
        {
            var ally = FindMostInjuredAlly(caster, ability.Range);
            if (ally == null && caster != null && caster.IsAlive)
            {
                ally = caster;
            }

            if (ally == null)
            {
                preview.UnavailableReason = "Нет подходящей цели";
                return;
            }

            preview.Target = ally;
            preview.TargetLabel = ally.DisplayName;
        }

        private void ApplyAbility(PlayerCharacter caster, AbilityTargetPreview preview)
        {
            var ability = preview.Ability;
            switch (ability.EffectType)
            {
                case AbilityEffectType.RepairGenerator:
                    generator.RepairHealth(ability.PowerAmount);
                    FlashTarget(preview.Target, GetAbilityColor(ability));
                    notificationUI?.ShowMessage($"{caster.DisplayName}: {ability.DisplayName}. Generator G-17: {generator.CurrentHealth}/{generator.MaxHealth} HP.");
                    break;

                case AbilityEffectType.DamageEnemy:
                    var damageTarget = preview.Target as Enemy;
                    damageTarget?.TakeDamage(ability.PowerAmount);
                    FlashTarget(damageTarget, GetAbilityColor(ability));
                    notificationUI?.ShowMessage($"{caster.DisplayName}: {ability.DisplayName}");
                    break;

                case AbilityEffectType.ShockEnemy:
                    var shockTarget = preview.Target as Enemy;
                    if (shockTarget != null)
                    {
                        shockTarget.TakeDamage(ability.PowerAmount);
                        shockTarget.GetComponent<EnemyAI>()?.StunFor(ability.StunDuration);
                    }

                    FlashTarget(shockTarget, GetAbilityColor(ability));
                    notificationUI?.ShowMessage($"{caster.DisplayName}: {ability.DisplayName}");
                    break;

                case AbilityEffectType.HealAlly:
                    var healTarget = preview.Target as PlayerCharacter;
                    healTarget?.Heal(ability.PowerAmount);
                    FlashTarget(healTarget, GetAbilityColor(ability));
                    notificationUI?.ShowMessage($"{caster.DisplayName}: {ability.DisplayName}");
                    break;

                case AbilityEffectType.StabilizeAlly:
                    var stabilizeTarget = preview.Target as PlayerCharacter;
                    stabilizeTarget?.Heal(ability.PowerAmount);
                    FlashTarget(stabilizeTarget, GetAbilityColor(ability));
                    notificationUI?.ShowMessage($"{caster.DisplayName}: {ability.DisplayName}");
                    break;
            }
        }

        private static void FlashTarget(Component target, Color color)
        {
            if (target == null)
            {
                return;
            }

            var flash = target.GetComponent<AbilityTargetFlash>();
            if (flash == null)
            {
                flash = target.gameObject.AddComponent<AbilityTargetFlash>();
            }

            flash.Play(color, 0.45f);
        }

        public static Color GetAbilityColor(AbilityData ability)
        {
            if (ability == null)
            {
                return Color.white;
            }

            if (ability.Id != null && ability.Id.StartsWith("dax"))
            {
                return new Color(1f, 0.62f, 0.16f);
            }

            if (ability.Id != null && ability.Id.StartsWith("nyra"))
            {
                return new Color(0.2f, 0.72f, 1f);
            }

            if (ability.Id != null && ability.Id.StartsWith("cormac"))
            {
                return new Color(0.34f, 0.9f, 0.42f);
            }

            return Color.white;
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
