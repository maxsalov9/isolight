using IsoLight.Characters;
using IsoLight.Combat;
using IsoLight.Party;
using IsoLight.Power;
using UnityEngine;
using UnityEngine.AI;

namespace IsoLight.Enemies
{
    [RequireComponent(typeof(Enemy))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyAI : MonoBehaviour
    {
        [SerializeField] private EnemyState currentState = EnemyState.Idle;

        private Enemy enemy;
        private NavMeshAgent navMeshAgent;
        private CombatManager combatManager;
        private PartyManager partyManager;
        private GeneratorG17 generator;
        private float nextAttackTime;

        public EnemyState CurrentState => currentState;

        private void Awake()
        {
            enemy = GetComponent<Enemy>();
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        public void SetReferences(CombatManager combat, PartyManager party, GeneratorG17 targetGenerator)
        {
            combatManager = combat;
            partyManager = party;
            generator = targetGenerator;
        }

        private void Update()
        {
            if (enemy == null || !enemy.IsAlive)
            {
                currentState = EnemyState.Dead;
                return;
            }

            if (combatManager == null || !combatManager.IsCombatActive)
            {
                currentState = EnemyState.Idle;
                return;
            }

            if (enemy.Role == EnemyRole.Saboteur && generator != null && generator.IsAlive)
            {
                TickAgainstTarget(generator, generator.transform, EnemyState.SabotageGenerator);
                return;
            }

            var target = FindNearestLivingPartyMember();
            if (target != null)
            {
                TickAgainstTarget(target, target.transform, EnemyState.AttackPlayer);
                return;
            }

            if (generator != null && generator.IsAlive)
            {
                TickAgainstTarget(generator, generator.transform, EnemyState.AttackGenerator);
            }
        }

        private void TickAgainstTarget(IDamageable damageable, Transform targetTransform, EnemyState attackState)
        {
            var distance = Vector3.Distance(transform.position, targetTransform.position);
            if (distance > enemy.AttackRange)
            {
                currentState = attackState == EnemyState.SabotageGenerator ? EnemyState.SabotageGenerator : EnemyState.MoveToCover;
                MoveTo(targetTransform.position);
                return;
            }

            if (navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.isStopped = true;
            }

            currentState = attackState;
            if (Time.time >= nextAttackTime)
            {
                damageable.TakeDamage(enemy.Damage);
                nextAttackTime = Time.time + enemy.AttackCooldown;
            }
        }

        private PlayerCharacter FindNearestLivingPartyMember()
        {
            if (partyManager == null)
            {
                partyManager = FindAnyObjectByType<PartyManager>();
            }

            PlayerCharacter nearest = null;
            var nearestDistance = float.MaxValue;
            var members = partyManager?.PartyMembers;
            if (members == null)
            {
                return null;
            }

            for (var i = 0; i < members.Count; i++)
            {
                var member = members[i];
                if (member == null || !member.IsAlive)
                {
                    continue;
                }

                var distance = Vector3.Distance(transform.position, member.transform.position);
                if (distance < nearestDistance)
                {
                    nearest = member;
                    nearestDistance = distance;
                }
            }

            return nearest;
        }

        private void MoveTo(Vector3 destination)
        {
            if (navMeshAgent == null || !navMeshAgent.enabled || !navMeshAgent.isOnNavMesh)
            {
                return;
            }

            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(destination);
        }
    }
}
