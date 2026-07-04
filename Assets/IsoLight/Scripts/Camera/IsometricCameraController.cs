using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace IsoLight.Camera
{
    public class IsometricCameraController : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float followSpeed = 8f;
        [SerializeField] private float zoomSpeed = 4f;
        [SerializeField] private float minZoom = 6f;
        [SerializeField] private float maxZoom = 22f;
        [SerializeField] private Vector2 boundsMin = new Vector2(-25f, -25f);
        [SerializeField] private Vector2 boundsMax = new Vector2(25f, 25f);
        [SerializeField] private Vector3 isometricRotation = new Vector3(60f, 45f, 0f);
        [SerializeField] private Vector3 defaultOffset = new Vector3(-8f, 10f, -8f);

        private float zoomDistance;
        private Vector3 manualPanOffset;

        public Transform Target
        {
            get => target;
            set => target = value;
        }

        public float FollowSpeed
        {
            get => followSpeed;
            set => followSpeed = Mathf.Max(0.01f, value);
        }

        public float ZoomSpeed
        {
            get => zoomSpeed;
            set => zoomSpeed = Mathf.Max(0.01f, value);
        }

        public float MinZoom
        {
            get => minZoom;
            set => minZoom = Mathf.Max(0.01f, value);
        }

        public float MaxZoom
        {
            get => maxZoom;
            set => maxZoom = Mathf.Max(minZoom, value);
        }

        public Vector2 BoundsMin
        {
            get => boundsMin;
            set => boundsMin = value;
        }

        public Vector2 BoundsMax
        {
            get => boundsMax;
            set => boundsMax = value;
        }

        private void Awake()
        {
            zoomDistance = Mathf.Clamp(defaultOffset.magnitude, minZoom, maxZoom);
            transform.rotation = Quaternion.Euler(isometricRotation);
        }

        private void LateUpdate()
        {
            HandleZoomInput();
            HandlePanInput();

            var anchor = target != null ? target.position : Vector3.zero;
            var desiredPosition = anchor + manualPanOffset + defaultOffset.normalized * zoomDistance;
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, boundsMin.x, boundsMax.x);
            desiredPosition.z = Mathf.Clamp(desiredPosition.z, boundsMin.y, boundsMax.y);

            transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(isometricRotation);
        }

        private void HandleZoomInput()
        {
            var scroll = 0f;

#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                scroll = Mouse.current.scroll.ReadValue().y;
            }
#else
            scroll = UnityEngine.Input.mouseScrollDelta.y;
#endif

            if (Mathf.Abs(scroll) > Mathf.Epsilon)
            {
                zoomDistance = Mathf.Clamp(zoomDistance - scroll * zoomSpeed * 0.01f, minZoom, maxZoom);
            }
        }

        private void HandlePanInput()
        {
            var input = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                input.x = ReadAxis(keyboard.aKey.isPressed, keyboard.dKey.isPressed);
                input.y = ReadAxis(keyboard.sKey.isPressed, keyboard.wKey.isPressed);
            }
#else
            input.x = UnityEngine.Input.GetAxisRaw("Horizontal");
            input.y = UnityEngine.Input.GetAxisRaw("Vertical");
#endif

            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }

            var pan = new Vector3(input.x, 0f, input.y);
            manualPanOffset += pan * (zoomDistance * 0.5f * Time.deltaTime);
        }

        private static float ReadAxis(bool negativePressed, bool positivePressed)
        {
            if (negativePressed == positivePressed)
            {
                return 0f;
            }

            return positivePressed ? 1f : -1f;
        }
    }
}
