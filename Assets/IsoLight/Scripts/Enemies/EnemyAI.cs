using IsoLight.Characters;
using IsoLight.Combat;
using IsoLight.Party;
using IsoLight.Power;
using IsoLight.UI;
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
        private NotificationUI notificationUI;
        private float nextAttackTime;
        private float stunnedUntilTime;
        private float nextSaboteurAtGeneratorNoticeTime;
        private bool hasWarnedSaboteurApproach;

        public EnemyState CurrentState => currentState;

        private void Awake()
        {
            enemy = GetComponent<Enemy>();
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        public void SetReferences(CombatManager combat, PartyManager party, GeneratorG17 targetGenerator, NotificationUI notifications = null)
        {
            combatManager = combat;
            partyManager = party;
            generator = targetGenerator;
            notificationUI = notifications;
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

            if (combatManager.IsTacticalPaused)
            {
                currentState = EnemyState.Idle;
                if (navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
                {
                    navMeshAgent.isStopped = true;
                }

                return;
            }

            if (Time.time < stunnedUntilTime)
            {
                currentState = EnemyState.Idle;
                if (navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
                {
                    navMeshAgent.isStopped = true;
                }

                return;
            }

            if (enemy.Role == EnemyRole.Saboteur && generator != null && generator.IsAlive)
            {
                if (!hasWarnedSaboteurApproach)
                {
                    notificationUI?.ShowMessage("Рейдер пытается повредить Generator G-17!");
                    hasWarnedSaboteurApproach = true;
                }

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
                if (attackState == EnemyState.SabotageGenerator && Time.time >= nextSaboteurAtGeneratorNoticeTime)
                {
                    notificationUI?.ShowMessage("Saboteur у генератора!");
                    nextSaboteurAtGeneratorNoticeTime = Time.time + 5f;
                }

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

        public void StunFor(float duration)
        {
            if (duration <= 0f)
            {
                return;
            }

            stunnedUntilTime = Mathf.Max(stunnedUntilTime, Time.time + duration);
            nextAttackTime = Mathf.Max(nextAttackTime, stunnedUntilTime);
            if (navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.isStopped = true;
            }
        }
    }
}
