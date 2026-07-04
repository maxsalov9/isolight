using System.Collections.Generic;
using UnityEngine;

namespace IsoLight.Quests
{
    [CreateAssetMenu(fileName = "SO_Quest", menuName = "IsoLight/Quests/Quest Data")]
    public class QuestData : ScriptableObject
    {
        public string Id;
        public string Title;
        public string Description;
        public List<ObjectiveData> Objectives = new List<ObjectiveData>();
    }
}
