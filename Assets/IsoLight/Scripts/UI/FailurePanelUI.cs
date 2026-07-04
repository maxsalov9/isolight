using IsoLight.Combat;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IsoLight.UI
{
    public class FailurePanelUI : MonoBehaviour
    {
        [SerializeField] private CombatManager combatManager;

        private string failureMessage;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle buttonStyle;

        public bool IsVisible => !string.IsNullOrEmpty(failureMessage);

        public void SetCombatManager(CombatManager manager)
        {
            combatManager = manager;
        }

        public void ShowFailure(string message)
        {
            failureMessage = string.IsNullOrWhiteSpace(message)
                ? "Столкновение провалено. Перезапустите сцену и попробуйте снова."
                : message;
        }

        public void Hide()
        {
            failureMessage = null;
        }

        private void OnGUI()
        {
            if (string.IsNullOrEmpty(failureMessage))
            {
                return;
            }

            titleStyle ??= CreateTitleStyle();
            bodyStyle ??= CreateBodyStyle();
            buttonStyle ??= CreateButtonStyle();

            var rect = new Rect(Screen.width * 0.5f - 260f, Screen.height * 0.5f - 120f, 520f, 240f);
            GUI.Box(rect, string.Empty);
            GUI.Label(new Rect(rect.x + 18f, rect.y + 16f, rect.width - 36f, 32f), "Миссия провалена", titleStyle);
            GUI.Label(new Rect(rect.x + 18f, rect.y + 62f, rect.width - 36f, 84f), failureMessage, bodyStyle);
            GUI.Label(new Rect(rect.x + 18f, rect.y + 138f, rect.width - 36f, 42f), "Для быстрого playtest можно перезапустить сцену или заново собрать ее через IsoLight > Build Riverside Prototype Scene.", bodyStyle);

            if (GUI.Button(new Rect(rect.x + 18f, rect.y + 188f, 220f, 34f), "Перезапустить сцену", buttonStyle))
            {
                Hide();
                ReloadCurrentScene();
            }

            if (GUI.Button(new Rect(rect.x + 256f, rect.y + 188f, 120f, 34f), "Скрыть", buttonStyle))
            {
                Hide();
            }
        }

        private static GUIStyle CreateTitleStyle()
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 22,
                fontStyle = FontStyle.Bold
            };
            style.normal.textColor = new Color(1f, 0.35f, 0.25f);
            return style;
        }

        private static GUIStyle CreateBodyStyle()
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 15,
                wordWrap = true
            };
            style.normal.textColor = Color.white;
            return style;
        }

        private static GUIStyle CreateButtonStyle()
        {
            var style = new GUIStyle(GUI.skin.button)
            {
                fontSize = 14
            };
            return style;
        }

        private static void ReloadCurrentScene()
        {
            var activeScene = SceneManager.GetActiveScene();
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(activeScene.path))
            {
                if (Application.isPlaying)
                {
                    UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(
                        activeScene.path,
                        new LoadSceneParameters(LoadSceneMode.Single));
                }
                else
                {
                    UnityEditor.SceneManagement.EditorSceneManager.OpenScene(activeScene.path);
                }

                return;
            }
#endif
            if (activeScene.buildIndex >= 0)
            {
                SceneManager.LoadScene(activeScene.buildIndex);
            }
            else if (!string.IsNullOrEmpty(activeScene.name))
            {
                SceneManager.LoadScene(activeScene.name);
            }
        }
    }
}
