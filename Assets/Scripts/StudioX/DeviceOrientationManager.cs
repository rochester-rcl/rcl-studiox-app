namespace StudioX
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    // using screen orientation because of jank with DeviceOrientation 
    // TODO refactor to ScreenOrientationManager
    public class DeviceOrientationManager : MonoBehaviour
    {
        public GameObject portraitUI;
        public GameObject landscapeUI;
        public ScreenOrientation Orientation { get; set; }
        private DeviceOrientation NativeOrientation { get; set; }
        // TODO change to ScreenOrientationChanged and DeviceOrientationChanged
        public delegate void ScreenOrientationChanged(ScreenOrientation orientation);
        public static event ScreenOrientationChanged OnOrientationChanged;
        public delegate void DeviceOrientationChanged(DeviceOrientation orientation);
        public static event DeviceOrientationChanged OnDeviceOrientationChanged;
        void Start()
        {
            SetUI();
        }

        public bool IsDeviceLandscape()
        {
            return IsDeviceLandscape(Orientation);
        }

        public static bool IsDeviceLandscape(ScreenOrientation orientation)
        {
            if (orientation == ScreenOrientation.Landscape) return true;
            if (orientation == ScreenOrientation.LandscapeLeft) return true;
            if (orientation == ScreenOrientation.LandscapeRight) return true;
            return false;
        }

        public static bool IsDeviceLandscape(DeviceOrientation orientation)
        {
            if (orientation == DeviceOrientation.LandscapeLeft) return true;
            if (orientation == DeviceOrientation.LandscapeRight) return true;
            return false;
        }

        public static DeviceOrientation GetDeviceOrientation()
        {
            return Input.deviceOrientation;
        }

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

        void FixedUpdate()
        {
            SetUI();
        }
    }
}

