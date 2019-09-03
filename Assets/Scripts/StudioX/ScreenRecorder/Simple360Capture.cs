using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioX
{
    namespace ScreenRecorder
    {
#if UNITY_EDITOR
        public class Simple360Capture : MonoBehaviour
        {
            private Cubemap tex;
            public Camera cam;
            public int cubemapSize = 512;
            public int targetFramerate = 30;
            public int duration;
            public bool startCaptureOnStart = true;
            public string imagePrefix;
            private Texture2D OutTex { get; set; }
            private byte[] OutBuffer { get; set; }
            private CubemapFace[] CubemapFaces = { CubemapFace.PositiveX, CubemapFace.NegativeX,
            CubemapFace.PositiveY, CubemapFace.NegativeY,
            CubemapFace.PositiveZ, CubemapFace.NegativeZ };
            private int framesCaptured;
            private int totalFrames;
            private string imagesDir;
            void Start()
            {
                framesCaptured = 0;
                totalFrames = duration * targetFramerate;
                Application.targetFrameRate = targetFramerate;
                imagesDir = FindRecordingsDir();
                tex = new Cubemap(512, TextureFormat.RGBA32, false);
                tex.anisoLevel = 9;
                OutTex = new Texture2D(tex.width, tex.height * 6, TextureFormat.RGBA32, false);
                StartCoroutine(ExecRecording());
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
                while (framesCaptured < totalFrames)
                {
                    yield return new WaitForEndOfFrame();
                    Capture();
                }
                Debug.Log("DONE RECORDING");
            }

            private void Capture()
            {
                if (cam)
                {
                    CubemapToBitmapLayout();
                    OutBuffer = OutTex.EncodeToPNG();
                    string outPath = string.Format("{0}/{1}{2:D6}.png", imagesDir, imagePrefix, framesCaptured);
                    System.IO.File.WriteAllBytes(outPath, OutBuffer);
                    framesCaptured++;
                }
            }

            void Update()
            {
                if (!startCaptureOnStart)
                {
                    if (Input.GetKeyDown("space"))
                    {
                        StartCoroutine(ExecRecording());
                    }
                }
            }
        }
#endif
    }
}

