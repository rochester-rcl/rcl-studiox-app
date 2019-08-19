namespace StudioX
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class DeviceOrientationManager : MonoBehaviour
    {
        [SerializeField]
        public GameObject portraitUI;
        [SerializeField]
        public GameObject landscapeUI;
        private ScreenOrientation Orientation { get; set; }
        void Start()
        {
            SetUI();
        }

        bool IsDeviceLandscape()
        {
            if (Orientation == ScreenOrientation.Landscape) return true;
            if (Orientation == ScreenOrientation.LandscapeLeft) return true;
            if (Orientation == ScreenOrientation.LandscapeRight) return true;
            return false;
        }

        void SetUI()
        {
            if (Orientation != Screen.orientation)
            {
                Orientation = Screen.orientation;
                if (IsDeviceLandscape())
                {
                    portraitUI.SetActive(false);
                    landscapeUI.SetActive(true);
                }
                else
                {
                    portraitUI.SetActive(true);
                    landscapeUI.SetActive(false);
                }
            }

        }

        void Update()
        {
            SetUI();
        }
    }
}

