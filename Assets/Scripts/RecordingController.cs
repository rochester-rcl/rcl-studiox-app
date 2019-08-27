using System;
using System.Collections.Generic;
using System.Linq;
using GetSocialSdk.Capture.Scripts;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class RecordingController : MonoBehaviour
{

    [SerializeField]
    private GetSocialCapturePreview capturePreview;
    [SerializeField]
    private GetSocialCapture _capture;

    [SerializeField]
    private Camera _camera;
    [SerializeField]
    private RawImage _preview;

    //Buttons
    [SerializeField]
    private GameObject recordButton;
    [SerializeField]
    private GameObject recordButtonBG;
    [SerializeField]
    private GameObject cancelButton;
    [SerializeField]
    private GameObject SaveButton;
    [SerializeField]
    private GameObject recordButtonFG;

    private bool isRecording;
    private bool off = false;
    private bool on = true;

    bool tookPhoto = false;
    bool tookVideo = false;

    private byte[] stillImageResult;


    bool inAlbum = false;


    void Awake()
	{
        _capture.captureMode = GetSocialCapture.GetSocialCaptureMode.Continuous;
	}

	// Start is called before the first frame update
	void Start()
    {

        _preview.color = Color.clear;
        _preview.texture = null;
    
        enableReadyToRecordState(true);

        isRecording = false;
       // _capture.StartCapture();
    }

    void Update()
    {
		//if (screenRecorder.isVideoSaved() && !inAlbum)
		//{
		//	// byte[] rawFrames = screenRecorder.getRawFrames();
		//	byte[] rf = File.ReadAllBytes(screenRecorder.getPath());
		//	NativeGallery.SaveVideoToGallery(rf, "album", "rawtestVideo.mov", null);

		//	//NativeGallery.SaveVideoToGallery(screenRecorder.getPath(), "album2", "testVideo.bmp", null);
		//	Debug.Log("saved video to gallery");
		//	inAlbum = true;
		//}
	}

    public void TakeStillImage()
    {
        int resWidth = Screen.width;
        int resHeight = Screen.height;

        Debug.Log("Take still image with res (" + resWidth + ", " + resHeight + ")");

        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        _camera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24,false);
        _camera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        _camera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        _preview.texture = screenShot;
        stillImageResult = screenShot.EncodeToPNG();

        Debug.Log("done taking photo");

        enableReadyToRecordState(false);
        tookPhoto = true;
    }

    public void SaveResult()
    {

        if (tookPhoto && stillImageResult != null)
        {
            string filename = "test.png";
            NativeGallery.SaveImageToGallery(stillImageResult, "album", filename, null);

            Debug.Log("saved photo");

            tookPhoto = false;

            //stillImageResult = null;

        } else if (tookVideo)
        {
		
            //List<Texture2D> gifFrames = capturePreview.getFramesToPlay();
            //         List<byte> unrolled_frames_list = new List<byte>();
            //         for (int i = 0; i < gifFrames.Count; i++)
            //         {
            //             byte[] rgf = gifFrames[i].GetRawTextureData();

            //             for (int j =0; j < rgf.Length; j++)
            //             {
            //                 unrolled_frames_list.Add(rgf[j]);
            //             }

            //         }
            //         byte[] all_rgf = unrolled_frames_list.ToArray();


            //         for (int i = 0; i < gifFrames.Count; i++)
            //{
            //             byte[] rgf = gifFrames[i].GetRawTextureData();

            //}

            string read_from_path = _capture.getResultFilePath();

            NativeGallery.SaveVideoToGallery(read_from_path, "album", "rawtestVideo1.gif", null);
			Debug.Log("saved video");
			tookVideo = false;
        } else
        {
            Debug.Log("no data to save");
        }

    }

    public void CancelResult()
    {
         capturePreview.Stop();
        Destroy(_preview.texture);

        _preview.texture = null;
        _preview.color = Color.clear;

        enableReadyToRecordState(true);
    }

    public void PingRecording()
    {
        if (!isRecording)
        {
            Debug.Log("recording started");
            _capture.StartCapture();
            isRecording = true;
			tookVideo = true;
        } else{
            StopRecording();
            isRecording = false;
        }
    }

    //public void ping_test()
    //{
    //    Debug.Log("recording controller ping");
    //}

    public void StartRecording()
    {
        //screenRecorder.recordVideo = true;
        isRecording = true;

    }

    private void StopRecording()
    {
        Debug.Log("recording stopped");

        _capture.StopCapture();
        Debug.Log("starting gif generation");
        Action<byte[]> result = bytes =>
        {
            // generated gif returned as byte[]

            //byte[] gifContent = result.ToArray();
        };
        _capture.GenerateCapture(result);
        Debug.Log("generated gif");
        capturePreview.Play();
        Debug.Log("after preview play");
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
        recordButtonFG.GetComponent<Image>().enabled = _defaultState;

        toggleButton(!_defaultState, ref SaveButton);
        toggleButton(!_defaultState, ref cancelButton);
    }


    public bool isControllerRecording() { return isRecording; }

}
