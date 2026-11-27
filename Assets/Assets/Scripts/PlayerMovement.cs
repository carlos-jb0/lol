using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 12f;

    [Header("Salto y gravedad")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -20f;

    private CharacterController controller;
    private Vector3 verticalVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        // Entradas del teclado: WASD o flechas.
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 input = new Vector3(horizontal, 0f, vertical).normalized;

        // El personaje gira hacia la dirección en la que camina.
        if (input.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(input, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        Vector3 horizontalMove = input * moveSpeed;
        controller.Move(horizontalMove * Time.deltaTime);

        // Mantiene al personaje pegado al suelo y aplica gravedad.
        if (controller.isGrounded && verticalVelocity.y < 0f)
            verticalVelocity.y = -2f;

        if (controller.isGrounded && Input.GetButtonDown("Jump"))
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        verticalVelocity.y += gravity * Time.deltaTime;
        controller.Move(verticalVelocity * Time.deltaTime);
    }
}
