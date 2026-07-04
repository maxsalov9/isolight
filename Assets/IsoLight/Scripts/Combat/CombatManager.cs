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
        [SerializeField] private GeneratorStatusUI generatorStatusUI;
        [SerializeField] private FailurePanelUI failurePanelUI;
        [SerializeField] private List<EnemyData> enemyData = new List<EnemyData>();
        [SerializeField] private List<Vector3> spawnPoints = new List<Vector3>();

        private readonly List<Enemy> livingEnemies = new List<Enemy>();
        private Transform enemyParent;

        public bool IsCombatActive { get; private set; }
        public int LivingEnemyCount => livingEnemies.Count;
        public IReadOnlyList<Enemy> LivingEnemies => livingEnemies;
        public Enemy FocusedEnemy { get; private set; }

        public void SetReferences(
            GameManager game,
            PartyManager party,
            QuestManager quest,
            GeneratorG17 targetGenerator,
            NotificationUI notifications,
            CombatStatusUI statusUI,
            GeneratorStatusUI generatorStatus,
            FailurePanelUI failurePanel = null)
        {
            gameManager = game;
            partyManager = party;
            questManager = quest;
            generator = targetGenerator;
            notificationUI = notifications;
            combatStatusUI = statusUI;
            generatorStatusUI = generatorStatus;
            failurePanelUI = failurePanel;
        }

        private void Update()
        {
            if (IsCombatActive && AreAllPartyMembersDowned())
            {
                notificationUI?.ShowMessage("Весь отряд выведен из строя — столкновение провалено.");
                EndCombat(false, "Весь отряд выведен из строя. Перезапустите столкновение или сцену для повторного playtest.");
            }
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
            generatorStatusUI?.SetReferences(this, generator);
            notificationUI?.ShowMessage("Бой начался: защитите Generator G-17.");

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
                notificationUI?.ShowMessage("Цель слишком далеко или атака еще восстанавливается.");
            }

            FocusedEnemy = enemy;
        }

        public void HandleGeneratorDestroyed()
        {
            if (!IsCombatActive)
            {
                return;
            }

            notificationUI?.ShowMessage("Генератор уничтожен. Схватка провалена.");
            EndCombat(false, "Генератор уничтожен. Схватка провалена. Перезапустите столкновение или сцену для повторного playtest.");
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
            enemyObject.transform.localScale = GetEnemyScale(data.Role);

            var renderer = enemyObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = GetEnemyColor(data.Role);
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
            ai.SetReferences(this, partyManager, generator, notificationUI);
            AddEnemyRoleMarker(enemyObject, data.Role);
            enemyObject.AddComponent<EnemyHealthBarUI>();

            return enemy;
        }

        private static Color GetEnemyColor(EnemyRole role)
        {
            return role switch
            {
                EnemyRole.Gunner => new Color(0.33f, 0.22f, 0.17f),
                EnemyRole.Runner => new Color(0.52f, 0.34f, 0.21f),
                EnemyRole.Saboteur => new Color(0.82f, 0.08f, 0.04f),
                _ => new Color(0.42f, 0.24f, 0.18f)
            };
        }

        private static Vector3 GetEnemyScale(EnemyRole role)
        {
            return role switch
            {
                EnemyRole.Gunner => new Vector3(1.18f, 1.08f, 1.18f),
                EnemyRole.Runner => new Vector3(0.86f, 1f, 0.86f),
                EnemyRole.Saboteur => new Vector3(1.12f, 1.15f, 1.12f),
                _ => Vector3.one
            };
        }

        private static void AddEnemyRoleMarker(GameObject enemyObject, EnemyRole role)
        {
            if (enemyObject == null || role != EnemyRole.Saboteur)
            {
                return;
            }

            var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = "Saboteur_GeneratorThreatMarker";
            marker.transform.SetParent(enemyObject.transform, false);
            marker.transform.localPosition = new Vector3(0f, 1.35f, 0f);
            marker.transform.localScale = new Vector3(0.42f, 0.12f, 0.42f);

            var markerRenderer = marker.GetComponent<Renderer>();
            if (markerRenderer != null)
            {
                markerRenderer.material.color = new Color(1f, 0.08f, 0.02f);
            }

            var markerCollider = marker.GetComponent<Collider>();
            if (markerCollider != null)
            {
                Destroy(markerCollider);
            }
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
                notificationUI?.ShowMessage($"Враг уничтожен: {enemy.name}.");
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

        public void DebugWinCombat()
        {
            if (!IsCombatActive)
            {
                StartCombat();
            }

            for (var i = livingEnemies.Count - 1; i >= 0; i--)
            {
                var enemy = livingEnemies[i];
                if (enemy != null)
                {
                    enemy.Died -= HandleEnemyDied;
                    enemy.gameObject.SetActive(false);
                }
            }

            livingEnemies.Clear();
            FocusedEnemy = null;
            EndCombat(true);
            notificationUI?.ShowMessage("Debug: бой завершен победой.");
        }

        private void EndCombat(bool victory, string failureReason = null)
        {
            IsCombatActive = false;

            if (gameManager != null)
            {
                gameManager.CurrentGameMode = GameMode.Exploration;
                gameManager.MissionState.GeneratorDefended = victory;
            }

            if (victory)
            {
                questManager?.CompleteObjective(MissionFlowController.DefendGeneratorObjectiveId);
                questManager?.ActivateObjective(AllocatePowerObjectiveId);
                notificationUI?.ShowMessage("Генератор защищен. Комната переключателей разблокирована.");
            }
            else
            {
                failurePanelUI?.ShowFailure(failureReason);
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

            if (generatorStatusUI == null)
            {
                generatorStatusUI = FindAnyObjectByType<GeneratorStatusUI>();
            }

            if (failurePanelUI == null)
            {
                failurePanelUI = FindAnyObjectByType<FailurePanelUI>();
            }
        }

        private bool AreAllPartyMembersDowned()
        {
            var members = partyManager?.PartyMembers;
            if (members == null || members.Count == 0)
            {
                return false;
            }

            for (var i = 0; i < members.Count; i++)
            {
                if (members[i] != null && members[i].IsAlive)
                {
                    return false;
                }
            }

            return true;
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
