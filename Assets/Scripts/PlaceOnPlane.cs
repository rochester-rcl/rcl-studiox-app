using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
//using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
public class PlaceOnPlane : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Instantiates this prefab on a plane at the touch location.")]
    GameObject m_PlacedPrefab;

    /// <summary>
    /// The prefab to instantiate on touch.
    /// </summary>
    public GameObject placedPrefab
    {
        get { return m_PlacedPrefab; }
        set { m_PlacedPrefab = value; }
    }
    
    /// <summary>
    /// The object instantiated as a result of a successful raycast intersection with a plane.
    /// </summary>
    public GameObject spawnedObject { get; private set; }
    private ARSessionOrigin m_SessionOrigin;

    //TODO: Eventually add support for objects without animators.
    private Animator m_anim;

    //rate of changes for pinch zoom.
    private float scale_factor = 0.001f;
    private float position_factor = 0.1f;

    private float min_scale_value = 0.1f;
    private float max_scale_value = 10f;

    private Vector3 prevObjPos = Vector3.zero;

    private bool in_touch_drag = false;

    void Awake()
    {
        m_RaycastManager = GetComponent<ARRaycastManager>();
        m_SessionOrigin = GetComponent<ARSessionOrigin>();

    }

    void Update()
    {

       // bool isP = recordButton.OnPointerDown

        if (Input.touchCount == 0)
            return;
        
        if (Input.touchCount == 2)
        {                   
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

        Touch touch = Input.GetTouch(0);

        Vector3 raycastHit = Vector3.one;

        if (m_RaycastManager.Raycast(touch.position, s_Hits, TrackableType.PlaneWithinPolygon))
        {
            // Raycast hits are sorted by distance, so the first one
            // will be the closest hit.
            var hitPose = s_Hits[0].pose;

            raycastHit = hitPose.position;

            if (spawnedObject == null)
            {
                spawnedObject = Instantiate(m_PlacedPrefab, raycastHit, hitPose.rotation);
                spawnedObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                prevObjPos = spawnedObject.transform.position;
            }
            else
            {
                switch(touch.phase)
                {
                    case TouchPhase.Began:
                        in_touch_drag = false;
                        break;

                    case TouchPhase.Moved:
                        in_touch_drag = true;

                        if (Input.touchCount == 1)
                        {
                            Vector3 objPosDiff = prevObjPos - raycastHit;
                            spawnedObject.transform.position += (-objPosDiff * position_factor);
                        }

                        break;

                    case TouchPhase.Ended:
                        if (!in_touch_drag)
                        {
                            spawnedObject.transform.position = raycastHit;
                        }
                        in_touch_drag = false;
                        break;
                }
            }
        }

        if (spawnedObject != null)
        {
            prevObjPos = spawnedObject.transform.position;
        }

    }

    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    ARRaycastManager m_RaycastManager;
}
