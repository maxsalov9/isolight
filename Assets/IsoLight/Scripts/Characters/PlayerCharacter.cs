using System;
using System.Collections.Generic;
using IsoLight.Combat;
using UnityEngine;
using UnityEngine.AI;

namespace IsoLight.Characters
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class PlayerCharacter : MonoBehaviour, IDamageable
    {
        [SerializeField] private CharacterData characterData;
        [SerializeField] private GameObject selectionIndicator;
        [SerializeField] private int currentHealth;
        [SerializeField] private int currentEnergy;
        [SerializeField] private bool isSelected;
        [SerializeField] private int attackDamage = 16;
        [SerializeField] private float attackRange = 6f;
        [SerializeField] private float attackCooldown = 0.75f;

        private static readonly IReadOnlyList<AbilityData> NoAbilities = Array.Empty<AbilityData>();

        private NavMeshAgent navMeshAgent;
        private float nextAttackTime;
        private readonly Dictionary<string, float> abilityReadyTimes = new Dictionary<string, float>();

        public event Action<PlayerCharacter> StatsChanged;
        public event Action<PlayerCharacter, bool> SelectionChanged;

        public CharacterData CharacterData => characterData;
        public string CharacterId => characterData != null ? characterData.Id : name;
        public string DisplayName => characterData != null ? characterData.DisplayName : name;
        public int MaxHealth => characterData != null ? characterData.MaxHealth : 1;
        public int MaxEnergy => characterData != null ? characterData.MaxEnergy : 1;
        public int CurrentHealth => currentHealth;
        public int CurrentEnergy => currentEnergy;
        public bool IsAlive => currentHealth > 0;
        public bool IsSelected => isSelected;
        public int AttackDamage => characterData != null ? characterData.AttackDamage : attackDamage;
        public float AttackRange => characterData != null ? characterData.AttackRange : attackRange;
        public float AttackCooldown => characterData != null ? characterData.AttackCooldown : attackCooldown;
        public IReadOnlyList<AbilityData> Abilities => characterData != null ? characterData.Abilities : NoAbilities;

        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            InitializeFromData();
            UpdateSelectionVisual();
        }

        private void OnValidate()
        {
            if (navMeshAgent == null)
            {
                navMeshAgent = GetComponent<NavMeshAgent>();
            }
        }

        public void SetCharacterData(CharacterData data)
        {
            characterData = data;
            InitializeFromData();
        }

        public void SetSelectionIndicator(GameObject indicator)
        {
            selectionIndicator = indicator;
            UpdateSelectionVisual();
        }

        public void InitializeFromData()
        {
            if (characterData == null)
            {
                return;
            }

            currentHealth = characterData.MaxHealth;
            currentEnergy = characterData.MaxEnergy;
            StatsChanged?.Invoke(this);
        }

        public void SetSelected(bool selected)
        {
            if (isSelected == selected)
            {
                return;
            }

            isSelected = selected;
            UpdateSelectionVisual();
            SelectionChanged?.Invoke(this, isSelected);
        }

        public void MoveTo(Vector3 destination)
        {
            if (!IsAlive || navMeshAgent == null || !navMeshAgent.enabled || !navMeshAgent.isOnNavMesh)
            {
                return;
            }

            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(destination);
        }

        public void SetMovementPaused(bool paused)
        {
            if (navMeshAgent == null || !navMeshAgent.enabled || !navMeshAgent.isOnNavMesh)
            {
                return;
            }

            navMeshAgent.isStopped = paused;
        }

        public bool TryAttack(IDamageable target)
        {
            if (!IsAlive || target == null || !target.IsAlive || Time.time < nextAttackTime)
            {
                return false;
            }

            if (target is not Component targetComponent)
            {
                return false;
            }

            var distance = Vector3.Distance(transform.position, targetComponent.transform.position);
            if (distance > AttackRange)
            {
                MoveTo(targetComponent.transform.position);
                return false;
            }

            if (navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.ResetPath();
            }

            target.TakeDamage(AttackDamage);
            nextAttackTime = Time.time + AttackCooldown;
            return true;
        }

        public bool TryBeginAbility(AbilityData ability, out string failureReason)
        {
            failureReason = null;

            if (!IsAlive)
            {
                failureReason = $"{DisplayName} не может действовать.";
                return false;
            }

            if (ability == null)
            {
                failureReason = "Способность не назначена.";
                return false;
            }

            if (currentEnergy < ability.EnergyCost)
            {
                failureReason = "Недостаточно энергии.";
                return false;
            }

            var cooldownRemaining = GetAbilityCooldownRemaining(ability);
            if (cooldownRemaining > 0f)
            {
                failureReason = $"Способность перезаряжается: {cooldownRemaining:0.0} сек.";
                return false;
            }

            currentEnergy -= ability.EnergyCost;
            abilityReadyTimes[ability.Id] = Time.time + Mathf.Max(0f, ability.Cooldown);
            StatsChanged?.Invoke(this);
            return true;
        }

        public float GetAbilityCooldownRemaining(AbilityData ability)
        {
            if (ability == null || string.IsNullOrEmpty(ability.Id))
            {
                return 0f;
            }

            return abilityReadyTimes.TryGetValue(ability.Id, out var readyTime)
                ? Mathf.Max(0f, readyTime - Time.time)
                : 0f;
        }

        public bool HasEnoughEnergy(AbilityData ability)
        {
            return ability != null && currentEnergy >= ability.EnergyCost;
        }

        public void TakeDamage(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            currentHealth = Mathf.Max(0, currentHealth - amount);
            if (currentHealth <= 0 && navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.ResetPath();
            }

            StatsChanged?.Invoke(this);
        }

        public void Heal(int amount)
        {
            if (amount <= 0 || characterData == null)
            {
                return;
            }

            currentHealth = Mathf.Min(characterData.MaxHealth, currentHealth + amount);
            StatsChanged?.Invoke(this);
        }

        public void RestoreEnergy(int amount)
        {
            if (amount <= 0 || characterData == null)
            {
                return;
            }

            currentEnergy = Mathf.Min(characterData.MaxEnergy, currentEnergy + amount);
            StatsChanged?.Invoke(this);
        }

        private void UpdateSelectionVisual()
        {
            if (selectionIndicator != null)
            {
                selectionIndicator.SetActive(isSelected);
            }
        }
    }
}
