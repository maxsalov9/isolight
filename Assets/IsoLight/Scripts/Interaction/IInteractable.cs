using IsoLight.Characters;

namespace IsoLight.Interaction
{
    public interface IInteractable
    {
        string InteractionName { get; }
        float InteractionDistance { get; }
        bool CanInteract(PlayerCharacter character);
        void Interact(PlayerCharacter character);
    }
}
