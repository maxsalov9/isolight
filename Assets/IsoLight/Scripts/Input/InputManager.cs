using IsoLight.Core;
using IsoLight.Party;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace IsoLight.Input
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private PartyManager partyManager;

        public void SetReferences(GameManager game, PartyManager party)
        {
            gameManager = game;
            partyManager = party;
        }

        private void Awake()
        {
            if (gameManager == null)
            {
                gameManager = FindAnyObjectByType<GameManager>();
            }

            if (partyManager == null)
            {
                partyManager = FindAnyObjectByType<PartyManager>();
            }
        }

        private void Update()
        {
            if (gameManager == null || !CanSelectPartyMember() || partyManager == null)
            {
                return;
            }

            if (WasPressed(0))
            {
                partyManager.SelectCharacterByIndex(0);
            }
            else if (WasPressed(1))
            {
                partyManager.SelectCharacterByIndex(1);
            }
            else if (WasPressed(2))
            {
                partyManager.SelectCharacterByIndex(2);
            }
        }

        private static bool WasPressed(int index)
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return false;
            }

            return index switch
            {
                0 => keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame,
                1 => keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame,
                2 => keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame,
                _ => false
            };
#else
            return UnityEngine.Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + index))
                || UnityEngine.Input.GetKeyDown((KeyCode)((int)KeyCode.Keypad1 + index));
#endif
        }

        private bool CanSelectPartyMember()
        {
            return gameManager.CurrentGameMode == GameMode.Exploration
                || gameManager.CurrentGameMode == GameMode.Combat;
        }
    }
}
