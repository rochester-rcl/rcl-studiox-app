using System;
using System.Linq;
using GetSocialSdk.Capture.Scripts;
using UnityEngine;
using UnityEngine.UI;

public class RecordingController : MonoBehaviour
{
    [SerializeField]
    private GetSocialCapturePreview capturePreview;
    [SerializeField]
    private GetSocialCapture _capture;

    //Buttons
    [SerializeField]
    private GameObject recordButton;
    [SerializeField]
    private GameObject recordButtonBG;
    [SerializeField]
    private GameObject cancelButton;
    [SerializeField]
    private GameObject SaveButton;

    private bool isRecording;
    private bool off = false;
    private bool on = true;

	void Awake()
	{
        _capture.captureMode = GetSocialCapture.GetSocialCaptureMode.Continuous;
	}

	// Start is called before the first frame update
	void Start()
    {

        enableReadyToRecordState(true);

        isRecording = false;
       // _capture.StartCapture();
    }

    public void TakeStillImage()
    {
        Debug.Log("Take still image");

        enableReadyToRecordState(false);

    }

    public void SaveResult()
    {
        Debug.Log("starting gif generation");
        //Action<byte[]> result = bytes =>
        //{

        //};
        //_capture.GenerateCapture(result);
    }

    public void CancelResult()
    {
        capturePreview.Stop();

        enableReadyToRecordState(true);
    }

    public void PingRecording()
    {
        if (!isRecording)
        {
            Debug.Log("recording started");
            _capture.StartCapture();
            isRecording = true;
        } else{
            StopRecording();
            isRecording = false;
        }
    }

    //public void ping_test()
    //{
    //    Debug.Log("recording controller ping");
    //}


    private void StopRecording()
    {
        Debug.Log("recording stopped");

        _capture.StopCapture();
        capturePreview.Play();

        enableReadyToRecordState(false);

    }

    private void toggleButton (bool _state, ref GameObject _button)
    {   
        _button.GetComponent<Image>().enabled = _state;
        _button.GetComponent<Button>().interactable = _state;
    }

    private void enableReadyToRecordState(bool _defaultState)
    {
        recordButton.GetComponent<LongPressButton>().enabled = _defaultState;//need seperate call since LongPressButton != Button
        recordButton.GetComponent<Image>().enabled = _defaultState;

        recordButtonBG.GetComponent<Image>().enabled = _defaultState;

        toggleButton(!_defaultState, ref SaveButton);
        toggleButton(!_defaultState, ref cancelButton);
    }


    public bool isControllerRecording() { return isRecording; }

}
