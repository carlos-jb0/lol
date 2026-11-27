using UnityEngine;

public class SkyrimThirdPersonCamera : MonoBehaviour
{
    [Header("Objetivo")]
    [SerializeField] private Transform target;
    [SerializeField] private float pivotHeight = 1.45f;

    [Header("Orbita")]
    [SerializeField] private float distance = 5.2f;
    [SerializeField] private float shoulderOffset = 0.55f;
    [SerializeField] private float sensitivity = 2.8f;
    [SerializeField] private float minPitch = -18f;
    [SerializeField] private float maxPitch = 68f;
    [SerializeField] private float positionSmooth = 16f;
    [SerializeField] private float rotationSmooth = 18f;

    [Header("Colisiones")]
    [SerializeField] private float collisionRadius = 0.22f;
    [SerializeField] private LayerMask collisionMask = ~0;

    private float yaw;
    private float pitch = 16f;
    private Vector3 currentVelocity;

    public Transform Target => target;

    public void Configure(Transform followTarget)
    {
        target = followTarget;
        if (target != null) yaw = target.eulerAngles.y;
    }

    private void Start()
    {
        if (target != null) yaw = target.eulerAngles.y;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (target == null || Cursor.lockState != CursorLockMode.Locked) return;
        yaw += Input.GetAxis("Mouse X") * sensitivity * 3f;
        pitch -= Input.GetAxis("Mouse Y") * sensitivity * 2f;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 pivot = target.position + Vector3.up * pivotHeight;
        Quaternion orbit = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 direction = orbit * Vector3.back;
        Vector3 desired = pivot + direction * distance + orbit * Vector3.right * shoulderOffset;

        RaycastHit[] hits = Physics.SphereCastAll(
            pivot, collisionRadius, (desired - pivot).normalized,
            Vector3.Distance(pivot, desired), collisionMask, QueryTriggerInteraction.Ignore);

        float allowedDistance = Vector3.Distance(pivot, desired);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null || hit.collider.transform == target ||
                hit.collider.transform.IsChildOf(target)) continue;
            allowedDistance = Mathf.Min(allowedDistance, Mathf.Max(0.35f, hit.distance - collisionRadius));
        }

        Vector3 safePosition = pivot + (desired - pivot).normalized * allowedDistance;
        transform.position = Vector3.SmoothDamp(transform.position, safePosition, ref currentVelocity,
            1f / Mathf.Max(1f, positionSmooth));
        Quaternion lookRotation = Quaternion.LookRotation(pivot - transform.position, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSmooth * Time.deltaTime);
    }
}