using UnityEngine;

namespace IsoLight.Characters
{
    public class AbilityTargetFlash : MonoBehaviour
    {
        private Renderer[] renderers;
        private Color[][] originalColors;
        private float flashUntilTime;

        private void Awake()
        {
            CacheRenderers();
        }

        private void Update()
        {
            if (flashUntilTime > 0f && Time.time >= flashUntilTime)
            {
                RestoreColors();
                flashUntilTime = 0f;
            }
        }

        public void Play(Color color, float duration)
        {
            CacheRenderers();

            if (renderers == null)
            {
                return;
            }

            for (var i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                var materials = renderer.materials;
                for (var j = 0; j < materials.Length; j++)
                {
                    if (materials[j] != null)
                    {
                        materials[j].color = color;
                    }
                }
            }

            flashUntilTime = Time.time + Mathf.Max(0.05f, duration);
        }

        private void CacheRenderers()
        {
            renderers = GetComponentsInChildren<Renderer>();
            originalColors = new Color[renderers.Length][];

            for (var i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (renderer == null)
                {
                    originalColors[i] = new Color[0];
                    continue;
                }

                var materials = renderer.materials;
                originalColors[i] = new Color[materials.Length];
                for (var j = 0; j < materials.Length; j++)
                {
                    originalColors[i][j] = materials[j] != null ? materials[j].color : Color.white;
                }
            }
        }

        private void RestoreColors()
        {
            if (renderers == null || originalColors == null)
            {
                return;
            }

            for (var i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                if (renderer == null || i >= originalColors.Length)
                {
                    continue;
                }

                var materials = renderer.materials;
                for (var j = 0; j < materials.Length && j < originalColors[i].Length; j++)
                {
                    if (materials[j] != null)
                    {
                        materials[j].color = originalColors[i][j];
                    }
                }
            }
        }
    }
}
