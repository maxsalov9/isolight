using System;
using System.Collections.Generic;

namespace IsoLight.Dialogue
{
    [Serializable]
    public class DialogueNode
    {
        public string NodeId;
        public string SpeakerId;
        public string SpeakerName;
        public string Text;
        public string CompanionComment;
        public List<DialogueChoice> Choices = new List<DialogueChoice>();
        public List<DialogueEffect> Effects = new List<DialogueEffect>();
    }
}
