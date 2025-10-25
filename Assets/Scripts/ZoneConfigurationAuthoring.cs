using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// Authoring component for ZoneConfiguration.
    /// Allows to place a game object in the scene to define zone center position.
    /// Provides ZoneConfiguration data for ZoneStreamer.
    /// </summary>
    public class ZoneConfigurationAuthoring : MonoBehaviour
    {
        /// <summary>
        /// Zone configuration data. 
        /// Expected that there may be different sources of configuration in the future,
        /// so configuration is separated from authoring component.
        /// </summary>
        public ZoneConfiguration ZoneConfig;

        /// <summary>
        /// Acessor for ZoneConfiguration data.
        /// </summary>
        public ZoneConfiguration ZoneConfiguration => GetZoneConfiguration();

        /// <summary>
        /// When something is changed, we recalculate zone center position.
        /// </summary>
        private void OnValidate()
        {
            ZoneConfig ??= new ZoneConfiguration();
            ZoneConfig.AreaCenter = transform.position;
        }

        /// <summary>
        /// Gives a ZoneConfiguration instance with updated AreaCenter based on the game object position.
        /// </summary>
        public ZoneConfiguration GetZoneConfiguration()
        {
            var config = ZoneConfig;
            if (config == null)
            {
                return default;
            }

            config.AreaCenter = transform.position;
            return config;
        }
    }
}