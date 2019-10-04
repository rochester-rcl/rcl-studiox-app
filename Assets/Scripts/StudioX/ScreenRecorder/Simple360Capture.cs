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
        /// <summary> A simple 360 recorder that exports a numbered png sequence. </summary>
        public class Simple360Capture : MonoBehaviour
        {
            /// <summary>
            /// The <see cref="UnityEngine.Cubemap"/> that the camera will render to
            /// if using <see cref="CubemapToBitmapLayout"/>.
            /// </summary>
            private Cubemap cube;
            /// <summary>
            /// The intermediate <see cref="UnityEngine.RenderTexture"/> that the camera will render
            /// a cubemap to if using <see cref="CubemapToEquirectangular"/>.
            /// </summary> 
            private RenderTexture rt;
            /// <summary>
            /// The <see cref="UnityEngine.RenderTexture"/> that <see cref="rt"/> cubemap will be converted to
            /// when using <see cref="CubemapToEquirectangular"/>.
            /// </summary>
            private RenderTexture equirect;
            /// <summary>The size of each individual face of the cubemap the camera will render to.</summary>
            public int cubemapSize = 512;
            /// <summary>
            /// The width of the equirectangular bitmap if using <see cref="CubemapToEquirectangular"/>.
            /// </summary>
            /// <remarks>Note: The aspect ratio of the resulting bitmap is 2:1, so if <see cref="equirectangularWidth"/>
            /// is 2048, the height of the image will be 1024.
            /// </summary>
            public int equirectangularWidth = 2048;
            /// <summary>The rate at which the resulting sequence will be played back.</summary>
            public int targetFramerate = 30;
            /// <summary>The duration of the recording.</summary>
            public int duration;
            /// <summary>
            /// Whether or not to start recording immediately.
            /// If false, recording can be started by pressing the spacebar.
            /// </summary>
            public bool startCaptureOnStart = true;
            /// <summary>
            /// Whether or not to export an equirectangular image.
            /// If false, a cubemap layout with aspect ratio 1:6 will be exported.
            /// </summary> 
            /// <remarks> 
            /// For example: if exporting a cubemap layout with <see cref="cubemapSize"/> of 512, 
            /// the resulting file will have a resolution of 512x3072.
            /// </remarks>
            public bool exportEquirectangular;
            /// <summary>The prefix for each image file, i.e. frame_ or image_ .</summary>
            public string imagePrefix;
            /// <summary>The local folder that the image sequence will be saved to. </summary>
            public string imageFolder;
            /// <summary>
            /// The intermediate texture that all images will be written to before they're saved. 
            /// </summary>
            private Texture2D OutTex { get; set; }
            /// <summary>The faces of the cubemap.</summary>
            private CubemapFace[] CubemapFaces = { CubemapFace.PositiveX, CubemapFace.NegativeX,
            CubemapFace.PositiveY, CubemapFace.NegativeY,
            CubemapFace.PositiveZ, CubemapFace.NegativeZ };
            /// <summary>The number of frames currently captured.</summary>
            private int framesCaptured;
            /// <summary>
            /// The total number of frames, calculated by <see cref="targetFramerate"/> and <see cref="duration"/>.
            /// </summary>
            private int totalFrames;
            /// <summary>The camera to render from (MainCamera).</summary>
            private Camera cam;
            /// <summary>
            /// The <see cref="UnityEngine.GameObject"/> that displays the progress message while recording.
            /// </summary>
            private GameObject progress;
            /// <summary>The text of the progess message. </summary>
            private Text progressText;
            /// <summary>Whether or not the Component is currently recording.</summary>
            private bool recording;
            /// <summary>The bounding box of the equirectangular output bitmap.</summary>
            private Rect equirectangularArea;
            /// <summary> 
            /// Initializes all render textures and sets <see cref="cam"/>'s culling mask to ignore
            /// the UI layer. If <see cref="startCaptureOnStart"/> is set to true, then recording will begin immediately.
            /// </summary>
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
                    cube = new Cubemap(cubemapSize, TextureFormat.RGBA32, false);
                    cube.anisoLevel = 9;
                    OutTex = new Texture2D(cube.width, cube.height * 6, TextureFormat.RGBA32, false);
                }

               
                InitCanvas();
                if (startCaptureOnStart)
                {
                    StartRecording();
                }
            }
            /// <summary>Initialzes the progress <see cref="UnityEngine.Canvas" />.</summary>
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
            /// <summary>
            /// If <see cref="imagesFolder"/> is null, creates a folder in <see cref="Application.dataPath"/>
            /// called "Recordings" and saves frames there.
            /// </summary>
            private string FindRecordingsDir()
            {
                string projectDir = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/'));
                string recordingsDir = string.Format("{0}/Recordings", projectDir);
                System.IO.Directory.CreateDirectory(recordingsDir);
                return recordingsDir;
            }
            /// <summary>
            /// Responsible for converting <see cref="cube"/> to a bitmap layout (1:6 resolution png).
            /// </summary>
            private void CubemapToBitmapLayout()
            {
                cam.RenderToCubemap(cube, 63);
                cube.SmoothEdges();
                Unity.Collections.NativeArray<Color32> rawData = OutTex.GetRawTextureData<Color32>();
                int index = 0;
                for (int i = CubemapFaces.Length; i > 0; --i)
                {
                    Color[] face = cube.GetPixels(CubemapFaces[i - 1]);
                    for (int y = 0; y < cube.height; y++)
                    {
                        for (int x = 0; x < cube.width; x++)
                        {
                            // invert the input tex since origin is bottom left corner
                            int faceIndex = ((cube.height - 1 - y) * cube.width + x);
                            rawData[index++] = face[faceIndex];
                        }
                    }
                }
                OutTex.Apply();
            }
            /// <summary>
            /// Responsible for converting <see cref="rt"/> cubemap to an image with equirectangular projection.
            /// </summary>
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
            /// <summary>Starts the recording.</summary>
            private void StartRecording()
            {
                recording = true;
                progress.SetActive(true);
                Time.captureFramerate = targetFramerate;
            }
            /// <summary>Stops the recording.</summary>
            private void StopRecording()
            {
                recording = false;
                cam.cullingMask = ~(-1);
                progress.SetActive(false);
                Time.captureFramerate = 0;
            }
            /// <summary>
            /// Responsible for continuing or stopping capture based on <see cref="framesCaptured"/>.
            /// </summary>
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
            /// <summary>Performs cubemap rendering and saves the current frame to a file.</summary>
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
            /// <summary>Asynchronously writes a frame to a file.</summary>
            /// <param name="buffer">The pixel data.</param>
            /// <param name="outPath">The name of the output file.</param>
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

