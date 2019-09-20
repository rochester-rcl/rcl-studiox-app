namespace StudioX
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class DeviceOrientationManager : MonoBehaviour
    {
        public GameObject portraitUI;
        public GameObject landscapeUI;
        private ScreenOrientation Orientation { get; set; }
        public delegate void OrientationChanged(ScreenOrientation orientation);
        public static event OrientationChanged OnOrientationChanged;
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
                    OnOrientationChanged(Screen.orientation);
                }
            }

        }

        void Update()
        {
            SetUI();
        }
    }
}

