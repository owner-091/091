using UnityEngine;

namespace TrainGame
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class ConductorTopDownController : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float moveSpeed = 3f;

        private Rigidbody2D body;
        private Vector2 moveInput;

        public Vector2 MoveInput => moveInput;
        public bool IsMoving => moveInput.sqrMagnitude > 0.001f;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.None;
        }

        private void Update()
        {
            moveInput = new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical"));

            if (moveInput.sqrMagnitude > 1f)
                moveInput.Normalize();
        }

        private void FixedUpdate()
        {
            body.MovePosition(body.position + moveInput * (moveSpeed * Time.fixedDeltaTime));
        }
    }
}
