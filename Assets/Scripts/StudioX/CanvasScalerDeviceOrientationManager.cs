using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace StudioX
{
    /// <summary>Updates a <see cref="UnityEngine.UI.CanvasScaler"/> to scale according to width or height
    /// depending on screen orientation.
    /// </summary>
    [RequireComponent(typeof(CanvasScaler))]
    public class CanvasScalerDeviceOrientationManager : MonoBehaviour
    {
        /// <summary>
        /// The <see cref="UnityEngine.UI.CanvasScaler"/> attached to <see cref="CanvasScalerDeviceOrientationManager.gameObject"/>.
        /// </summary>
        private CanvasScaler scaler;
        /// <summary>Initializes <see cref="scaler"/> and adds <see cref="UpdateCanvasScaler"/>
        /// as a handler for <see cref="DeviceOrientationManager.OnOrientationChanged" />.
        /// </summary>
        public void Start()
        {
            scaler = gameObject.GetComponent<CanvasScaler>();
            DeviceOrientationManager.OnOrientationChanged += UpdateCanvasScaler;
            if (DeviceOrientationManager.IsDeviceLandscape(Screen.orientation))
            {
                scaler.matchWidthOrHeight = 1;
            }
            else
            {
                scaler.matchWidthOrHeight = 0;
            }
        }
        /// <summary>
        /// Removes <see cref="UpdateCanvasScaler"/> handler from 
        /// <see cref="DeviceOrientationManager.OnOrientationChanged"/>
        /// </summary>
        public void OnDestroy()
        {
            DeviceOrientationManager.OnOrientationChanged -= UpdateCanvasScaler;
        }
        /// <summary>
        /// Updates <see cref="scaler"/> to reflect current <see cref="UnityEngine.ScreenOrientation"/>.
        /// </summary>
        /// <param name="orientation">The current screen orientation.</param>
        public void UpdateCanvasScaler(ScreenOrientation orientation)
        {
            StartCoroutine(DoCanvasScalerUpdate(orientation));
        }
        /// <summary>
        /// Sets <see cref="scaler"/> matchWidthOrHeight property to reflect current orientation.
        /// </summary>
        /// <param name="orientation">The current screen orientation.</param> 
        private IEnumerator DoCanvasScalerUpdate(ScreenOrientation orientation)
        {
            if (DeviceOrientationManager.IsDeviceLandscape(orientation))
            {
                yield return null;
                scaler.matchWidthOrHeight = 1;
            }
            else
            {
                yield return null;
                scaler.matchWidthOrHeight = 0;
            }
        }
    }
}
