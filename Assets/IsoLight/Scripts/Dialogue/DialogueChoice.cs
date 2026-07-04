using System;
using System.Collections.Generic;

namespace IsoLight.Dialogue
{
    [Serializable]
    public class DialogueChoice
    {
        public string Text;
        public string NextNodeId;
        public List<DialogueCondition> Conditions = new List<DialogueCondition>();
        public List<DialogueEffect> Effects = new List<DialogueEffect>();
    }
}
