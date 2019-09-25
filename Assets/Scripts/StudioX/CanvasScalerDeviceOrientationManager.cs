using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace StudioX
{
    public class CanvasScalerDeviceOrientationManager : MonoBehaviour
    {
        private CanvasScaler scaler;

        public void Start()
        {
            scaler = gameObject.GetComponent<CanvasScaler>();
            if (!scaler)
            {
                throw new MissingComponentException("GameObject must have a CanvasScaler Component attached!");
            }
            else
            {
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
        }

        public void OnDestroy()
        {
            DeviceOrientationManager.OnOrientationChanged -= UpdateCanvasScaler;
        }

        public void UpdateCanvasScaler(ScreenOrientation orientation)
        {
            StartCoroutine(DoCanvasScalerUpdate(orientation));
        }

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
