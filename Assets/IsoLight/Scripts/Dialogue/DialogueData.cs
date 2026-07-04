using System.Collections.Generic;
using UnityEngine;

namespace IsoLight.Dialogue
{
    [CreateAssetMenu(fileName = "SO_Dialogue", menuName = "IsoLight/Dialogue/Dialogue Data")]
    public class DialogueData : ScriptableObject
    {
        public string Id;
        public string SpeakerName;
        public string StartNodeId = "start";
        public List<DialogueNode> Nodes = new List<DialogueNode>();
    }
}
