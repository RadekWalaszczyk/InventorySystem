using UnityEngine;

namespace Player
{
    public class MoveCamera : MonoBehaviour
    {
        [SerializeField] Transform player;

        void Update()
        {
            transform.position = player.transform.position;
        }
    }
}