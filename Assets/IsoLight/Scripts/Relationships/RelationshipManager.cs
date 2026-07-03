using UnityEngine;

namespace IsoLight.Relationships
{
    public class RelationshipManager : MonoBehaviour
    {
        [SerializeField] private RelationshipState relationshipState = new RelationshipState();

        public RelationshipState RelationshipState => relationshipState;
    }
}
