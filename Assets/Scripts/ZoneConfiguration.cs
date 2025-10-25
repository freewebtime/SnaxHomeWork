using System;
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// Configuration object that defines a streamed Zone.
    /// This configuration is used by ZoneStreamer to load/unload scenes based on the player position.
    /// Separated from ZoneConfigurationAuthoring because 
    /// there is a chance that this data comes from other sources like config files .
    /// </summary>
    [Serializable]
    public class ZoneConfiguration
    {
        [Tooltip("Name of the streamed Zone")]
        public string Name;

        [Tooltip("Zone is a circle with this radius")]
        public float ZoneRadius;

        [Tooltip("Center of the Zone in global coordinates")]
        public Vector3 AreaCenter;

        [Tooltip("Addressable name of the scene that is loaded when the zone is loaded")]
        public string SceneAddress;

        [Tooltip("Radius around the zone center where the zone will be loaded")]
        public float LoadZoneRadius;

        [Tooltip("Radius around the zone center where the zone will be unloaded")]
        public float UnloadZoneRadius;

        public static ZoneConfiguration Default => new ZoneConfiguration
        {
            Name = string.Empty,
            ZoneRadius = 0,
            AreaCenter = Vector3.zero,
            SceneAddress = string.Empty,
            LoadZoneRadius = 2,
            UnloadZoneRadius = 3
        };
    }
}