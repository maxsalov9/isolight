using IsoLight.Combat;
using IsoLight.Core;
using IsoLight.Power;
using IsoLight.Quests;
using UnityEngine;

namespace IsoLight.UI
{
    public class PlaytestDebugPanelUI : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private QuestManager questManager;
        [SerializeField] private CombatManager combatManager;
        [SerializeField] private PowerManager powerManager;
        [SerializeField] private GeneratorG17 generator;
        [SerializeField] private NotificationUI notificationUI;

        private bool expanded;
        private GUIStyle panelStyle;
        private GUIStyle labelStyle;
        private GUIStyle buttonStyle;

        public void SetReferences(
            GameManager game,
            QuestManager quests,
            CombatManager combat,
            PowerManager power,
            GeneratorG17 targetGenerator,
            NotificationUI notifications)
        {
            gameManager = game;
            questManager = quests;
            combatManager = combat;
            powerManager = power;
            generator = targetGenerator;
            notificationUI = notifications;
        }

        private void OnGUI()
        {
            panelStyle ??= CreatePanelStyle();
            labelStyle ??= CreateLabelStyle();
            buttonStyle ??= CreateButtonStyle();

            var width = expanded ? 260f : 142f;
            var height = expanded ? 374f : 34f;
            var rect = new Rect(Screen.width - width - 20f, 20f, width, height);
            GUI.Box(rect, string.Empty, panelStyle);

            if (GUI.Button(new Rect(rect.x + 8f, rect.y + 6f, width - 16f, 24f), expanded ? "Debug: скрыть" : "Debug tools", buttonStyle))
            {
                expanded = !expanded;
            }

            if (!expanded)
            {
                return;
            }

            GUI.Label(new Rect(rect.x + 10f, rect.y + 38f, width - 20f, 22f), "DEV ONLY / PLAYTEST", labelStyle);
            var y = rect.y + 66f;
            DrawButton(rect.x + 10f, ref y, width - 20f, "Собрать все breaker-модули", CollectBreakerModules);
            DrawButton(rect.x + 10f, ref y, width - 20f, "Осмотреть все системы", MarkAllSystemsInspected);
            DrawButton(rect.x + 10f, ref y, width - 20f, "Починить генератор", DebugRepairGenerator);
            DrawButton(rect.x + 10f, ref y, width - 20f, "Начать бой", DebugStartCombat);
            DrawButton(rect.x + 10f, ref y, width - 20f, "Победить в бою", DebugWinCombat);
            DrawButton(rect.x + 10f, ref y, width - 20f, "Повредить генератор", DebugDamageGenerator);
            DrawButton(rect.x + 10f, ref y, width - 20f, "Открыть распределение", DebugOpenPowerBoard);
            DrawButton(rect.x + 10f, ref y, width - 20f, "Сбросить миссию", ResetMissionState);
        }

        private void DrawButton(float x, ref float y, float width, string text, System.Action action)
        {
            if (GUI.Button(new Rect(x, y, width, 28f), text, buttonStyle))
            {
                CacheReferences();
                action?.Invoke();
            }

            y += 34f;
        }

        private void CollectBreakerModules()
        {
            if (gameManager == null || questManager == null)
            {
                return;
            }

            gameManager.MissionState.BreakerModulesCollected = 2;
            questManager.UpdateObjectiveDescription(MissionFlowController.FindBreakerModulesObjectiveId, "Найти 2 breaker-модуля: 2/2");
            questManager.CompleteObjective(MissionFlowController.FindBreakerModulesObjectiveId);
            questManager.ActivateObjective(MissionFlowController.RepairGeneratorLineObjectiveId);
            notificationUI?.ShowMessage("Debug: breaker-модули собраны.");
        }

        private void MarkAllSystemsInspected()
        {
            if (gameManager == null || questManager == null)
            {
                return;
            }

            var state = gameManager.MissionState;
            state.InspectedWaterFilters = true;
            state.InspectedHydroponicFarm = true;
            state.InspectedDefenseGate = true;
            state.InspectedPublicStage = true;
            state.InspectedWorkshop = true;
            state.InspectedRelayStation = true;
            questManager.UpdateObjectiveDescription(MissionFlowController.InspectKeySystemsObjectiveId, "Осмотреть ключевые системы: 6/6");
            questManager.CompleteObjective(MissionFlowController.InspectKeySystemsObjectiveId);
            questManager.ActivateObjective(MissionFlowController.FindBreakerModulesObjectiveId);
            notificationUI?.ShowMessage("Debug: ключевые системы отмечены как осмотренные.");
        }

        private void DebugRepairGenerator()
        {
            generator?.DebugRepairForPlaytest();
            notificationUI?.ShowMessage("Debug: Generator G-17 починен.");
        }

        private void DebugStartCombat()
        {
            generator?.DebugStartForPlaytest();
            combatManager?.StartCombat();
        }

        private void DebugWinCombat()
        {
            combatManager?.DebugWinCombat();
        }

        private void DebugDamageGenerator()
        {
            generator?.TakeDamage(40);
            notificationUI?.ShowMessage("Debug: Generator G-17 получил урон.");
        }

        private void DebugOpenPowerBoard()
        {
            if (gameManager != null)
            {
                gameManager.MissionState.GeneratorDefended = true;
            }

            questManager?.ActivateObjective(MissionFlowController.AllocatePowerObjectiveId);
            powerManager?.OpenPowerAllocationBoard();
        }

        private void ResetMissionState()
        {
            gameManager?.DebugResetMissionState();

            if (questManager?.ActiveQuest != null)
            {
                var quest = questManager.ActiveQuest;
                questManager.StartQuest(quest);
                questManager.ActivateObjective(MissionFlowController.ReachShelterObjectiveId);
            }

            notificationUI?.ShowMessage("Debug: mission state сброшен. Для полной очистки объектов лучше перезапустить сцену.");
        }

        private void CacheReferences()
        {
            if (gameManager == null) gameManager = FindAnyObjectByType<GameManager>();
            if (questManager == null) questManager = FindAnyObjectByType<QuestManager>();
            if (combatManager == null) combatManager = FindAnyObjectByType<CombatManager>();
            if (powerManager == null) powerManager = FindAnyObjectByType<PowerManager>();
            if (generator == null) generator = FindAnyObjectByType<GeneratorG17>();
            if (notificationUI == null) notificationUI = FindAnyObjectByType<NotificationUI>();
        }

        private static GUIStyle CreatePanelStyle()
        {
            var style = new GUIStyle(GUI.skin.box);
            return style;
        }

        private static GUIStyle CreateLabelStyle()
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            style.normal.textColor = new Color(1f, 0.8f, 0.25f);
            return style;
        }

        private static GUIStyle CreateButtonStyle()
        {
            return new GUIStyle(GUI.skin.button)
            {
                fontSize = 12
            };
        }
    }
}
