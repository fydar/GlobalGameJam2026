using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace GlobalGameJam2026.Gameplay.Overworld.PlayerController
{
    public class PlayerOverworldCharacterController : MonoBehaviour
    {
        [SerializeField] private float movementSpeed;

        [SerializeField] private InputActionReference analogMovement;
        [SerializeField] private InputActionReference orthogonalMovement;

        private void Update()
        {
            // Read the users analog and orthogonal input
            var analogInput = analogMovement.action.ReadValue<Vector2>();
            var orthogonalInput = orthogonalMovement.action.ReadValue<Vector2>();

            // Decide which input to use. Prioritize orthogonal input if it's significant enough.
            Vector2 input;
            if (orthogonalInput.magnitude > 0.5f)
            {
                input = orthogonalInput;
            }
            else
            {
                input = analogInput;

                // Rotate input by direction the camera is facing.
                var camera = Camera.main;
                if (camera != null)
                {
                    var cameraYRotation = Quaternion.Euler(0, camera.transform.eulerAngles.y, 0);
                    input = cameraYRotation * new Vector3(input.x, 0, input.y);
                }
            }

            // Prevent faster diagonal movement
            input = Vector2.ClampMagnitude(input, 1);

            var locomotion = new Vector3(input.x, 0, input.y);

            // Move the character based on input
            transform.position += movementSpeed * Time.deltaTime * locomotion;

            // Snap position to NavMesh
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit myNavHit, 100, -1))
            {
                transform.position = myNavHit.position;
            }
        }
    }
}
