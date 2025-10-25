using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// Script that is responsible for showing/hiding loading indicator UI element.
    /// ZoneStreamer uses this to indicate loading state.
    /// </summary>
    public class LoadingIndicatorController : MonoBehaviour
    {
        /// <summary>
        /// Reference to the game object that represents loading indicator UI element.
        /// This object will be enabled/disabled based on loading state.
        /// </summary>
        public GameObject LoadingIndicator;

        [Tooltip("Indicates whether loading is in progress.")]
        [SerializeField]
        private bool _isLoading;

        /// <summary>
        /// Gets or sets a value indicating whether a loading operation is in progress.
        /// </summary>
        /// <remarks>Setting this property triggers the <c>IsLoadingChanged</c> method if the value
        /// changes.</remarks>
        public bool IsLoading 
        { 
            get => _isLoading; 
            set
            {
                if (_isLoading == value)
                {
                    return;
                }

                _isLoading = value;
                IsLoadingChanged();
            } 
        }

        private void Start()
        {
            IsLoadingChanged();
        }

        /// <summary>
        /// Called when IsLoading property changes.
        /// Enables/disables the loading indicator game object based on the current loading state.
        /// </summary>
        public void IsLoadingChanged()
        {
            if (LoadingIndicator != null)
            {
                LoadingIndicator.SetActive(_isLoading);
            }
        }
    }
}