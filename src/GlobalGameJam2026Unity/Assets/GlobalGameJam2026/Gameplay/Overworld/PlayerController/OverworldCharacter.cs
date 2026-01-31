using UnityEngine;
using UnityEngine.AI;

namespace GlobalGameJam2026.Gameplay.Overworld.PlayerController
{
    public class OverworldCharacter : MonoBehaviour
    {


        private void Update()
        {
            // Snap position to NavMesh
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit myNavHit, 100, -1))
            {
                transform.position = myNavHit.position;
            }
        }
    }
}
