using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 12f;
    [SerializeField] private float sprintMultiplier = 1.65f;

    [Header("Resistencia")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrain = 24f;
    [SerializeField] private float staminaRecovery = 17f;
    [SerializeField] private float recoveryDelay = 0.8f;

    [Header("Salto y gravedad")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float jumpBufferTime = 0.15f;

    [Header("Limites del reino")]
    [SerializeField] private bool useWorldBounds = true;
    [SerializeField] private Vector2 worldMin = new Vector2(-94f, -94f);
    [SerializeField] private Vector2 worldMax = new Vector2(94f, 94f);

    private CharacterController controller;
    private Vector3 verticalVelocity;
    private float stamina;
    private float recoveryTimer;
    private float jumpBufferCounter;
    private float coyoteTimeCounter;
    private const float CoyoteTime = 0.16f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        stamina = maxStamina;
    }

    public void ConfigureWorldBounds(Vector2 minimum, Vector2 maximum)
    {
        worldMin = minimum;
        worldMax = maximum;
        useWorldBounds = true;
    }

    private void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(horizontal, 0f, vertical).normalized;

        Transform cameraTransform = Camera.main != null ? Camera.main.transform : null;
        Vector3 forward = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
        Vector3 right = cameraTransform != null ? cameraTransform.right : Vector3.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * input.z + right * input.x).normalized;
        bool sprintKey = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool wantsSprint = sprintKey && input.sqrMagnitude > 0.01f && stamina > 0.1f;

        if (wantsSprint)
        {
            stamina = Mathf.Max(0f, stamina - staminaDrain * Time.deltaTime);
            recoveryTimer = recoveryDelay;
        }
        else
        {
            recoveryTimer = Mathf.Max(0f, recoveryTimer - Time.deltaTime);
            if (recoveryTimer <= 0f)
                stamina = Mathf.Min(maxStamina, stamina + staminaRecovery * Time.deltaTime);
        }

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
                rotationSpeed * Time.deltaTime);
        }

        float speed = moveSpeed * (wantsSprint ? sprintMultiplier : 1f);
        controller.Move(moveDirection * speed * Time.deltaTime);

        // Comprobar el suelo después del desplazamiento horizontal evita perder el salto
        // en el mismo frame en que el CharacterController vuelve a tocar el terreno.
        bool grounded = CheckGrounded();
        if (grounded)
            coyoteTimeCounter = CoyoteTime;
        else
            coyoteTimeCounter = Mathf.Max(0f, coyoteTimeCounter - Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Space))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter = Mathf.Max(0f, jumpBufferCounter - Time.deltaTime);

        if (grounded && verticalVelocity.y < 0f)
            verticalVelocity.y = -2f;

        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
        }

        verticalVelocity.y += gravity * Time.deltaTime;
        controller.Move(Vector3.up * verticalVelocity.y * Time.deltaTime);

        KeepInsideWorldBounds();
    }

    public void SetWorldBoundsActive(bool active)
    {
        useWorldBounds = active;
    }

    public void ResetVerticalMotion()
    {
        verticalVelocity = Vector3.zero;
        jumpBufferCounter = 0f;
        coyoteTimeCounter = 0f;
    }

    private bool CheckGrounded()
    {
        if (controller.isGrounded) return true;

        // El origen del rayo parte del centro de la cápsula y busca el piso
        // hasta un poco más allá de la base para tolerar pequeños desniveles.
        Bounds bounds = controller.bounds;
        RaycastHit[] hits = Physics.RaycastAll(
            bounds.center, Vector3.down, bounds.extents.y + 0.22f,
            ~0, QueryTriggerInteraction.Ignore);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null) continue;
            Transform hitTransform = hit.collider.transform;
            if (hitTransform == transform || hitTransform.IsChildOf(transform)) continue;
            return true;
        }
        return false;
    }

    private void KeepInsideWorldBounds()
    {
        if (!useWorldBounds) return;

        Vector3 position = transform.position;
        Vector3 clamped = new Vector3(
            Mathf.Clamp(position.x, worldMin.x, worldMax.x),
            position.y,
            Mathf.Clamp(position.z, worldMin.y, worldMax.y));

        Vector3 correction = clamped - position;
        if (correction.sqrMagnitude > 0.0001f)
            controller.Move(correction);
    }

    private void OnGUI()
    {
        const float width = 190f;
        const float height = 16f;
        float x = 18f;
        float y = Screen.height - 38f;
        float ratio = maxStamina <= 0f ? 0f : Mathf.Clamp01(stamina / maxStamina);

        GUI.Box(new Rect(x - 5f, y - 5f, width + 10f, height + 10f), GUIContent.none);
        GUI.Box(new Rect(x, y, width * ratio, height), GUIContent.none);
        GUI.Label(new Rect(x + width + 12f, y - 2f, 110f, 20f),
            "Resistencia " + Mathf.CeilToInt(stamina));
    }
}
