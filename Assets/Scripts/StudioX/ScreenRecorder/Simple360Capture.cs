using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using System.IO;

namespace StudioX
{
    namespace ScreenRecorder
    {
#if UNITY_EDITOR
        public class Simple360Capture : MonoBehaviour
        {
            private Cubemap tex;
            private RenderTexture rt;
            private RenderTexture equirect;
            public int cubemapSize = 512;
            public int equirectangularWidth = 2048;
            public int targetFramerate = 30;
            public int duration;
            public bool startCaptureOnStart = true;
            public bool exportEquirectangular;
            public string imagePrefix;
            public string imageFolder;
            private Texture2D OutTex { get; set; }
            private CubemapFace[] CubemapFaces = { CubemapFace.PositiveX, CubemapFace.NegativeX,
            CubemapFace.PositiveY, CubemapFace.NegativeY,
            CubemapFace.PositiveZ, CubemapFace.NegativeZ };
            private int framesCaptured;
            private int totalFrames;

            private Camera cam;
            private GameObject progress;
            private Text progressText;
            private bool recording;
            private Rect equirectangularArea;
            void Start()
            {
                cam = Camera.main;
                cam.tag = "Untagged";
                // IGNORE UI LAYER
                cam.cullingMask = ~(1 << 5);
                framesCaptured = 0;
                totalFrames = duration * targetFramerate;
                if (string.IsNullOrWhiteSpace(imageFolder))
                {
                    imageFolder = FindRecordingsDir();
                }
                if (exportEquirectangular)
                {
                    rt = new RenderTexture(cubemapSize, cubemapSize, 24, RenderTextureFormat.ARGB32);
                    rt.dimension = TextureDimension.Cube;
                    rt.anisoLevel = 9;
                    rt.antiAliasing = 8;
                    equirect = new RenderTexture(equirectangularWidth, equirectangularWidth / 2, 24, RenderTextureFormat.ARGB32);
                    equirect.anisoLevel = 9;
                    equirect.antiAliasing = 8;
                    OutTex = new Texture2D(equirectangularWidth, equirectangularWidth / 2, TextureFormat.RGB24, false);
                    equirectangularArea = new Rect(0, 0, equirectangularWidth, equirectangularWidth * 2);
                    cam.stereoSeparation = 0f;
                }
                else
                {
                    tex = new Cubemap(cubemapSize, TextureFormat.RGBA32, false);
                    tex.anisoLevel = 9;
                    OutTex = new Texture2D(tex.width, tex.height * 6, TextureFormat.RGBA32, false);
                }

               
                InitCanvas();
                if (startCaptureOnStart)
                {
                    StartRecording();
                }
            }

            private void InitCanvas()
            {
                progress = new GameObject();
                progress.layer = 5;
                progress.name = "ProgressOverlay";
                Canvas canvas = progress.AddComponent<Canvas>();
                CanvasScaler scaler = progress.AddComponent<CanvasScaler>();
                progressText = progress.AddComponent<Text>();
                progressText.alignment = TextAnchor.LowerCenter;
                progressText.text = string.Format("Recorded {0} Frames / {1}", framesCaptured, totalFrames);
                progressText.resizeTextMinSize = 14;
                progressText.resizeTextMaxSize = 30;
                progressText.resizeTextForBestFit = true;
                progressText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.matchWidthOrHeight = 0;
                scaler.referencePixelsPerUnit = 100;
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                progress.SetActive(false);
            }

            private string FindRecordingsDir()
            {
                string projectDir = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/'));
                string recordingsDir = string.Format("{0}/Recordings", projectDir);
                System.IO.Directory.CreateDirectory(recordingsDir);
                return recordingsDir;
            }

            private void CubemapToBitmapLayout()
            {
                cam.RenderToCubemap(tex, 63);
                tex.SmoothEdges();
                Unity.Collections.NativeArray<Color32> rawData = OutTex.GetRawTextureData<Color32>();
                int index = 0;
                for (int i = CubemapFaces.Length; i > 0; --i)
                {
                    Color[] face = tex.GetPixels(CubemapFaces[i - 1]);
                    for (int y = 0; y < tex.height; y++)
                    {
                        for (int x = 0; x < tex.width; x++)
                        {
                            // invert the input tex since origin is bottom left corner
                            int faceIndex = ((tex.height - 1 - y) * tex.width + x);
                            rawData[index++] = face[faceIndex];
                        }
                    }
                }
                OutTex.Apply();
            }

            private void CubemapToEquirectangular()
            {
                cam.RenderToCubemap(rt, 63, Camera.MonoOrStereoscopicEye.Left);
                rt.ConvertToEquirect(equirect, Camera.MonoOrStereoscopicEye.Mono);
                RenderTexture oldActive = RenderTexture.active;
                RenderTexture.active = equirect;
                OutTex.ReadPixels(equirectangularArea, 0, 0);
                OutTex.Apply();
                RenderTexture.active = oldActive;
            }

            private void StartRecording()
            {
                recording = true;
                progress.SetActive(true);
                Time.captureFramerate = targetFramerate;
            }

            private void StopRecording()
            {
                recording = false;
                progress.SetActive(false);
                Time.captureFramerate = 0;
            }

            private void HandleCapture()
            {
                if (recording)
                {
                    if (framesCaptured < totalFrames)
                    {
                        Capture();
                        framesCaptured++;
                        progressText.text = string.Format("Recorded {0} Frames / {1}", framesCaptured, totalFrames);
                    }
                    else
                    {
                        StopRecording();
                        Debug.Log("DONE RECORDING");
                    }
                }
            }

            private void Capture()
            {
                if (cam)
                {
                    if (exportEquirectangular)
                    {
                        CubemapToEquirectangular();
                    }
                    else
                    {
                        CubemapToBitmapLayout();
                    }
                    byte[] OutBuffer = OutTex.EncodeToPNG();
                    string outPath = string.Format("{0}/{1}{2:D6}.png", imageFolder, imagePrefix, framesCaptured);
                    WriteFileAsync(OutBuffer, outPath);
                }
            }

            private async void WriteFileAsync(byte[] buffer, string outPath)
            {
                using (FileStream fs = File.Open(outPath, FileMode.Create))
                {
                    fs.Seek(0, SeekOrigin.End);
                    await fs.WriteAsync(buffer, 0, buffer.Length);
                }
            }

            void Update()
            {
                if (Input.GetKeyDown("space"))
                {
                    if (recording)
                    {
                        StopRecording();
                    }
                    else
                    {
                        StartRecording();
                    }
                }
                HandleCapture();
            }
        }
#endif
    }
}

