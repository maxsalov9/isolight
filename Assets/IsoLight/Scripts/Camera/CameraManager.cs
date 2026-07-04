using IsoLight.Characters;
using IsoLight.Party;
using UnityEngine;

namespace IsoLight.Camera
{
    public class CameraManager : MonoBehaviour
    {
        [SerializeField] private IsometricCameraController isometricCameraController;
        [SerializeField] private PartyManager partyManager;

        public void SetReferences(PartyManager party, IsometricCameraController cameraController)
        {
            partyManager = party;
            isometricCameraController = cameraController;
            FollowActiveCharacter(partyManager != null ? partyManager.ActiveCharacter : null);
        }

        private void Start()
        {
            if (partyManager == null)
            {
                partyManager = FindAnyObjectByType<PartyManager>();
            }

            if (isometricCameraController == null)
            {
                isometricCameraController = FindAnyObjectByType<IsometricCameraController>();
            }

            if (partyManager != null)
            {
                partyManager.ActiveCharacterChanged += FollowActiveCharacter;
                FollowActiveCharacter(partyManager.ActiveCharacter);
            }
        }

        private void OnDestroy()
        {
            if (partyManager != null)
            {
                partyManager.ActiveCharacterChanged -= FollowActiveCharacter;
            }
        }

        private void FollowActiveCharacter(PlayerCharacter activeCharacter)
        {
            if (isometricCameraController != null && activeCharacter != null)
            {
                isometricCameraController.Target = activeCharacter.transform;
            }
        }
    }
}
