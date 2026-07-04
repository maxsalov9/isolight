using System;
using IsoLight.Core;
using IsoLight.Interaction;
using IsoLight.Party;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace IsoLight.Input
{
    public class ClickToMoveController : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private PartyManager partyManager;
        [SerializeField] private UnityEngine.Camera raycastCamera;
        [SerializeField] private LayerMask groundLayer = Physics.DefaultRaycastLayers;
        [SerializeField] private float raycastDistance = 1000f;

        public void SetReferences(GameManager game, PartyManager party, UnityEngine.Camera camera)
        {
            gameManager = game;
            partyManager = party;
            raycastCamera = camera;
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

            if (raycastCamera == null)
            {
                raycastCamera = UnityEngine.Camera.main;
            }
        }

        private void Update()
        {
            if (!CanProcessMovementClick() || !WasLeftMousePressedThisFrame() || IsPointerOverUi())
            {
                return;
            }

            var ray = raycastCamera.ScreenPointToRay(GetPointerPosition());
            if (Physics.Raycast(ray, out var hit, raycastDistance, groundLayer, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.GetComponentInParent<InteractableObject>() != null)
                {
                    return;
                }

                partyManager.MoveActiveCharacterTo(hit.point);
            }
        }

        private bool CanProcessMovementClick()
        {
            return gameManager != null
                && gameManager.CurrentGameMode == GameMode.Exploration
                && partyManager != null
                && partyManager.ActiveCharacter != null
                && raycastCamera != null;
        }

        private static bool WasLeftMousePressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
#else
            return UnityEngine.Input.GetMouseButtonDown(0);
#endif
        }

        private static Vector3 GetPointerPosition()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector3.zero;
#else
            return UnityEngine.Input.mousePosition;
#endif
        }

        private static bool IsPointerOverUi()
        {
            var eventSystemType = Type.GetType("UnityEngine.EventSystems.EventSystem, UnityEngine.UI");
            var currentProperty = eventSystemType?.GetProperty("current");
            var currentEventSystem = currentProperty?.GetValue(null);
            var isPointerOverMethod = eventSystemType?.GetMethod("IsPointerOverGameObject", Type.EmptyTypes);

            return currentEventSystem != null
                && isPointerOverMethod != null
                && (bool)isPointerOverMethod.Invoke(currentEventSystem, null);
        }
    }
}
