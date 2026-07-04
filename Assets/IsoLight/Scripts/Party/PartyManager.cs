using System;
using System.Collections.Generic;
using IsoLight.Characters;
using UnityEngine;

namespace IsoLight.Party
{
    public class PartyManager : MonoBehaviour
    {
        [SerializeField] private List<PlayerCharacter> partyMembers = new List<PlayerCharacter>();
        [SerializeField] private int activeCharacterIndex;

        public event Action<PlayerCharacter> ActiveCharacterChanged;

        public IReadOnlyList<PlayerCharacter> PartyMembers => partyMembers;
        public PlayerCharacter ActiveCharacter { get; private set; }

        private void Awake()
        {
            SelectInitialCharacter();
        }

        public void SetPartyMembers(IEnumerable<PlayerCharacter> members)
        {
            partyMembers.Clear();

            foreach (var member in members)
            {
                RegisterPartyMember(member);
            }

            SelectInitialCharacter();
        }

        public void RegisterPartyMember(PlayerCharacter character)
        {
            if (character != null && !partyMembers.Contains(character))
            {
                partyMembers.Add(character);
            }
        }

        public void SelectCharacterByIndex(int index)
        {
            if (index < 0 || index >= partyMembers.Count)
            {
                return;
            }

            activeCharacterIndex = index;
            SelectCharacter(partyMembers[index]);
        }

        public void SelectCharacter(PlayerCharacter character)
        {
            if (character == null || !partyMembers.Contains(character))
            {
                return;
            }

            ActiveCharacter = character;

            for (var i = 0; i < partyMembers.Count; i++)
            {
                partyMembers[i].SetSelected(partyMembers[i] == ActiveCharacter);
                if (partyMembers[i] == ActiveCharacter)
                {
                    activeCharacterIndex = i;
                }
            }

            ActiveCharacterChanged?.Invoke(ActiveCharacter);
        }

        public void MoveActiveCharacterTo(Vector3 destination)
        {
            ActiveCharacter?.MoveTo(destination);
        }

        private void SelectInitialCharacter()
        {
            if (partyMembers.Count == 0)
            {
                ActiveCharacter = null;
                return;
            }

            activeCharacterIndex = Mathf.Clamp(activeCharacterIndex, 0, partyMembers.Count - 1);
            SelectCharacterByIndex(activeCharacterIndex);
        }
    }
}
