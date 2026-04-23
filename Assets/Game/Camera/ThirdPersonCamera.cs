using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Third-person camera that orbits around a target and follows it.
/// Attach this to your Camera GameObject (or a camera rig).
///
/// Setup:
///  1. Attach this script to your Camera.
///  2. Assign the 'target' field to your player Transform.
///  3. In your Input Actions asset, make sure you have:
///       - An action named "Look" (Value, Vector2) bound to Mouse/Delta
///       - (Optional) "Zoom" (Value, float) bound to Mouse/Scroll Y
///  4. Assign the generated C# class (or PlayerInput component) reference, OR
///     use the manual [SerializeField] InputActionReference fields below.
/// </summary>
public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The transform the camera orbits and follows (your player).")]
    public Transform target;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private InputActionReference zoomAction; // optional scroll zoom

    [Header("Orbit Settings")]
    [SerializeField] private float horizontalSensitivity = 0.2f;
    [SerializeField] private float verticalSensitivity   = 0.2f;

    [Tooltip("Vertical look limits in degrees.")]
    [SerializeField] private float minPitch = -30f;
    [SerializeField] private float maxPitch =  60f;

    [Header("Distance")]
    [SerializeField] private float distance        = 5f;
    [SerializeField] private float minDistance     = 1.5f;
    [SerializeField] private float maxDistance     = 12f;
    [SerializeField] private float zoomSensitivity = 2f;
    [SerializeField] private float zoomSmoothing   = 8f;

    [Header("Follow Settings")]
    [SerializeField] private Vector3 targetOffset  = new Vector3(0f, 1.6f, 0f); // head height
    [SerializeField] private float   followSmoothing = 10f;

    [Header("Collision")]
    [SerializeField] private bool   enableCollision = true;
    [SerializeField] private float  collisionRadius = 0.2f;
    [SerializeField] private LayerMask collisionLayers = ~0; // everything by default

    // Runtime state
    private float   _yaw;          // horizontal angle
    private float   _pitch;        // vertical angle
    private float   _targetDist;   // desired distance (lerped)
    private Vector3 _followPos;    // smoothed follow position

    // ---------------------------------------------------------------
    private void Awake()
    {
        if (target == null)
            Debug.LogWarning("[ThirdPersonCamera] No target assigned!", this);

        // Initialise smoothed follow position so camera doesn't snap on frame 1
        if (target != null)
            _followPos = target.position;

        _targetDist = distance;

        // Initialise yaw to match current camera facing so there's no jump
        Vector3 angles = transform.eulerAngles;
        _yaw   = angles.y;
        _pitch = angles.x;

        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible   = false;
    }

    private void OnEnable()
    {
        lookAction?.action.Enable();
        zoomAction?.action.Enable();
    }

    private void OnDisable()
    {
        lookAction?.action.Disable();
        zoomAction?.action.Disable();
    }

    // ---------------------------------------------------------------
    // Use LateUpdate so the camera moves AFTER the player has moved.
    private void LateUpdate()
    {
        if (target == null) return;

        HandleInput();
        SmoothFollow();
        ApplyTransform();
    }

    // ---------------------------------------------------------------
    private void HandleInput()
    {
        // --- Look ---
        if (lookAction != null)
        {
            Vector2 lookDelta = lookAction.action.ReadValue<Vector2>();
            _yaw   += lookDelta.x * horizontalSensitivity;
            _pitch -= lookDelta.y * verticalSensitivity;   // subtract so moving mouse up pitches up
            _pitch  = Mathf.Clamp(_pitch, minPitch, maxPitch);
        }

        // --- Zoom (scroll wheel) ---
        if (zoomAction != null)
        {
            float scroll = zoomAction.action.ReadValue<float>();
            _targetDist -= scroll * zoomSensitivity * Time.deltaTime;
            _targetDist  = Mathf.Clamp(_targetDist, minDistance, maxDistance);
        }

        // Smooth the actual distance toward target distance
        distance = Mathf.Lerp(distance, _targetDist, zoomSmoothing * Time.deltaTime);
    }

    // ---------------------------------------------------------------
    private void SmoothFollow()
    {
        _followPos = Vector3.Lerp(
            _followPos,
            target.position + targetOffset,
            followSmoothing * Time.deltaTime
        );
    }

    // ---------------------------------------------------------------
    private void ApplyTransform()
    {
        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3 desiredPos  = _followPos - rotation * Vector3.forward * distance;

        // Optional camera collision — push camera forward if geometry is in the way
        if (enableCollision)
            desiredPos = ResolveCollision(_followPos, desiredPos);

        transform.position = desiredPos;
        transform.LookAt(_followPos);
    }

    // ---------------------------------------------------------------
    private Vector3 ResolveCollision(Vector3 from, Vector3 desired)
    {
        Vector3 dir  = desired - from;
        float   dist = dir.magnitude;

        if (Physics.SphereCast(from, collisionRadius, dir.normalized, out RaycastHit hit,
                               dist, collisionLayers, QueryTriggerInteraction.Ignore))
        {
            // Pull the camera in front of the hit point
            return from + dir.normalized * (hit.distance - collisionRadius);
        }

        return desired;
    }

    // ---------------------------------------------------------------
    // Unlock cursor when the application loses focus (optional quality-of-life)
    private void OnApplicationFocus(bool hasFocus)
    {
        //if (hasFocus)
        //{
        //    Cursor.lockState = CursorLockMode.Locked;
        //    Cursor.visible   = false;
        //}
        //else
        //{
        //    Cursor.lockState = CursorLockMode.None;
        //    Cursor.visible   = true;
        //}
    }
}