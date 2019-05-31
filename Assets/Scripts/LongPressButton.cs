using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LongPressButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{

    private bool pointerDown = false;
    private float pointerDownTimer = 0f;

    private float maxHoldTime = 3f;

    private float minHoldTime = 0.1f;//minimum time before the press is counted as a long press

    //public UnityEvent onLongPress;
    //public UnityEvent onShortPress;

    [SerializeField]
    private Image fillImage;

    [SerializeField]
    private RecordingController recordingController;
       
    //Functions
    void Start()
    {
        fillImage.fillAmount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (pointerDown)
        {
            if (pointerDownTimer > minHoldTime && !recordingController.isControllerRecording())//if past the threshold for gif and if controller isnt already recording.
            {
                recordingController.PingRecording();//start recording.
            }

            pointerDownTimer += Time.deltaTime;

            if (pointerDownTimer >= maxHoldTime)
            {
                //if (onLongPress != null){ onLongPress.Invoke(); }//reached max gif length so automatically end capture.
                if (recordingController.isControllerRecording())
                {
                    recordingController.PingRecording();//reached max gif length so automatically end capture.
                } else {
                    Debug.Log("--------- out of sync in update --------- ");//I don't think we should reach maxHoldTime with the recording controller not recording.
                }
                Reset();
            }

            if (pointerDownTimer > minHoldTime) { fillImage.fillAmount = pointerDownTimer / maxHoldTime; }
        }

    }

    private void Reset()
    {
        pointerDown = false;
        pointerDownTimer = 0;
        fillImage.fillAmount = 0;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pointerDown = true;
        Debug.Log("pointer down");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (pointerDownTimer <= minHoldTime)
        {
            //onShortPress.Invoke();
            recordingController.TakeStillImage();//take photo

        }
        else
        {
            //clean up gif capture
            Debug.Log("Create gif");
            recordingController.PingRecording();//stop recording.
            if (recordingController.isControllerRecording())//shouldnt be recording.
            {
                Debug.Log("--------- out of sync ---------");
            }
        }

        Reset();
    }
}

