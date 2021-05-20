using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
//using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using StudioX;

[RequireComponent(typeof(ARRaycastManager))]
public class MeshControlMain : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Instantiates this prefab on a plane at the touch location.")]
    GameObject m_PlacedPrefab;
    // TODO come up with default placed prefab?
    /// <summary>
    /// The prefab to instantiate on touch.
    /// </summary>
/*     public GameObject placedPrefab
    {
        get { return m_PlacedPrefab; }
        set { m_PlacedPrefab = value; }
    } */

    /// <summary>
    /// The object instantiated as a result of a successful raycast intersection with a plane.
    /// </summary>
    public GameObject spawnedObject { get; private set; }
    public bool enableTouchEvents = true;
    private ARSessionOrigin m_SessionOrigin;

    //TODO: Eventually add support for objects without animators.
    private Animator m_anim;

    //rate of changes for pinch zoom.
    private float scale_factor = 0.001f;
    private float position_factor = 0.1f;

    private float min_scale_value = 0.1f;
    private float max_scale_value = 1000.0f;

    private Vector3 prevObjPos = Vector3.zero;
    public bool meshAdded;


    void Awake()
    {
        // pm.planePrefab.
        m_RaycastManager = GetComponent<ARRaycastManager>();
        m_SessionOrigin = GetComponent<ARSessionOrigin>();

    }

    void Update()
    {
        if (meshAdded)
        {
           spawnedObject = GameObject.Find("Your Object");
            if (Input.touchCount == 0)
                return;

            if (Input.touchCount == 2)
            {
                Debug.Log("here");
                Touch touch0 = Input.GetTouch(0);
                Touch touch1 = Input.GetTouch(1);

                Vector2 touch0PrevPos = touch0.position - touch0.deltaPosition;
                Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;

                float prevTouchDeltaMagnitude = (touch0PrevPos - touch1PrevPos).magnitude;
                float touchDeltaMagnitude = (touch0.position - touch1.position).magnitude;

                float deltaMagnitudeDifference = prevTouchDeltaMagnitude - touchDeltaMagnitude;

                if (spawnedObject != null)//make sure the object is visible before allowing the user to change the scale
                {
                    Vector3 prevScale = spawnedObject.transform.localScale;

                    Vector3 vecForLocalScale = prevScale + Vector3.one * (-deltaMagnitudeDifference * scale_factor);

                    //make sure the scale stays positive
                    float fX_Scale = Mathf.Clamp(vecForLocalScale.x, min_scale_value, max_scale_value);
                    //float fY_Scale = Mathf.Clamp(vecForLocalScale.y, min_scale_value, max_scale_value);
                    //float fZ_Scale = Mathf.Clamp(vecForLocalScale.z, min_scale_value, max_scale_value);

                    spawnedObject.transform.localScale = new Vector3(fX_Scale, fX_Scale, fX_Scale); //Should just be uniform, right?

                    //spawnedObject.transform.localScale += Vector3.one * (-deltaMagnitudeDifference * scale_factor);
                    //scale = deltaMagnitudeDifference * scale_factor;
                }

            }
            else
            {
                Touch touch = Input.GetTouch(0);
                // Don't move spawnedObject if the instructions are up
               // if (EventSystem.current.IsPointerOverGameObject(touch.fingerId)) return;
                // if (EventSystem.current.IsPointerOverGameObject(touch.fingerId)) return;
                Vector3 raycastHit = Vector3.one;
                if (m_RaycastManager.Raycast(touch.position, s_Hits, TrackableType.PlaneWithinPolygon))
                {
                    // Raycast hits are sorted by distance, so the first one
                    // will be the closest hit.
                    var hitPose = s_Hits[0].pose;

                    raycastHit = hitPose.position;

                    if (spawnedObject == null)
                    {
                        //spawnedObject = Instantiate(m_PlacedPrefab, raycastHit, hitPose.rotation);
                        prevObjPos = spawnedObject.transform.position;
                    }
                    else
                    {
                        if (enableTouchEvents)
                        {
                            switch (touch.phase)
                            {
                                case TouchPhase.Began:
                                    if (IsDoubleTap(touch))
                                    {
                                        spawnedObject.transform.position = raycastHit;
                                    }
                                    break;

                                case TouchPhase.Moved:
                                    if (Input.touchCount == 1)
                                    {
                                        Vector3 objPosDiff = prevObjPos - raycastHit;
                                        spawnedObject.transform.position += (-objPosDiff * position_factor);
                                    }
                                    break;
                            }
                        }
                    }

                }
            }

            if (spawnedObject != null)
            {
                prevObjPos = spawnedObject.transform.position;
            }
        }



    }


/*     public void UpdatePlacedPrefab(ref GameObject prefab)
    {
        if (placedPrefab == null)
        {
            placedPrefab = prefab;
        }
        else
        {
            if (prefab.name != placedPrefab.name)
            {
                placedPrefab = prefab;
                Destroy(spawnedObject);
            }
        }
    } */

    private bool IsDoubleTap(Touch touch)
    {
#if UNITY_ANDROID
        float deltaTime = touch.deltaTime;
        float deltaMagnitude = touch.deltaPosition.magnitude;
        if (deltaTime > 0 && deltaTime < 0.2f && deltaMagnitude < 1.0f)
        {
            return true;
        }
        return false;
#endif
#if UNITY_IOS
        return touch.tapCount == 2;
#endif
    }

    private void EnableTouchEvents()
    {
        enableTouchEvents = true;
    }

    private void DisableTouchEvents()
    {
        enableTouchEvents = false;
    }

    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    ARRaycastManager m_RaycastManager;
}
