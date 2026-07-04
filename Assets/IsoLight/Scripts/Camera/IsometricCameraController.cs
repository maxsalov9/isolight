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
        [SerializeField] private float cameraPitch = 57f;
        [SerializeField] private float yaw = 45f;
        [SerializeField] private float orthographicSize = 16f;
        [SerializeField] private float minZoom = 11f;
        [SerializeField] private float maxZoom = 24f;
        [SerializeField] private float followHeight = 0f;
        [SerializeField] private float followDistance = 17.5f;
        [SerializeField] private Vector2 boundsMin = new Vector2(-25f, -25f);
        [SerializeField] private Vector2 boundsMax = new Vector2(25f, 25f);

        private Vector3 manualPanOffset;
        private UnityEngine.Camera controlledCamera;

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
            set
            {
                minZoom = Mathf.Max(0.01f, value);
                maxZoom = Mathf.Max(minZoom, maxZoom);
                orthographicSize = Mathf.Clamp(orthographicSize, minZoom, maxZoom);
                ApplyCameraSettings();
            }
        }

        public float MaxZoom
        {
            get => maxZoom;
            set
            {
                maxZoom = Mathf.Max(minZoom, value);
                orthographicSize = Mathf.Clamp(orthographicSize, minZoom, maxZoom);
                ApplyCameraSettings();
            }
        }

        public float CameraPitch
        {
            get => cameraPitch;
            set
            {
                cameraPitch = Mathf.Clamp(value, 25f, 89f);
                ApplyCameraSettings();
            }
        }

        public float Yaw
        {
            get => yaw;
            set
            {
                yaw = value;
                ApplyCameraSettings();
            }
        }

        public float OrthographicSize
        {
            get => orthographicSize;
            set
            {
                orthographicSize = Mathf.Clamp(value, minZoom, maxZoom);
                ApplyCameraSettings();
            }
        }

        public float FollowHeight
        {
            get => followHeight;
            set => followHeight = Mathf.Max(0f, value);
        }

        public float FollowDistance
        {
            get => followDistance;
            set => followDistance = Mathf.Max(1f, value);
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
            controlledCamera = GetComponent<UnityEngine.Camera>();
            ApplyCameraSettings();
        }

        private void OnValidate()
        {
            minZoom = Mathf.Max(0.01f, minZoom);
            maxZoom = Mathf.Max(minZoom, maxZoom);
            cameraPitch = Mathf.Clamp(cameraPitch, 25f, 89f);
            orthographicSize = Mathf.Clamp(orthographicSize, minZoom, maxZoom);
            followHeight = Mathf.Max(0f, followHeight);
            followDistance = Mathf.Max(1f, followDistance);
            controlledCamera = GetComponent<UnityEngine.Camera>();
            ApplyCameraSettings();
        }

        public void ConfigureView(float pitch, float viewYaw, float size, float minSize, float maxSize, float height, float distance)
        {
            minZoom = Mathf.Max(0.01f, minSize);
            maxZoom = Mathf.Max(minZoom, maxSize);
            cameraPitch = Mathf.Clamp(pitch, 25f, 89f);
            yaw = viewYaw;
            orthographicSize = Mathf.Clamp(size, minZoom, maxZoom);
            followHeight = Mathf.Max(0f, height);
            followDistance = Mathf.Max(1f, distance);
            ApplyCameraSettings();
        }

        private void LateUpdate()
        {
            HandleZoomInput();
            HandlePanInput();

            var anchor = target != null ? target.position : Vector3.zero;
            var rotation = Quaternion.Euler(cameraPitch, yaw, 0f);
            var desiredPosition = anchor + manualPanOffset - rotation * Vector3.forward * followDistance + Vector3.up * followHeight;
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, boundsMin.x, boundsMax.x);
            desiredPosition.z = Mathf.Clamp(desiredPosition.z, boundsMin.y, boundsMax.y);

            transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
            transform.rotation = rotation;
            ApplyCameraSettings();
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
                orthographicSize = Mathf.Clamp(orthographicSize - scroll * zoomSpeed * 0.05f, minZoom, maxZoom);
                ApplyCameraSettings();
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
            manualPanOffset += pan * (orthographicSize * 0.55f * Time.deltaTime);
        }

        private void ApplyCameraSettings()
        {
            if (controlledCamera == null)
            {
                controlledCamera = GetComponent<UnityEngine.Camera>();
            }

            if (controlledCamera != null)
            {
                controlledCamera.orthographic = true;
                controlledCamera.orthographicSize = orthographicSize;
            }

            transform.rotation = Quaternion.Euler(cameraPitch, yaw, 0f);
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
