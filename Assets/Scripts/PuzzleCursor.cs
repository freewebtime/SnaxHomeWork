using Unity.Mathematics;
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// Represents a cursor visuals on the grid.
    /// Controlled externally from PuzzleController.
    /// </summary>
    public class PuzzleCursor : MonoBehaviour
    {
        /// <summary>
        /// Cached transform
        /// </summary>
        private Transform _transform;

        private void Awake()
        {
            _transform = transform;
        }

        /// <summary>
        /// Places the cursor in a 3d space based on a 2d position (adds y coordinate)
        /// </summary>
        public void SetPosition(float2 newPosition)
        { 
            if (_transform == null)
            {
                return;
            }

            _transform.position = new Vector3(newPosition.x, 0, newPosition.y);
        }
    }
}