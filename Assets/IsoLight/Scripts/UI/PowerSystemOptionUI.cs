using IsoLight.Power;
using UnityEngine;

namespace IsoLight.UI
{
    public static class PowerSystemOptionUI
    {
        public static bool DrawOption(Rect rect, PowerSystemData system, bool selected, GUIStyle style)
        {
            var selectedPrefix = selected ? "> " : string.Empty;
            var label = $"{selectedPrefix}{system.DisplayName}  [{system.RequiredPower}%]";
            return GUI.Button(rect, label, style);
        }
    }
}
