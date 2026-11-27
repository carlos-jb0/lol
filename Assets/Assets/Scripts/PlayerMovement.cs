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

    private CharacterController controller;
    private Vector3 verticalVelocity;
    private float stamina;
    private float recoveryTimer;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        stamina = maxStamina;
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
        bool wantsSprint = Input.GetKey(KeyCode.LeftShift) && input.sqrMagnitude > 0.01f && stamina > 0.5f;
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

        if (controller.isGrounded && verticalVelocity.y < 0f)
            verticalVelocity.y = -2f;

        if (controller.isGrounded && Input.GetButtonDown("Jump"))
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        verticalVelocity.y += gravity * Time.deltaTime;
        controller.Move(verticalVelocity * Time.deltaTime);
    }

    private void OnGUI()
    {
        const float width = 190f;
        const float height = 16f;
        float x = 18f;
        float y = Screen.height - 38f;
        GUI.Box(new Rect(x - 5f, y - 5f, width + 10f, height + 10f), GUIContent.none);
        GUI.Box(new Rect(x, y, width * (maxStamina <= 0f ? 0f : stamina / maxStamina), height), GUIContent.none);
        GUI.Label(new Rect(x + width + 12f, y - 2f, 90f, 20f), "Resistencia");
    }
}