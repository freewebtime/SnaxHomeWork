using System;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Assets.Scripts
{
    /// <summary>
    /// Represents the state of a zone, including its configuration, loading status, and activity state.
    /// </summary>
    /// <remarks>This class provides information about the current state of a zone, such as whether it is
    /// loaded or active, its target state, and its associated configuration. It also tracks the loading status of the
    /// zone and contains a reference to the scene instance associated with the zone.</remarks>
    [Serializable]
    public class ZoneState
    {
        /// <summary>
        /// Configuration data for the zone.
        /// </summary>
        public ZoneConfiguration Configuration;

        /// <summary>
        /// If true, the zone scene is loaded (may not be active).
        /// </summary>
        public bool IsLoaded;

        /// <summary>
        /// If true, the zone scene is active.
        /// </summary>
        public bool IsActive;

        /// <summary>
        /// When true, the zone scene should be loaded.
        /// Scene may be in the process of loading or unloading and by the time the current loading/unloading is done,
        /// this value may have changed. 
        /// For instance, if a cursor moved quickly through multiple zones, some zones may not be loaded by the time they are not needed anymore.
        /// </summary>
        public bool TargetIsLoaded;

        /// <summary>
        /// When true, the zone scene should be active.
        /// </summary>
        public bool TargetIsActive;

        /// <summary>
        /// Current loading status of the zone.
        /// </summary>
        public ZoneLoadingStatus LoadingStatus;

        /// <summary>
        /// Acessor to the SceneInstance of the loaded zone scene (if loaded).
        /// </summary>
        public SceneInstance SceneInstance;

        public ZoneState()
        {
            IsLoaded = false;
            LoadingStatus = ZoneLoadingStatus.Unloaded;
        }

        public ZoneState(ZoneConfiguration configuration) : this()
        {
            Configuration = configuration;
        }

        public enum ZoneLoadingStatus
        {
            Unloaded = 0,
            Loading = 1,
            Loaded = 2,
            Unloading = 3
        }
    }
}