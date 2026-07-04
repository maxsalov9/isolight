namespace IsoLight.Relationships
{
    public class RelationshipChange
    {
        public string GroupName;
        public RelationshipLevel PreviousLevel;
        public RelationshipLevel NewLevel;

        public RelationshipChange(string groupName, RelationshipLevel previousLevel, RelationshipLevel newLevel)
        {
            GroupName = groupName;
            PreviousLevel = previousLevel;
            NewLevel = newLevel;
        }
    }
}
