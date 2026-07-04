using System;
using IsoLight.Quests;

namespace IsoLight.Dialogue
{
    [Serializable]
    public class DialogueEffect
    {
        public DialogueEffectType Type;
        public string Key;
        public bool BoolValue = true;
        public QuestData Quest;
    }
}
