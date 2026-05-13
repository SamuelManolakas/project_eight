using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Third-person camera that orbits around a target and follows it.
/// Attach this to your Camera GameObject (or a camera rig).
///
/// Setup:
///  1. Attach this script to your Camera.
///  2. Assign 'target' to your player Transform.
///  3. In your Input Actions asset, add:
///       - "Look"   (Value, Vector2) bound to Mouse/Delta
///       - "Zoom"   (Value, float)   bound to Mouse/Scroll Y  [optional]
///       - "LockOn" (Button)         bound to your preferred key/button
///  4. Assign the InputActionReferences in the Inspector.
///  5. Optionally assign a 'lockOnTarget' in the Inspector, or set it at
///     runtime via SetLockOnTarget().
/// </summary>
public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The transform the camera orbits and follows (your player).")]
    [SerializeField] public Transform target;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private InputActionReference zoomAction;   // optional scroll zoom
    [SerializeField] private InputActionReference lockOnAction; // toggle lock-on

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
    [SerializeField] private Vector3 targetOffset    = new Vector3(0f, 1.6f, 0f);
    [SerializeField] private float   followSmoothing = 10f;

    [Header("Collision")]
    [SerializeField] private bool        enableCollision  = true;
    [SerializeField] private float       collisionRadius  = 0.2f;
    [SerializeField] private LayerMask   collisionLayers  = ~0;

    [Header("Lock-On")]
    [Tooltip("The transform the camera looks at while locked on.")]
    [SerializeField] private Transform lockOnTarget;
    [Tooltip("Offset applied to the lock-on target position (e.g. chest height).")]
    [SerializeField] private Vector3 lockOnOffset = new Vector3(0f, 1f, 0f);
    [Tooltip("How quickly the camera rotates to face the lock-on target.")]
    [SerializeField] private float lockOnRotationSpeed = 8f;

    // Runtime state
    private float   _yaw;
    private float   _pitch;
    private float   _targetDist;
    private Vector3 _followPos;
    private bool    _cursorLocked;
    private bool    _isLockedOn;

    // ---------------------------------------------------------------
    private void Awake()
    {
        if (target == null)
            Debug.LogWarning("[ThirdPersonCamera] No target assigned!", this);

        if (target != null)
            _followPos = target.position;

        _targetDist = distance;

        Vector3 angles = transform.eulerAngles;
        _yaw   = angles.y;
        _pitch = angles.x;

        //SetCursorLocked(true);
    }

    private void OnEnable()
    {
        lookAction?.action.Enable();
        zoomAction?.action.Enable();
        lockOnAction?.action.Enable();
    }

    private void OnDisable()
    {
        lookAction?.action.Disable();
        zoomAction?.action.Disable();
        lockOnAction?.action.Disable();
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
        // --- Toggle cursor lock with Escape ---
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            SetCursorLocked(!_cursorLocked);

        // --- Toggle lock-on ---
        if (lockOnAction != null && lockOnAction.action.WasPressedThisFrame())
        {
            if (_isLockedOn || lockOnTarget == null)
                SetLockOn(false);
            else
                SetLockOn(true);
        }

        // Only orbit with mouse when the cursor is locked and not locked on
        if (!_cursorLocked || _isLockedOn) return;

        // --- Look ---
        if (lookAction != null)
        {
            Vector2 lookDelta = lookAction.action.ReadValue<Vector2>();
            _yaw   += lookDelta.x * horizontalSensitivity;
            _pitch -= lookDelta.y * verticalSensitivity;
            _pitch  = Mathf.Clamp(_pitch, minPitch, maxPitch);
        }

        // --- Zoom (scroll wheel) ---
        if (zoomAction != null)
        {
            float scroll = zoomAction.action.ReadValue<float>();
            _targetDist -= scroll * zoomSensitivity * Time.deltaTime;
            _targetDist  = Mathf.Clamp(_targetDist, minDistance, maxDistance);
        }

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
        if (_isLockedOn && lockOnTarget != null)
        {
            // --- Lock-on mode ---
            // Camera stays at its orbited position but rotates to face the lock-on target.
            Quaternion currentRotation = Quaternion.Euler(_pitch, _yaw, 0f);
            Vector3 desiredPos = _followPos - currentRotation * Vector3.forward * distance;

            if (enableCollision)
                desiredPos = ResolveCollision(_followPos, desiredPos);

            transform.position = desiredPos;

            // Smoothly rotate toward the lock-on target
            Vector3    lookAtPoint = lockOnTarget.position + lockOnOffset;
            Quaternion targetRot   = Quaternion.LookRotation(lookAtPoint - transform.position);
            transform.rotation     = Quaternion.Slerp(transform.rotation, targetRot,
                                                      lockOnRotationSpeed * Time.deltaTime);

            // Sync _yaw/_pitch to the smoothed rotation so there's no snap on unlock
            _yaw   = transform.eulerAngles.y;
            _pitch = transform.eulerAngles.x;
            if (_pitch > 180f) _pitch -= 360f; // normalise to -180..180 so clamp works correctly
        }
        else
        {
            // --- Normal orbit mode ---
            Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            Vector3 desiredPos  = _followPos - rotation * Vector3.forward * distance;

            if (enableCollision)
                desiredPos = ResolveCollision(_followPos, desiredPos);

            transform.position = desiredPos;
            transform.LookAt(_followPos);
        }
    }

    // ---------------------------------------------------------------
    private Vector3 ResolveCollision(Vector3 from, Vector3 desired)
    {
        Vector3 dir  = desired - from;
        float   dist = dir.magnitude;

        if (Physics.SphereCast(from, collisionRadius, dir.normalized, out RaycastHit hit,
                               dist, collisionLayers, QueryTriggerInteraction.Ignore))
        {
            return from + dir.normalized * (hit.distance - collisionRadius);
        }

        return desired;
    }

    // ---------------------------------------------------------------
    private void SetCursorLocked(bool locked)
    {
        _cursorLocked    = locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible   = !locked;
    }

    /// <summary>Enable or disable lock-on mode.</summary>
    public void SetLockOn(bool active)
    {
        if (active && lockOnTarget == null)
        {
            Debug.LogWarning("[ThirdPersonCamera] Cannot lock on: no lockOnTarget assigned.");
            return;
        }
        _isLockedOn = active;
    }

    /// <summary>Assign a new lock-on target at runtime (e.g. from an enemy selector).</summary>
    public void SetLockOnTarget(Transform newTarget)
    {
        lockOnTarget = newTarget;
        if (newTarget == null)
            SetLockOn(false);
    }
}