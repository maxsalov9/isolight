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
            states.Clear();
            if (visualStates != null)
            {
                states.AddRange(visualStates);
            }

            SetAllInactive();
        }

        public void Apply(PowerChoice choice)
        {
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
            if (activeChoice == PowerChoice.None)
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
            for (var i = 0; i < states.Count; i++)
            {
                states[i]?.SetActive(false, 1f);
            }
        }
    }

    [Serializable]
    public class PowerResultVisualState
    {
        public PowerChoice Choice;
        public GameObject Root;
        public Light[] Lights;
        public bool Blink;
        public bool EnableForSplitLoad = true;

        private readonly List<float> baseIntensities = new List<float>();

        public bool IsActive => Root != null && Root.activeSelf;

        public PowerResultVisualState(PowerChoice choice, GameObject root, Light[] lights, bool blink, bool enableForSplitLoad)
        {
            Choice = choice;
            Root = root;
            Lights = lights;
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
            for (var i = 0; i < Lights.Length; i++)
            {
                if (Lights[i] != null)
                {
                    Lights[i].enabled = active;
                    Lights[i].intensity = baseIntensities[i] * intensityMultiplier;
                }
            }
        }

        public void SetLightMultiplier(float multiplier)
        {
            CaptureBaseIntensities();
            for (var i = 0; i < Lights.Length; i++)
            {
                if (Lights[i] != null && Lights[i].enabled)
                {
                    Lights[i].intensity = baseIntensities[i] * multiplier;
                }
            }
        }

        private void CaptureBaseIntensities()
        {
            if (Lights == null)
            {
                Lights = Array.Empty<Light>();
            }

            if (baseIntensities.Count == Lights.Length)
            {
                return;
            }

            baseIntensities.Clear();
            for (var i = 0; i < Lights.Length; i++)
            {
                baseIntensities.Add(Lights[i] != null ? Mathf.Max(0.01f, Lights[i].intensity) : 0.01f);
            }
        }
    }
}
