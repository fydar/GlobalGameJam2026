using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Windows;

namespace GlobalGameJam2026.Gameplay.Overworld.PlayerController
{
    public class OverworldCharacter : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private float movementSpeed;
        
        private OrthogonalDirection orthogonalFacingDirection = OrthogonalDirection.Down;

        public OrthogonalDirection OrthogonalFacingDirection
        {
            get
            {
                return orthogonalFacingDirection;
            }
            set
            {
                if (orthogonalFacingDirection != value)
                {
                    orthogonalFacingDirection = value;

                    // Update animator parameters based on facing direction
                    animator.SetFloat("OrthogonalFacingX", orthogonalFacingDirection.X);
                    animator.SetFloat("OrthogonalFacingY", orthogonalFacingDirection.Y);
                }
            }
        }

        public Vector2 CameraFacingDirection
        {
            get
            {
                var camera = Camera.main;
                if (camera == null)
                {
                    return Vector2.up;
                }

                var cameraYRotation = Quaternion.Euler(0, camera.transform.eulerAngles.y, 0);

                var orthogonalFacingVector = OrthogonalFacingDirection.ToVector2();

                var direction = cameraYRotation * new Vector3(orthogonalFacingVector.x, 0.0f, orthogonalFacingVector.y);
                return new Vector2(direction.x, direction.z).normalized;
            }
        }

        private void Update()
        {
            var cameraFacingDirection = CameraFacingDirection;
            animator.SetFloat("CameraFacingX", cameraFacingDirection.x);
            animator.SetFloat("CameraFacingY", cameraFacingDirection.y);
        }

        public void MoveOrthogonally(Vector2 input)
        {
            // Round the analog input to the nearest orthogonal direction.
            orthogonalFacingDirection = OrthogonalDirection.FromVector2(input);

            MakeMovement(input);
        }

        public void MoveAnalog(Vector2 input)
        {
            // Rotate input by direction the camera is facing.
            var camera = Camera.main;
            if (camera != null)
            {
                var cameraYRotation = Quaternion.Euler(0, camera.transform.eulerAngles.y, 0);
                input = cameraYRotation * new Vector3(input.x, 0, input.y);
            }

            // Round the analog input to the nearest orthogonal direction.
            orthogonalFacingDirection = OrthogonalDirection.FromVector2(input);

            MakeMovement(input);
        }

        private void MakeMovement(Vector2 movement)
        {
            var movementLocomotion = new Vector3(movement.x, 0, movement.y);

            // Move the character based on input
            transform.position += movementSpeed * Time.deltaTime * movementLocomotion;

            // Snap position to NavMesh
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit myNavHit, 100, -1))
            {
                transform.position = myNavHit.position;
            }
        }
    }
}
