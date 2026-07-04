using System;
using UnityEngine;
using UnityEngine.AI;

namespace IsoLight.Characters
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class PlayerCharacter : MonoBehaviour
    {
        [SerializeField] private CharacterData characterData;
        [SerializeField] private GameObject selectionIndicator;
        [SerializeField] private int currentHealth;
        [SerializeField] private int currentEnergy;
        [SerializeField] private bool isSelected;

        private NavMeshAgent navMeshAgent;

        public event Action<PlayerCharacter> StatsChanged;
        public event Action<PlayerCharacter, bool> SelectionChanged;

        public CharacterData CharacterData => characterData;
        public string CharacterId => characterData != null ? characterData.Id : name;
        public string DisplayName => characterData != null ? characterData.DisplayName : name;
        public int CurrentHealth => currentHealth;
        public int CurrentEnergy => currentEnergy;
        public bool IsSelected => isSelected;

        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
            InitializeFromData();
            UpdateSelectionVisual();
        }

        private void OnValidate()
        {
            if (navMeshAgent == null)
            {
                navMeshAgent = GetComponent<NavMeshAgent>();
            }
        }

        public void SetCharacterData(CharacterData data)
        {
            characterData = data;
            InitializeFromData();
        }

        public void SetSelectionIndicator(GameObject indicator)
        {
            selectionIndicator = indicator;
            UpdateSelectionVisual();
        }

        public void InitializeFromData()
        {
            if (characterData == null)
            {
                return;
            }

            currentHealth = characterData.MaxHealth;
            currentEnergy = characterData.MaxEnergy;
            StatsChanged?.Invoke(this);
        }

        public void SetSelected(bool selected)
        {
            if (isSelected == selected)
            {
                return;
            }

            isSelected = selected;
            UpdateSelectionVisual();
            SelectionChanged?.Invoke(this, isSelected);
        }

        public void MoveTo(Vector3 destination)
        {
            if (navMeshAgent == null || !navMeshAgent.enabled || !navMeshAgent.isOnNavMesh)
            {
                return;
            }

            navMeshAgent.SetDestination(destination);
        }

        public void TakeDamage(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            currentHealth = Mathf.Max(0, currentHealth - amount);
            StatsChanged?.Invoke(this);
        }

        public void Heal(int amount)
        {
            if (amount <= 0 || characterData == null)
            {
                return;
            }

            currentHealth = Mathf.Min(characterData.MaxHealth, currentHealth + amount);
            StatsChanged?.Invoke(this);
        }

        private void UpdateSelectionVisual()
        {
            if (selectionIndicator != null)
            {
                selectionIndicator.SetActive(isSelected);
            }
        }
    }
}
