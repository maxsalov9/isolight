using System;
using System.Collections.Generic;
using UnityEngine;

namespace IsoLight.Power
{
    public class PowerResultVisualController : MonoBehaviour
    {
        [SerializeField] private List<PowerResultVisualState> states = new List<PowerResultVisualState>();
        [SerializeField] private float blinkSpeed = 2.4f;
        [SerializeField] private float blinkAmount = 0.35f;

        private PowerChoice activeChoice = PowerChoice.None;

        public void SetStates(IEnumerable<PowerResultVisualState> visualStates)
        {
            EnsureStatesList();
            states.Clear();
            if (visualStates != null)
            {
                states.AddRange(visualStates);
            }

            SetAllInactive();
        }

        public void Apply(PowerChoice choice)
        {
            EnsureStatesList();
            activeChoice = choice;
            SetAllInactive();

            for (var i = 0; i < states.Count; i++)
            {
                var state = states[i];
                if (state == null)
                {
                    continue;
                }

                var shouldEnable = choice == PowerChoice.SplitLoad
                    ? state.EnableForSplitLoad
                    : state.Choice == choice;
                state.SetActive(shouldEnable, choice == PowerChoice.SplitLoad ? 0.55f : 1f);
            }
        }

        private void Update()
        {
            if (activeChoice == PowerChoice.None || states == null)
            {
                return;
            }

            var pulse = 1f + Mathf.Sin(Time.time * blinkSpeed) * blinkAmount;
            for (var i = 0; i < states.Count; i++)
            {
                var state = states[i];
                if (state != null && state.IsActive && state.Blink)
                {
                    state.SetLightMultiplier(pulse);
                }
            }
        }

        private void SetAllInactive()
        {
            if (states == null)
            {
                return;
            }

            for (var i = 0; i < states.Count; i++)
            {
                var state = states[i];
                if (state == null)
                {
                    continue;
                }

                state.SetActive(false, 1f);
            }
        }

        private void EnsureStatesList()
        {
            if (states == null)
            {
                states = new List<PowerResultVisualState>();
            }
        }
    }

    [Serializable]
    public class PowerResultVisualState
    {
        public PowerChoice Choice;
        public GameObject Root;
        public Light[] Lights;
        public Renderer[] Renderers;
        public Material[] Materials;
        public bool Blink;
        public bool EnableForSplitLoad = true;

        private List<float> baseIntensities = new List<float>();
        private bool missingReferencesWarningLogged;

        public bool IsActive => Root != null && Root.activeSelf;

        public PowerResultVisualState(PowerChoice choice, GameObject root, Light[] lights, bool blink, bool enableForSplitLoad)
            : this(choice, root, lights, null, null, blink, enableForSplitLoad)
        {
        }

        public PowerResultVisualState(
            PowerChoice choice,
            GameObject root,
            Light[] lights,
            Renderer[] renderers,
            Material[] materials,
            bool blink,
            bool enableForSplitLoad)
        {
            Choice = choice;
            Root = root;
            Lights = lights;
            Renderers = renderers;
            Materials = materials;
            Blink = blink;
            EnableForSplitLoad = enableForSplitLoad;
            CaptureBaseIntensities();
        }

        public void SetActive(bool active, float intensityMultiplier)
        {
            if (Root != null)
            {
                Root.SetActive(active);
            }

            CaptureBaseIntensities();
            if (Lights != null)
            {
                for (var i = 0; i < Lights.Length; i++)
                {
                    var light = Lights[i];
                    if (light == null)
                    {
                        continue;
                    }

                    light.enabled = active;
                    light.intensity = GetBaseIntensity(i) * intensityMultiplier;
                }
            }

            SetRenderersActive(active);
            LogMissingReferencesOnceIfNeeded();
        }

        public void SetLightMultiplier(float multiplier)
        {
            CaptureBaseIntensities();
            if (Lights == null)
            {
                return;
            }

            for (var i = 0; i < Lights.Length; i++)
            {
                var light = Lights[i];
                if (light != null && light.enabled)
                {
                    light.intensity = GetBaseIntensity(i) * multiplier;
                }
            }
        }

        private void CaptureBaseIntensities()
        {
            EnsureBaseIntensityCache();

            if (Lights == null || Lights.Length == 0)
            {
                baseIntensities.Clear();
                return;
            }

            if (baseIntensities.Count == Lights.Length)
            {
                return;
            }

            baseIntensities.Clear();
            for (var i = 0; i < Lights.Length; i++)
            {
                var light = Lights[i];
                baseIntensities.Add(light != null ? Mathf.Max(0.01f, light.intensity) : 0.01f);
            }
        }

        private void SetRenderersActive(bool active)
        {
            if (Renderers != null)
            {
                for (var i = 0; i < Renderers.Length; i++)
                {
                    var renderer = Renderers[i];
                    if (renderer == null)
                    {
                        continue;
                    }

                    renderer.enabled = active;
                    _ = renderer.sharedMaterial;
                }
            }

            if (Materials == null)
            {
                return;
            }

            for (var i = 0; i < Materials.Length; i++)
            {
                var material = Materials[i];
                if (material == null)
                {
                    continue;
                }
            }
        }

        private float GetBaseIntensity(int index)
        {
            EnsureBaseIntensityCache();
            if (index < 0 || index >= baseIntensities.Count)
            {
                CaptureBaseIntensities();
            }

            return index >= 0 && index < baseIntensities.Count
                ? baseIntensities[index]
                : 0.01f;
        }

        private void EnsureBaseIntensityCache()
        {
            if (baseIntensities == null)
            {
                baseIntensities = new List<float>();
            }
        }

        private void LogMissingReferencesOnceIfNeeded()
        {
            if (missingReferencesWarningLogged)
            {
                return;
            }

            var hasRoot = Root != null;
            var hasUsableLight = HasUsableLight();
            var hasUsableRenderer = HasUsableRenderer();
            var hasUsableMaterial = HasUsableMaterial();
            if (hasRoot && (hasUsableLight || hasUsableRenderer || hasUsableMaterial))
            {
                return;
            }

            missingReferencesWarningLogged = true;
            Debug.LogWarning($"Power result visual state '{Choice}' has incomplete references and will skip missing entries.");
        }

        private bool HasUsableLight()
        {
            if (Lights == null)
            {
                return false;
            }

            for (var i = 0; i < Lights.Length; i++)
            {
                if (Lights[i] != null)
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasUsableRenderer()
        {
            if (Renderers == null)
            {
                return false;
            }

            for (var i = 0; i < Renderers.Length; i++)
            {
                if (Renderers[i] != null)
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasUsableMaterial()
        {
            if (Materials == null)
            {
                return false;
            }

            for (var i = 0; i < Materials.Length; i++)
            {
                if (Materials[i] != null)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
