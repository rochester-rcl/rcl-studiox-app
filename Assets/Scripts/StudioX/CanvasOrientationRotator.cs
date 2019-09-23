using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace StudioX
{
    public class CanvasOrientationRotator : MonoBehaviour
    {
        private float rotationDuration = 2.0f;
        private Quaternion rotationPortrait = Quaternion.identity;
        private Quaternion rotationLandscape = Quaternion.Euler(0.0f, 0.0f, -90.0f);
        private VerticalLayoutGroup verticalLayoutGroup;
        private HorizontalLayoutGroup horizontalLayoutGroup;
        private GameObject swappedLayout;
        private int activeGameObjectId;
        internal struct LayoutObjects
        {
            public GameObject LandscapeObject { get; set; }
            public GameObject PortraitObject { get; set; }
            public LayoutObjects(GameObject portrait, GameObject landscape)
            {
                PortraitObject = portrait;
                LandscapeObject = landscape;
            }
        }
        private LayoutObjects layoutObjects;
        public void Start()
        {
            DeviceOrientationManager.OnDeviceOrientationChanged += Rotate;
            if (DeviceOrientationManager.IsDeviceLandscape(DeviceOrientationManager.GetDeviceOrientation()))
            {
                verticalLayoutGroup = gameObject.AddComponent<VerticalLayoutGroup>();
                swappedLayout = new GameObject("TempLayout");
                horizontalLayoutGroup = swappedLayout.AddComponent<HorizontalLayoutGroup>();
                layoutObjects = new LayoutObjects(swappedLayout, gameObject);
                swappedLayout.SetActive(false);
                FormatLayouts();
            }
            else
            {
                horizontalLayoutGroup = gameObject.AddComponent<HorizontalLayoutGroup>();
                swappedLayout = new GameObject("TempLayout");
                verticalLayoutGroup = swappedLayout.AddComponent<VerticalLayoutGroup>();
                layoutObjects = new LayoutObjects(gameObject, swappedLayout);
                swappedLayout.SetActive(false);
                FormatLayouts();
            }
        }

        public void OnDestroy()
        {
            DeviceOrientationManager.OnDeviceOrientationChanged -= Rotate;
        }

        private void Rotate(DeviceOrientation orientation)
        {
            if (DeviceOrientationManager.IsDeviceLandscape(orientation))
            {
                StartCoroutine(ExecRotation(true));
            }
            else
            {
                StartCoroutine(ExecRotation(false));
            }
        }

        private void FormatLayouts()
        {
            horizontalLayoutGroup.childControlWidth = true;
            horizontalLayoutGroup.childControlHeight = true;
            horizontalLayoutGroup.childForceExpandHeight = true;
            horizontalLayoutGroup.childForceExpandWidth = true;
            RectOffset hPadding = new RectOffset();
            hPadding.bottom = 40;
            horizontalLayoutGroup.padding = hPadding;
        }

        private IEnumerator ExecRotation(bool isLandscape)
        {
            float time = 0.0f;
            GameObject currentObj, dstObj;
            if (isLandscape)
            {
                dstObj = layoutObjects.LandscapeObject;
                currentObj = layoutObjects.PortraitObject;
            }
            else
            {
                dstObj = layoutObjects.PortraitObject;
                currentObj = layoutObjects.LandscapeObject;
            }
            foreach (Transform t in currentObj.transform)
            {
                t.transform.parent = dstObj.transform;
            }
            yield return null;
            currentObj.SetActive(false);
            dstObj.SetActive(true);
            while (time < rotationDuration)
            {
                time += Time.deltaTime;
                foreach (Transform t in dstObj.transform)
                {
                    Quaternion dst = isLandscape ? rotationLandscape : rotationPortrait;
                    t.rotation = Quaternion.Slerp(t.rotation, dst, time / rotationDuration);
                }
                yield return null;
            }
        }
    }
}

