using IsoLight.Characters;
using IsoLight.Core;
using IsoLight.Party;
using IsoLight.UI;
using UnityEngine;

namespace IsoLight.Interaction
{
    public class InteractableObject : MonoBehaviour, IInteractable
    {
        [SerializeField] private string interactionName = "Осмотреть объект";
        [SerializeField] private string promptPrefix = "[Клик]";
        [SerializeField] private float interactionDistance = 3f;
        [SerializeField] private Color highlightColor = new Color(1f, 0.86f, 0.25f);

        private GameManager gameManager;
        private PartyManager partyManager;
        private InteractionPromptUI promptUI;
        private Renderer[] renderers;
        private Color[] originalColors;
        private bool isHovered;

        public string InteractionName => interactionName;
        public float InteractionDistance => interactionDistance;
        protected PlayerCharacter ActiveCharacter => partyManager != null ? partyManager.ActiveCharacter : null;
        protected string PromptPrefix => promptPrefix;

        public void Configure(string name, string prefix, float distance)
        {
            interactionName = name;
            promptPrefix = prefix;
            interactionDistance = distance;
        }

        protected virtual void Awake()
        {
            CacheReferences();
            CacheRendererColors();
        }

        private void OnDisable()
        {
            ClearHover();
        }

        private void OnMouseEnter()
        {
            if (!CanShowInteraction())
            {
                return;
            }

            isHovered = true;
            SetHighlight(true);
            promptUI?.Show(GetPromptText(ActiveCharacter), this);
        }

        private void OnMouseOver()
        {
            if (!CanShowInteraction())
            {
                ClearHover();
                return;
            }

            promptUI?.Show(GetPromptText(ActiveCharacter), this);
        }

        private void OnMouseExit()
        {
            ClearHover();
        }

        private void OnMouseDown()
        {
            if (!CanShowInteraction())
            {
                return;
            }

            var character = ActiveCharacter;
            if (CanInteract(character))
            {
                Interact(character);
            }
        }

        public virtual bool CanInteract(PlayerCharacter character)
        {
            return character != null
                && Vector3.Distance(character.transform.position, transform.position) <= interactionDistance;
        }

        public virtual void Interact(PlayerCharacter character)
        {
        }

        protected virtual string GetPromptText(PlayerCharacter character)
        {
            return $"{promptPrefix} {InteractionName}";
        }

        protected virtual bool CanShowPrompt(PlayerCharacter character)
        {
            return true;
        }

        private void CacheReferences()
        {
            if (gameManager == null)
            {
                gameManager = FindAnyObjectByType<GameManager>();
            }

            if (partyManager == null)
            {
                partyManager = FindAnyObjectByType<PartyManager>();
            }

            if (promptUI == null)
            {
                promptUI = FindAnyObjectByType<InteractionPromptUI>();
            }
        }

        private void CacheRendererColors()
        {
            renderers = GetComponentsInChildren<Renderer>();
            originalColors = new Color[renderers.Length];

            for (var i = 0; i < renderers.Length; i++)
            {
                originalColors[i] = renderers[i].material.color;
            }
        }

        private bool CanShowInteraction()
        {
            CacheReferences();

            return gameManager != null
                && gameManager.CurrentGameMode == GameMode.Exploration
                && partyManager != null
                && partyManager.ActiveCharacter != null
                && CanShowPrompt(partyManager.ActiveCharacter);
        }

        private void SetHighlight(bool highlighted)
        {
            if (renderers == null)
            {
                return;
            }

            for (var i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].material.color = highlighted ? highlightColor : originalColors[i];
                }
            }
        }

        private void ClearHover()
        {
            if (!isHovered)
            {
                return;
            }

            isHovered = false;
            SetHighlight(false);
            promptUI?.Hide(this);
        }
    }
}
