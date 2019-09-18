
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
#if UNITY_IOS
using UnityEngine.iOS;
using UnityEngine.Apple.ReplayKit;
#endif
#if UNITY_ANDROID
using NatCorder.Clocks;
using NatCorder.Inputs;
#endif

//manages recording and ui for both ios/android versions of the AR studio x app.

public class RecordingManager : MonoBehaviour
{
    public GameObject recordButton;
    private Button discardButton;
    private Button previewButton;

    [SerializeField]
    Camera mainARCamera;

#if UNITY_ANDROID

    private float frameDuration = 0.1f; // seconds

    private NatCorder.GIFRecorder gifRecorder;
    private CameraInput cameraInput;
#endif

    private void Start()
    {
        discardButton = GameObject.FindGameObjectWithTag("DiscardButton").GetComponent<Button>();
        previewButton = GameObject.FindGameObjectWithTag("PreviewButton").GetComponent<Button>();

        discardButton.gameObject.SetActive(false);
        previewButton.gameObject.SetActive(false);
    }

    public void StartRecording()
    {
        //toggle AR camera's culling flags so the plane/point cloud won't show up in the recording video/gif.
        mainARCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("TransparentFX"));
#if UNITY_IOS
        ReplayKit.StartRecording();
#endif

#if UNITY_ANDROID
        // Start recording
        gifRecorder = new NatCorder.GIFRecorder(Screen.width/3, Screen.height/3, frameDuration, OnGIF);
        // Create a camera input
        cameraInput = new CameraInput(gifRecorder, new RealtimeClock(), Camera.main);
        // Get a real GIF look by skipping frames
        cameraInput.frameSkip = 4;
#endif
    }

    public void StopRecording()
    {
        mainARCamera.cullingMask |= 1 << LayerMask.NameToLayer("TransparentFX");
#if UNITY_IOS
        ReplayKit.StopRecording();
        toggleRecordingUI(false);
#endif

#if UNITY_ANDROID
        cameraInput.Dispose();
        gifRecorder.Dispose();
#endif

    }
#if UNITY_IOS
    public void DiscardRecording()
    {
        ReplayKit.Discard();
        toggleRecordingUI(true);
    }

    public void PreviewRecording()
    {
        ReplayKit.Preview();
        toggleRecordingUI(true);
    }
#endif

#if UNITY_ANDROID
    private void OnGIF(string path)
    {
        Debug.Log("Saved recording to: " + path);
        // Playback the video
        Application.OpenURL(path);
    }
#endif

    public void TakePhoto()
    {
        mainARCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("TransparentFX"));
        int resWidth = Screen.width;
        int resHeight = Screen.height;

        Debug.Log("Take still image with res (" + resWidth + ", " + resHeight + ")");

        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        mainARCamera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
        mainARCamera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        mainARCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        //_preview.texture = screenShot;
        byte[] stillImageResult = screenShot.EncodeToPNG();
        long date_long = System.DateTime.UtcNow.Ticks;
        
        string filename = "AR_Photo-" + date_long.ToString() + ".png";
        NativeGallery.SaveImageToGallery(stillImageResult, "album", filename, null);

        mainARCamera.cullingMask |= 1 << LayerMask.NameToLayer("TransparentFX");
    }
    //Don't need to have discard or preview buttons for Android since natcorder automatically opens the gif which leads to
    //the Android save/discard dialogue.
#if UNITY_IOS
    private void toggleRecordingUI(bool toggle)
    {
        recordButton.gameObject.SetActive(toggle);

        previewButton.gameObject.SetActive(!toggle);
        discardButton.gameObject.SetActive(!toggle);
    }
#endif
    }
