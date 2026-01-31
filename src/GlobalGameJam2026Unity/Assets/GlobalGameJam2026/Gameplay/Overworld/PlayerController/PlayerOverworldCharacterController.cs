using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace GlobalGameJam2026.Gameplay.Overworld.PlayerController
{
    public class PlayerOverworldCharacterController : MonoBehaviour
    {
        [SerializeField] private OverworldCharacter overworldCharacter;

        [SerializeField] private InputActionReference analogMovement;
        [SerializeField] private InputActionReference orthogonalMovement;

        private void Update()
        {
            // Read the users analog and orthogonal input
            var analogInput = analogMovement.action.ReadValue<Vector2>();
            var orthogonalInput = orthogonalMovement.action.ReadValue<Vector2>();

            // Decide which input to use. Prioritize orthogonal input if it's significant enough.
            if (orthogonalInput.magnitude > 0.5f)
            {
                // Prevent faster diagonal movement
                orthogonalInput = Vector2.ClampMagnitude(orthogonalInput, 1);

                overworldCharacter.MoveOrthogonally(orthogonalInput);
            }
            else if (analogInput.magnitude > 0.0001f)
            {
                // Prevent faster diagonal movement
                analogInput = Vector2.ClampMagnitude(analogInput, 1);

                overworldCharacter.MoveAnalog(analogInput);
            }
        }
    }
}
