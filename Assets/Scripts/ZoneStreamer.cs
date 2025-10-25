using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
    /// <summary>
    /// Manages the dynamic loading and unloading of zones based on the position of a reference point, such as a player
    /// character, to optimize resource usage and performance in a scene.
    /// </summary>
    /// <remarks>The <see cref="ZoneStreamer"/> class is responsible for determining which zones should be
    /// loaded or unloaded based on their proximity to a specified position. It supports simulating slow network
    /// connections for testing purposes and provides visual feedback through a loading indicator.  Zones are defined in
    /// the <see cref="ZoneConfigurations"/> collection, and their states are tracked in the <see cref="ZoneStates"/>
    /// collection. The class ensures that zones are loaded or unloaded as needed, based on their configuration and the
    /// distance from the reference point.  
    /// </remarks>
    public class ZoneStreamer : MonoBehaviour
    {
        /// <summary>
        /// Represents a collection of zone configurations used for authoring purposes.
        /// </summary>
        /// <remarks>Each <see cref="ZoneConfigurationAuthoring"/> in the collection defines the
        /// configuration details  for a specific zone. 
        /// </remarks>
        [Tooltip("List of zone configurations for authoring")]
        public List<ZoneConfigurationAuthoring> ZoneConfigurations;

        /// <summary>
        /// Controls the visibility and behavior of a loading indicator in the application.
        /// </summary>
        /// <remarks>Use this property to access or modify the loading indicator's state.</remarks>
        [Tooltip("Reference to loading indicator controller")]
        public LoadingIndicatorController LoadingIndicator;

        /// <summary>
        /// Tracks the number of active requests to show a loading indicator.
        /// </summary>
        /// <remarks>This field is used internally to manage the state of the loading indicator.  A
        /// positive value indicates that there are active requests to display the loading indicator,  while a value of
        /// zero means no such requests are active.</remarks>
        private int _showLoadingIndicatorRequestsCount;

        [Tooltip("State of each zone (loaded/unloaded/loading/unloading). Index in this collection = index in ZoneConfigurations collection")]
        public List<ZoneState> ZoneStates;

        [Tooltip("If true, waits for SimulateSlowConnectionsDelay seconds before actually load a scene")]
        public bool IsSimulateSlowConnection;

        [Tooltip("If IsSimulateSlowConnection is true, this amount of seconds will be awaited before actually loading a scene")]
        public float SimulateSlowConnectionsDelay;

        /// <summary>
        /// Calculates which zones must be loaded and which must be unloaded based on given position of the cursor (the point we calculate distances to zones from)
        /// </summary>
        /// <param name="cursorPosition">3d position of a cursor (player pawn)</param>
        public void StreamZones(Vector3 cursorPosition, float cursorRadius)
        {
            // prepare data
            ZoneStates ??= new List<ZoneState>();

            // validate zone configurations
            if (ZoneConfigurations == null || ZoneConfigurations.Count <= 0)
            {
                // we have no zones at all, so nothing to load and unload
                return;
            }

            // calculate which zones to load and which to unload
            for (var zoneIndex = 0; zoneIndex < ZoneConfigurations.Count; zoneIndex++)
            {
                // get zone configuration
                var zoneConfigurationObject = ZoneConfigurations[zoneIndex];
                if (zoneConfigurationObject == null)
                {
                    continue;
                }

                var zoneConfig = zoneConfigurationObject.GetZoneConfiguration();
                if (zoneConfig == null)
                {
                    continue;
                }

                // ensure that there is a ZoneState slot for this zone
                while (ZoneStates.Count <= zoneIndex)
                {
                    ZoneStates.Add(default);
                }

                // get zone status and ensure that it's not null
                var zoneState = ZoneStates[zoneIndex];
                if (zoneState == null)
                {
                    zoneState = CreateZoneState(zoneConfig);
                    ZoneStates[zoneIndex] = zoneState;
                }

                // now we have a zone state object instance we use for tracking zone,
                // it's stored in ZoneStates collection with index = zone configuration index
                // and it's not null

                // now it's time to check if zone has to be loaded or unloaded according to the distance from pawnPosition
                var distance = Vector3.Distance(cursorPosition, zoneConfig.AreaCenter);
                distance -= cursorRadius;

                if (distance <= zoneConfig.LoadZoneRadius)
                {
                    LoadZone(zoneState);
                }
                else if (distance >= zoneConfig.UnloadZoneRadius)
                {
                    UnloadZone(zoneState);
                }
            }
        }

        /// <summary>
        /// Initiates the loading process for the specified zone based on its current state.
        /// </summary>
        /// <remarks>If the zone's <see cref="ZoneState.LoadingStatus"/> is <see
        /// cref="ZoneState.ZoneLoadingStatus.Unloaded"/>, the method starts the loading process. For other statuses,
        /// the method takes no action and logs the current state.</remarks>
        /// <param name="zoneState">The state of the zone to be loaded. .</param>
        public void LoadZone(ZoneState zoneState)
        {
            if (zoneState == null)
            {
                return;
            }

            zoneState.TargetIsActive = true;
            zoneState.TargetIsLoaded = true;

            switch (zoneState.LoadingStatus)
            {
                case ZoneState.ZoneLoadingStatus.Loaded:
                case ZoneState.ZoneLoadingStatus.Loading:
                case ZoneState.ZoneLoadingStatus.Unloading:
                    // do nothing, it's already done or on it's way to
                    ReportStep($"{zoneState.Configuration.Name} Skipping loading, because the state is {zoneState.LoadingStatus}");
                    break;

                case ZoneState.ZoneLoadingStatus.Unloaded:
                    ReportStep($"{zoneState.Configuration.Name} Starting Loading Coroutine");
                    StartCoroutine(LoadZoneCoroutine(zoneState));
                    break;
            }
        }

        /// <summary>
        /// Unloads the specified zone, transitioning it to an inactive and unloaded state.
        /// </summary>
        /// <remarks>If the zone is already unloaded, unloading, or loading, the method will take no
        /// action. Otherwise, it initiates the unloading process for zones in the <see
        /// cref="ZoneState.ZoneLoadingStatus.Loaded"/> state.</remarks>
        /// <param name="zoneState">The state of the zone to unload.</param>
        public void UnloadZone(ZoneState zoneState)
        {
            if (zoneState == null || zoneState.LoadingStatus == ZoneState.ZoneLoadingStatus.Unloaded)
            {
                return;
            }

            zoneState.TargetIsActive = false;
            zoneState.TargetIsLoaded = false;

            switch (zoneState.LoadingStatus)
            {
                case ZoneState.ZoneLoadingStatus.Unloaded:
                case ZoneState.ZoneLoadingStatus.Unloading:
                case ZoneState.ZoneLoadingStatus.Loading:
                    // do nothing, it's already done or on it's way to
                    ReportStep($"{zoneState.Configuration.Name} Skipping unloading, because the state is {zoneState.LoadingStatus}");
                    break;

                case ZoneState.ZoneLoadingStatus.Loaded:
                    ReportStep($"{zoneState.Configuration.Name} Starting Scene Unloading");
                    StartCoroutine(UnloadZoneCoroutine(zoneState));
                    break;
            }
        }

        /// <summary>
        /// Loads a zone asynchronously based on the provided <see cref="ZoneState"/> configuration.
        /// </summary>
        /// <remarks>This coroutine handles the asynchronous loading of a zone's scene using the
        /// Addressables system. It updates the zone's loading status, manages loading indicators, and activates the
        /// scene upon successful loading. If the loading fails, the process is aborted, and an error is reported.
        /// Additionally, if the zone is loaded but not required, it is immediately unloaded.  The method simulates slow
        /// connections if the <c>IsSimulateSlowConnection</c> flag is enabled, introducing a delay before completing
        /// the loading process.</remarks>
        /// <param name="zoneState">The state of the zone to be loaded.</param>
        /// <returns>An enumerator that can be used to control the coroutine's execution flow.</returns>
        private IEnumerator LoadZoneCoroutine(ZoneState zoneState)
        {
            if (zoneState == null || zoneState.Configuration == null)
            {
                yield break;
            }

            ShowLoadingIndicator();

            ReportStep($"{zoneState.Configuration.Name} Starting Loading...");
            zoneState.LoadingStatus = ZoneState.ZoneLoadingStatus.Loading;

            var loadSceneHandle = Addressables.LoadSceneAsync(
                zoneState.Configuration.SceneAddress, 
                LoadSceneMode.Additive, 
                activateOnLoad: false
            );
            yield return loadSceneHandle;

            if (IsSimulateSlowConnection)
            {
                yield return new WaitForSeconds(SimulateSlowConnectionsDelay);
            }

            if (!loadSceneHandle.IsDone || loadSceneHandle.Status != AsyncOperationStatus.Succeeded)
            {
                ReportError($"Something is wrong with loading zone {zoneState.Configuration.Name}");

                HideLoadingIndicator();
                // here we can retry or accept failure
                yield break;
            }

            zoneState.SceneInstance = loadSceneHandle.Result;
            zoneState.IsLoaded = true;

            yield return zoneState.SceneInstance.ActivateAsync();

            zoneState.IsActive = true;
            zoneState.LoadingStatus = ZoneState.ZoneLoadingStatus.Loaded;

            HideLoadingIndicator();

            if (!zoneState.TargetIsLoaded)
            {
                ReportStep($"{zoneState.Configuration.Name} Is loaded, but not needed");

                UnloadZone(zoneState);
            }
        }

        /// <summary>
        /// Unloads the specified zone asynchronously, releasing its associated resources and updating its state.
        /// </summary>
        /// <remarks>This coroutine handles the unloading of a zone, including releasing its scene
        /// resources and updating its loading status. If the zone's scene is valid, it is unloaded using the
        /// Addressables system. Once the unloading is complete, the zone's state is updated to reflect that it is no
        /// longer loaded. If the zone is marked as a target for loading, the method initiates the loading process for
        /// the zone again.</remarks>
        /// <param name="zoneState">The <see cref="ZoneState"/> object representing the zone to unload. This parameter must not be <see
        /// langword="null"/> and must have a valid <see cref="ZoneState.Configuration"/>.</param>
        /// <returns></returns>
        private IEnumerator UnloadZoneCoroutine(ZoneState zoneState)
        {
            if (zoneState == null || zoneState.Configuration == null)
            {
                yield break;
            }

            ReportStep($"{zoneState.Configuration.Name} Starting Unloading...");
            zoneState.LoadingStatus = ZoneState.ZoneLoadingStatus.Unloading;

            if (zoneState.SceneInstance.Scene.IsValid())
            {
                var unloadSceneHandle = Addressables.UnloadSceneAsync(
                    zoneState.SceneInstance,
                    UnloadSceneOptions.UnloadAllEmbeddedSceneObjects,
                    autoReleaseHandle: false
                );
                yield return unloadSceneHandle;
            }

            zoneState.IsLoaded = false;
            zoneState.SceneInstance = default;
            zoneState.LoadingStatus = ZoneState.ZoneLoadingStatus.Unloaded;

            ReportStep($"{zoneState.Configuration.Name} is Unloaded.");

            if (zoneState.TargetIsLoaded)
            {
                LoadZone(zoneState);
            } 
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ZoneState"/> class using the specified zone configuration.
        /// </summary>
        /// <param name="zoneConfiguration">The configuration settings for the zone. This parameter cannot be null.</param>
        /// <returns>A new <see cref="ZoneState"/> instance initialized with the provided zone configuration.</returns>
        private ZoneState CreateZoneState(ZoneConfiguration zoneConfiguration)
        {
            var zoneState = new ZoneState(zoneConfiguration);
            return zoneState;
        }

        /// <summary>
        /// Displays the loading indicator by incrementing the request count and setting the loading state.
        /// </summary>
        /// <remarks>This method ensures that the loading indicator is shown by incrementing an internal
        /// counter and setting the <see cref="LoadingIndicator.IsLoading"/> property to <see langword="true"/>
        /// if the <see cref="LoadingIndicator"/> is not null.</remarks>
        private void ShowLoadingIndicator()
        {
            _showLoadingIndicatorRequestsCount++;

            if (LoadingIndicator != null)
            {
                LoadingIndicator.IsLoading = true;
            }
        }

        /// <summary>
        /// Hides the loading indicator if no active requests remain.
        /// </summary>
        /// <remarks>This method decrements the internal request count and ensures it does not drop below
        /// zero.  If the request count reaches zero and a loading indicator is available, the loading state is
        /// disabled.</remarks>
        private void HideLoadingIndicator()
        {
            _showLoadingIndicatorRequestsCount--;
            _showLoadingIndicatorRequestsCount = math.max(_showLoadingIndicatorRequestsCount, 0);

            if (LoadingIndicator != null && _showLoadingIndicatorRequestsCount <= 0)
            {
                LoadingIndicator.IsLoading = false;
            }
        }

        /// <summary>
        /// Logs a message to the debug console.
        /// </summary>
        /// <remarks>This method is intended for reporting the progress or status of a specific step
        /// during execution. The message will be output to the debug console for diagnostic purposes.</remarks>
        /// <param name="message">The message to be logged. Cannot be null or empty.</param>
        private void ReportStep(string message)
        {
            Debug.Log(message);
        }

        /// <summary>
        /// Logs the specified error message to the debug console.
        /// </summary>
        /// <remarks>This method writes the error message to the debug console using <see
        /// cref="Debug.LogError(string)"/>. Ensure that the provided <paramref name="error"/> parameter contains
        /// meaningful information for debugging purposes.</remarks>
        /// <param name="error">The error message to log. Cannot be null or empty.</param>
        private void ReportError(string error)
        {
            Debug.LogError(error);
        }
    }
}