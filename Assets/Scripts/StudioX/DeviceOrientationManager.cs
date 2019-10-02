namespace StudioX
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    // using screen orientation because of jank with DeviceOrientation 
    // TODO refactor to ScreenOrientationManager
    /// <summary>Component to handle device (or screen) orientation changes by swapping
    /// swapping between canvases depending on orientation
    /// </summary>
    public class DeviceOrientationManager : MonoBehaviour
    {
        /// <summary>
        /// The <see cref="UnityEngine.GameObject"/> containing the <see cref="UnityEngine.Canvas"/>
        /// with a portrait layout.
        /// </summary>
        public GameObject portraitUI;
        /// <summary>
        /// The <see cref="UnityEngine.GameObject"/> containing the <see cref="UnityEngine.Canvas"/>
        /// with a landscape layout.
        /// </summary>
        public GameObject landscapeUI;
        /// <summary>The current screen orientation.</summary>
        public ScreenOrientation Orientation { get; set; }
        /// <summary>The current native device orientation.</summary>
        private DeviceOrientation NativeOrientation { get; set; }
        /// <summary>Delegate for <see cref="OnOrientationChanged"/>.</summary>
        public delegate void ScreenOrientationChanged(ScreenOrientation orientation);
        /// <summary>
        /// Event that fires when screen orientation change is detected.
        /// Callbacks need signature <see cref="ScreenOrientationChanged" />.
        /// </summary>
        public static event ScreenOrientationChanged OnOrientationChanged;
        /// <summary>Delegate for <see cref="OnDeviceOrientationChanged"/>.</summary>
        public delegate void DeviceOrientationChanged(DeviceOrientation orientation);
        /// <summary>
        /// Event that fires when device orientation change is detected.
        /// Callbacks need signature <see cref="OnDeviceOrientationChanged" />.
        /// </summary>
        public static event DeviceOrientationChanged OnDeviceOrientationChanged;
        /// <summary>Initializes UI layout based on current orientation.</summary>
        void Start()
        {
            SetUI();
        }
        /// <summary>
        /// Detects whether or not <see cref="Orientation" /> is landscape or portrait.
        /// </summary>
        /// <returns>true if <see cref="Orientation"/> is landscape, false if portriat.</returns>
        public bool IsDeviceLandscape()
        {
            return IsDeviceLandscape(Orientation);
        }
        /// <summary>
        /// Checks whether or not a <see cref="UnityEngine.ScreenOrientation"/>  is landscape or portrait.
        /// </summary>
        /// <returns>true if orientation is landscape, false if portriat.</returns>
        public static bool IsDeviceLandscape(ScreenOrientation orientation)
        {
            if (orientation == ScreenOrientation.Landscape) return true;
            if (orientation == ScreenOrientation.LandscapeLeft) return true;
            if (orientation == ScreenOrientation.LandscapeRight) return true;
            return false;
        }
        /// <summary>
        /// Checks whether or not a <see cref="UnityEngine.DeviceOrientation"/>  is landscape or portrait.
        /// </summary>
        /// <returns>true if orientation is landscape, false if portriat.</returns>
        public static bool IsDeviceLandscape(DeviceOrientation orientation)
        {
            if (orientation == DeviceOrientation.LandscapeLeft) return true;
            if (orientation == DeviceOrientation.LandscapeRight) return true;
            return false;
        }
        /// <summary>Gets the current device orientation</summary>
        /// <returns>the current <see cref="UnityEngine.DeviceOrientation"/>.</summary>
        public static DeviceOrientation GetDeviceOrientation()
        {
            return Input.deviceOrientation;
        }
        /// 
        /// <summary>
        /// Checks for the current sceen / device orientation and activates 
        /// either <see cref="portraitUi"/> or <see cref="landscapeUI"/> depending on the orientation.
        /// </summary>
        /// <remarks>
        /// Will fire <see cref="OnOrientationChanged"/> and <see cref="OnDeviceOrientationChanged"/>
        /// when a change in orientation is detected. 
        /// </remarks>
        public void SetUI()
        {
            if (Orientation != Screen.orientation)
            {
                Orientation = Screen.orientation;
                if (IsDeviceLandscape())
                {
                    // Allows us to take into account any external sources that may be hiding / showing
                    if (portraitUI.activeSelf)
                    {
                        portraitUI.SetActive(false);
                        landscapeUI.SetActive(true);
                    }
                }
                else
                {
                    if (landscapeUI.activeSelf)
                    {
                        portraitUI.SetActive(true);
                        landscapeUI.SetActive(false);
                    }
                }
                if (OnOrientationChanged != null)
                {
                    OnOrientationChanged(Orientation);
                }
            }
            if (NativeOrientation != Input.deviceOrientation)
            {
                NativeOrientation = Input.deviceOrientation;
                if (OnDeviceOrientationChanged != null)
                {
                    OnDeviceOrientationChanged(NativeOrientation);
                }
            }
        }
        /// <summary>Checks for device orientation changes on every frame.</summary>
        void FixedUpdate()
        {
            SetUI();
        }
    }
}

