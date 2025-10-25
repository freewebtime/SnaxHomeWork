using System;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// Controlls the cursor movement on the puzzle grid based on user input
    /// and notifies ZoneStreamer about the cursor position changes.
    /// </summary>
    public class PuzzleController : MonoBehaviour
    {
        [Tooltip("Size of one cell on the grid in meters")]
        public float2 CellSize = new int2(1, 1);

        [Tooltip("Size of the whole grid in cells")]
        public int2 GridSize = new int2(10, 10);

        [Tooltip("The coordinate of the cell the Cursor is occupying currently")]
        [SerializeField]
        private int2 _cursorCoordinate = int2.zero;
        /// <summary>
        /// Gets or sets the coordinate of the cell the Cursor is occupying currently.
        /// Triggers OnCursorPositionChanged if value has changed.
        /// </summary>
        public int2 CursorCoordinate
        {
            get => _cursorCoordinate;
            set
            {
                if (_cursorCoordinate.Equals(value))
                {
                    return;
                }

                _cursorCoordinate = value;
                OnCursorPositionChanged();
            }
        }

        /// <summary>
        /// Cursor position in 2d world space (meters) calculated based on cell size and CursorCoordinate.
        /// </summary>
        public float2 CursorPosition2D => _cursorCoordinate * CellSize;

        /// <summary>
        /// Cursor position in 3d world space (meters) calculated based on CursorPosition2D.
        /// </summary>
        public float3 CursorPosition3D
        {
            get
            {
                var pos2d = CursorPosition2D;
                return new float3(pos2d.x, 0, pos2d.y);
            }
        }

        [Tooltip("Key to move the cursor up")]
        public KeyCode MoveUpKey = KeyCode.W;

        [Tooltip("Key to move the cursor down")]
        public KeyCode MoveDownKey = KeyCode.S;

        [Tooltip("Key to move the cursor left")]
        public KeyCode MoveLeftKey = KeyCode.A;

        [Tooltip("Key to move the cursor right")]
        public KeyCode MoveRightKey = KeyCode.D;
         
        [Tooltip("Reference to the PuzzleCursor that represents the cursor visuals on the grid")]
        public PuzzleCursor PuzzleCursor;

        [Tooltip("Input is ignored during this amount of seconds after previous input")]
        public float InputCooldown = 0.1f;

        [Tooltip("Exposed for debug, the time when last input was processed. Used for input cooldown")]
        public float LastMovementTimestamp = float.MinValue;

        [Tooltip("Reference to the ZoneStreamer that streams zones based on cursor position")]
        public ZoneStreamer ZoneStreamer;

        private void Start()
        {
            ValidateAll();

            OnCursorPositionChanged();
        }

        private void Update()
        {
            UpdateCursor();
        }

        /// <summary>
        /// Updates a cursor position based on user input.
        /// </summary>
        private void UpdateCursor()
        {
            // check the input cooldown
            if (Time.time - LastMovementTimestamp <= InputCooldown)
            {
                // ignore the input if we're still on cooldown
                return;
            }

            var horizontalMovement = 0;
            var verticalMovement = 0;

            // vertical movement
            if (Input.GetKeyUp(MoveUpKey)) verticalMovement += 1;
            if (Input.GetKeyUp(MoveDownKey)) verticalMovement -= 1;

            // horizontal movement
            if (Input.GetKeyUp(MoveRightKey)) horizontalMovement += 1;
            if (Input.GetKeyUp(MoveLeftKey)) horizontalMovement -= 1;

            var movement = new int2(horizontalMovement, verticalMovement);
            var newPosition = math.clamp(CursorCoordinate + movement, int2.zero, GridSize - new int2(1, 1));

            // now take actions only if new position differs from the previous one to save performance
            if (!CursorCoordinate.Equals(newPosition))
            {
                CursorCoordinate = newPosition;
                LastMovementTimestamp = Time.time;
            }
        }

        /// <summary>
        /// Called when CursorCoordinate changes.
        /// Updates visuals and notifies ZoneStreamer about the change.
        /// </summary>
        private void OnCursorPositionChanged()
        {
            UpdateCursorVisuals();
            UpdateZoneStreamer();
        }

        /// <summary>
        /// Updates the PuzzleCursor visuals based on the current CursorCoordinate.
        /// </summary>
        private void UpdateCursorVisuals()
        {
            if (PuzzleCursor != null)
            {
                PuzzleCursor.SetPosition(CursorCoordinate * CellSize);
            }
        }

        /// <summary>
        /// Asks ZoneStreamer to stream zones based on the current cursor position.
        /// </summary>
        private void UpdateZoneStreamer()
        {
            if (ZoneStreamer != null)
            {
                // cursor radius is needed for calculating which zones to load and which to unload.
                // cursor radius is substracted from the distance to zone boundaries.
                // This way we can calculate the distance from the edge of the cursor, not from its center.
                // And yes, the cursor is nor a circle, but this approximation is good enough.
                var cursorRadius = CellSize.x / 2f; 
                ZoneStreamer.StreamZones(CursorPosition3D, cursorRadius);
            }
        }

        /// <summary>
        /// Validates if all required references are set.
        /// </summary>
        private void ValidateAll()
        {
            Debug.Assert(ZoneStreamer != null, "PuzzleController: ZoneStreamer reference is not set!");
            Debug.Assert(PuzzleCursor != null, "PuzzleController: PuzzleCursor reference is not set!");
        }
    }
}