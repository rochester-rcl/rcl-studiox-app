using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

namespace StudioX
{
    namespace ScreenRecorder
    {
#if UNITY_EDITOR
        public class Simple360Capture : MonoBehaviour
        {
            private Cubemap tex;
            public int cubemapSize = 512;
            public int targetFramerate = 30;
            public int duration;
            public bool startCaptureOnStart = true;
            public string imagePrefix;
            private Texture2D OutTex { get; set; }
            private CubemapFace[] CubemapFaces = { CubemapFace.PositiveX, CubemapFace.NegativeX,
            CubemapFace.PositiveY, CubemapFace.NegativeY,
            CubemapFace.PositiveZ, CubemapFace.NegativeZ };
            private int framesCaptured;
            private int totalFrames;
            private string imagesDir;
            private Camera cam;
            private GameObject progress;
            private Text progressText;
            private bool recording;
            void Start()
            {
                cam = Camera.main;
                cam.tag = "Untagged";
                // IGNORE UI LAYER
                cam.cullingMask = ~(1 << 5);
                framesCaptured = 0;
                totalFrames = duration * targetFramerate;
                Application.targetFrameRate = targetFramerate;
                imagesDir = FindRecordingsDir();
                tex = new Cubemap(cubemapSize, TextureFormat.RGBA32, false);
                tex.anisoLevel = 9;
                OutTex = new Texture2D(tex.width, tex.height * 6, TextureFormat.RGBA32, false);
                InitCanvas();
                if (startCaptureOnStart)
                {
                    recording = true;
                    StartCoroutine(ExecRecording());
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

            private IEnumerator ExecRecording()
            {
                recording = true;
                progress.SetActive(true);
                while (framesCaptured < totalFrames && recording)
                {
                    yield return new WaitForEndOfFrame();
                    Capture();
                }
                recording = false;
                framesCaptured = 0;
                progress.SetActive(false);
                Debug.Log("DONE RECORDING");
            }

            private void Capture()
            {
                if (cam)
                {
                    CubemapToBitmapLayout();
                    byte[] OutBuffer = OutTex.EncodeToPNG();
                    string outPath = string.Format("{0}/{1}{2:D6}.png", imagesDir, imagePrefix, framesCaptured);
                    WriteFileAsync(OutBuffer, outPath);
                    framesCaptured++;
                    progressText.text = string.Format("Recorded {0} Frames / {1}", framesCaptured, totalFrames);
                }
            }

            private async void WriteFileAsync(byte[] buffer, string outPath)
            {
                string parentDir = Path.GetDirectoryName(outPath);
                if (!Directory.Exists(Path.GetDirectoryName(outPath)))
                {
                    Directory.CreateDirectory(parentDir);
                }
                using (FileStream fs = File.Open(outPath, FileMode.Create))
                {
                    fs.Seek(0, SeekOrigin.End);
                    await fs.WriteAsync(buffer, 0, buffer.Length);
                }
            }

            void Update()
            {
                if (!startCaptureOnStart)
                {
                    if (Input.GetKeyDown("space"))
                    {
                        if (recording)
                        {
                            recording = false;
                        }
                        else
                        {
                            StartCoroutine(ExecRecording());
                        }

                    }
                }
            }
        }
#endif
    }
}

