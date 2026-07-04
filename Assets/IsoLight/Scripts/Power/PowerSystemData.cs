using System.Collections.Generic;
using UnityEngine;

namespace IsoLight.Power
{
    [CreateAssetMenu(fileName = "SO_PowerSystem", menuName = "IsoLight/Power/Power System Data")]
    public class PowerSystemData : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        [TextArea(2, 4)] public string Description;
        public int RequiredPower;
        [TextArea(2, 4)] public string BenefitDescription;
        [TextArea(2, 4)] public string DownsideDescription;
        public string SupportingNPCId;
        public List<string> OpposingNPCIds = new List<string>();
        public PowerChoice PowerChoice;
    }
}
