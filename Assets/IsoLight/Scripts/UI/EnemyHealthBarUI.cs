using IsoLight.Enemies;
using UnityEngine;

namespace IsoLight.UI
{
    [RequireComponent(typeof(Enemy))]
    public class EnemyHealthBarUI : MonoBehaviour
    {
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2.35f, 0f);
        [SerializeField] private Vector2 size = new Vector2(76f, 12f);

        private Enemy enemy;
        private UnityEngine.Camera cachedCamera;

        private void Awake()
        {
            enemy = GetComponent<Enemy>();
            cachedCamera = UnityEngine.Camera.main;
        }

        private void OnGUI()
        {
            if (enemy == null || !enemy.IsAlive)
            {
                return;
            }

            if (cachedCamera == null)
            {
                cachedCamera = UnityEngine.Camera.main;
            }

            if (cachedCamera == null)
            {
                return;
            }

            var screenPosition = cachedCamera.WorldToScreenPoint(transform.position + worldOffset);
            if (screenPosition.z <= 0f)
            {
                return;
            }

            var rect = new Rect(screenPosition.x - size.x * 0.5f, Screen.height - screenPosition.y, size.x, size.y);
            GUI.Box(rect, GUIContent.none);

            var previousColor = GUI.color;
            GUI.color = new Color(0.75f, 0.08f, 0.06f);
            GUI.DrawTexture(new Rect(rect.x + 1f, rect.y + 1f, (rect.width - 2f) * Mathf.Clamp01((float)enemy.CurrentHealth / enemy.MaxHealth), rect.height - 2f), Texture2D.whiteTexture);
            GUI.color = previousColor;
        }
    }
}
