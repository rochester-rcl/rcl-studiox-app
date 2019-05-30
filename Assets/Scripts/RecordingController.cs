using System;
using System.Linq;
using GetSocialSdk.Capture.Scripts;
using UnityEngine;
using UnityEngine.UI;

public class RecordingController : MonoBehaviour
{
    public GetSocialCapturePreview capturePreview;
    public GetSocialCapture _capture;


    bool isRecording;

	void Awake()
	{
        _capture.captureMode = GetSocialCapture.GetSocialCaptureMode.Manual;
	}

	// Start is called before the first frame update
	void Start()
    {
        
        isRecording = false;
       // _capture.StartCapture();
    }

    public void ShareResult()
    {
        Debug.Log("starting gif generation");
        Action<byte[]> result = bytes =>
        {

        };
        _capture.GenerateCapture(result);
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


    private void StopRecording()
    {
        Debug.Log("recording stopped");

        _capture.StopCapture();
        capturePreview.Play();
    }
}
