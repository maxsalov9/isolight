using System;

namespace IsoLight.Quests
{
    [Serializable]
    public class ObjectiveData
    {
        public string Id;
        public string Description;
        public ObjectiveStatus Status = ObjectiveStatus.Hidden;

        public ObjectiveData()
        {
        }

        public ObjectiveData(ObjectiveData source)
        {
            Id = source.Id;
            Description = source.Description;
            Status = source.Status;
        }
    }
}
