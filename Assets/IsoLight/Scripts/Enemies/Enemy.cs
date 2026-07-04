using System;
using IsoLight.Combat;
using UnityEngine;
using UnityEngine.AI;

namespace IsoLight.Enemies
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Enemy : MonoBehaviour, IDamageable
    {
        [SerializeField] private EnemyData enemyData;
        [SerializeField] private int currentHealth;

        private NavMeshAgent navMeshAgent;
        private Collider cachedCollider;
        private CombatManager combatManager;

        public event Action<Enemy> Died;
        public event Action<Enemy> HealthChanged;

        public EnemyData EnemyData => enemyData;
        public EnemyRole Role => enemyData != null ? enemyData.Role : EnemyRole.Scavenger;
        public int MaxHealth => enemyData != null ? enemyData.MaxHealth : 1;
        public int CurrentHealth => currentHealth;
        public bool IsAlive => currentHealth > 0;
        public int Damage => enemyData != null ? enemyData.Damage : 1;
        public float AttackRange => enemyData != null ? enemyData.AttackRange : 3f;
        public float AttackCooldown => enemyData != null ? enemyData.AttackCooldown : 1.5f;

        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            cachedCollider = GetComponent<Collider>();
            combatManager = FindAnyObjectByType<CombatManager>();
            InitializeFromData();
        }

        private void OnMouseDown()
        {
            combatManager ??= FindAnyObjectByType<CombatManager>();
            combatManager?.HandleEnemyClicked(this);
        }

        public void Initialize(EnemyData data, CombatManager manager)
        {
            enemyData = data;
            combatManager = manager;
            InitializeFromData();
        }

        public void TakeDamage(int amount)
        {
            if (amount <= 0 || !IsAlive)
            {
                return;
            }

            currentHealth = Mathf.Max(0, currentHealth - amount);
            HealthChanged?.Invoke(this);

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void InitializeFromData()
        {
            currentHealth = MaxHealth;

            if (navMeshAgent != null && enemyData != null)
            {
                navMeshAgent.speed = enemyData.MoveSpeed;
                navMeshAgent.angularSpeed = 540f;
                navMeshAgent.acceleration = 10f;
                navMeshAgent.stoppingDistance = Mathf.Max(0.8f, enemyData.AttackRange * 0.75f);
            }

            HealthChanged?.Invoke(this);
        }

        private void Die()
        {
            if (navMeshAgent != null)
            {
                navMeshAgent.isStopped = true;
                navMeshAgent.enabled = false;
            }

            if (cachedCollider != null)
            {
                cachedCollider.enabled = false;
            }

            gameObject.SetActive(false);
            Died?.Invoke(this);
        }
    }
}
