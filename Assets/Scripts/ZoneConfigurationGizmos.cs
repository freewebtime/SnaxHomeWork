using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// Gizmo drawer for ZoneConfigurationAuthoring.
    /// Shows the load and unload zones in the scene view.
    /// </summary>
    public class ZoneConfigurationGizmos : MonoBehaviour
    {
        [Tooltip("Reference to the ZoneConfigurationAuthoring component.")]
        public ZoneConfigurationAuthoring ZoneConfigurationAuthoring;

        [Tooltip("Color for the load zone area gizmo.")]
        public Color LoadZoneAreaColor = Color.green;

        [Tooltip("Color for the unload zone area gizmo.")]
        public Color UnloadZoneAreaColor = Color.red;

        private void OnDrawGizmos()
        {
            if (ZoneConfigurationAuthoring == null)
            {
                return;
            }

            var zoneConfig = ZoneConfigurationAuthoring.GetZoneConfiguration();

            var oldColor = Gizmos.color;

            Gizmos.color = LoadZoneAreaColor;
            Gizmos.DrawWireSphere(zoneConfig.AreaCenter, zoneConfig.LoadZoneRadius);

            Gizmos.color = UnloadZoneAreaColor;
            Gizmos.DrawWireSphere(zoneConfig.AreaCenter, zoneConfig.UnloadZoneRadius);

            Gizmos.color = oldColor;
        }
    }
}