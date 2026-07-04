using System.Collections.Generic;
using IsoLight.Core;
using IsoLight.Enemies;
using IsoLight.Party;
using IsoLight.Power;
using IsoLight.Quests;
using IsoLight.UI;
using UnityEngine;
using UnityEngine.AI;

namespace IsoLight.Combat
{
    public class CombatManager : MonoBehaviour
    {
        private const string AllocatePowerObjectiveId = "allocate_power";

        [SerializeField] private GameManager gameManager;
        [SerializeField] private PartyManager partyManager;
        [SerializeField] private QuestManager questManager;
        [SerializeField] private GeneratorG17 generator;
        [SerializeField] private NotificationUI notificationUI;
        [SerializeField] private CombatStatusUI combatStatusUI;
        [SerializeField] private List<EnemyData> enemyData = new List<EnemyData>();
        [SerializeField] private List<Vector3> spawnPoints = new List<Vector3>();

        private readonly List<Enemy> livingEnemies = new List<Enemy>();
        private Transform enemyParent;

        public bool IsCombatActive { get; private set; }
        public int LivingEnemyCount => livingEnemies.Count;
        public Enemy FocusedEnemy { get; private set; }

        public void SetReferences(
            GameManager game,
            PartyManager party,
            QuestManager quest,
            GeneratorG17 targetGenerator,
            NotificationUI notifications,
            CombatStatusUI statusUI)
        {
            gameManager = game;
            partyManager = party;
            questManager = quest;
            generator = targetGenerator;
            notificationUI = notifications;
            combatStatusUI = statusUI;
        }

        public void SetEnemyData(IEnumerable<EnemyData> enemies)
        {
            enemyData.Clear();
            if (enemies != null)
            {
                enemyData.AddRange(enemies);
            }
        }

        public void SetSpawnPoints(IEnumerable<Vector3> points)
        {
            spawnPoints.Clear();
            if (points != null)
            {
                spawnPoints.AddRange(points);
            }
        }

        public void SetEnemyParent(Transform parent)
        {
            enemyParent = parent;
        }

        public void StartCombat()
        {
            CacheReferences();

            if (IsCombatActive || gameManager == null)
            {
                return;
            }

            IsCombatActive = true;
            gameManager.CurrentGameMode = GameMode.Combat;
            livingEnemies.Clear();
            FocusedEnemy = null;
            SpawnEnemies();
            combatStatusUI?.SetReferences(this, generator);
            notificationUI?.ShowMessage("Combat started: defend Generator G-17.");

            if (livingEnemies.Count == 0)
            {
                EndCombat(true);
            }
        }

        public void HandleEnemyClicked(Enemy enemy)
        {
            if (!IsCombatActive || enemy == null || !enemy.IsAlive || partyManager == null)
            {
                return;
            }

            var attacker = partyManager.ActiveCharacter;
            if (attacker == null || !attacker.IsAlive)
            {
                return;
            }

            if (!attacker.TryAttack(enemy))
            {
                notificationUI?.ShowMessage("Target out of range or attack cooling down.");
            }

            FocusedEnemy = enemy;
        }

        public void HandleGeneratorDestroyed()
        {
            if (!IsCombatActive)
            {
                return;
            }

            notificationUI?.ShowMessage("Generator destroyed — restart encounter.");
            EndCombat(false);
        }

        private void SpawnEnemies()
        {
            EnsureDefaultSpawnPoints();
            for (var i = 0; i < enemyData.Count; i++)
            {
                var data = enemyData[i];
                if (data == null)
                {
                    continue;
                }

                var spawnPoint = spawnPoints[i % spawnPoints.Count];
                var enemy = CreateEnemyInstance(data, spawnPoint);
                RegisterEnemy(enemy);
            }
        }

        private Enemy CreateEnemyInstance(EnemyData data, Vector3 position)
        {
            var enemyObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemyObject.name = data.DisplayName;
            enemyObject.transform.SetParent(enemyParent);
            enemyObject.transform.position = position;

            var renderer = enemyObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = data.Role == EnemyRole.Saboteur
                    ? new Color(0.72f, 0.12f, 0.08f)
                    : new Color(0.42f, 0.24f, 0.18f);
            }

            var agent = enemyObject.AddComponent<NavMeshAgent>();
            agent.speed = data.MoveSpeed;
            agent.angularSpeed = 540f;
            agent.acceleration = 10f;
            agent.radius = 0.35f;
            agent.height = 2f;

            var enemy = enemyObject.AddComponent<Enemy>();
            enemy.Initialize(data, this);

            var ai = enemyObject.AddComponent<EnemyAI>();
            ai.SetReferences(this, partyManager, generator);

            return enemy;
        }

        private void RegisterEnemy(Enemy enemy)
        {
            if (enemy == null)
            {
                return;
            }

            livingEnemies.Add(enemy);
            enemy.Died += HandleEnemyDied;
        }

        private void HandleEnemyDied(Enemy enemy)
        {
            if (enemy != null)
            {
                enemy.Died -= HandleEnemyDied;
                livingEnemies.Remove(enemy);
            }

            if (FocusedEnemy == enemy)
            {
                FocusedEnemy = null;
            }

            if (IsCombatActive && livingEnemies.Count == 0)
            {
                EndCombat(true);
            }
        }

        private void EndCombat(bool victory)
        {
            IsCombatActive = false;

            if (gameManager != null)
            {
                gameManager.CurrentGameMode = GameMode.Exploration;
                gameManager.MissionState.GeneratorDefended = victory;
            }

            if (victory)
            {
                questManager?.ActivateObjective(AllocatePowerObjectiveId);
                notificationUI?.ShowMessage("Generator defended. Switch Room is unlocked.");
            }
        }

        private void CacheReferences()
        {
            if (gameManager == null)
            {
                gameManager = FindAnyObjectByType<GameManager>();
            }

            if (partyManager == null)
            {
                partyManager = FindAnyObjectByType<PartyManager>();
            }

            if (questManager == null)
            {
                questManager = FindAnyObjectByType<QuestManager>();
            }

            if (generator == null)
            {
                generator = FindAnyObjectByType<GeneratorG17>();
            }

            if (notificationUI == null)
            {
                notificationUI = FindAnyObjectByType<NotificationUI>();
            }

            if (combatStatusUI == null)
            {
                combatStatusUI = FindAnyObjectByType<CombatStatusUI>();
            }
        }

        private void EnsureDefaultSpawnPoints()
        {
            if (spawnPoints.Count > 0)
            {
                return;
            }

            spawnPoints.Add(new Vector3(-4.5f, 0f, 7.2f));
            spawnPoints.Add(new Vector3(4.5f, 0f, 7.2f));
            spawnPoints.Add(new Vector3(-5.2f, 0f, 1.2f));
            spawnPoints.Add(new Vector3(5.2f, 0f, 1.2f));
            spawnPoints.Add(new Vector3(0f, 0f, 8.6f));
        }
    }
}
