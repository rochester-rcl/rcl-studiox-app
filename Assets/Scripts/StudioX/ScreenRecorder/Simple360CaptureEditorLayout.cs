using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
namespace StudioX
{
    namespace ScreenRecorder
    {
        [CustomEditor(typeof(Simple360Capture))]
        public class Simple360CaptureEditorLayout : Editor
        {
            SerializedProperty cubemapSize;
            SerializedProperty targetFramerate;
            SerializedProperty duration;
            SerializedProperty startCaptureOnStart;
            SerializedProperty imagePrefix;
            SerializedProperty imageFolder;
            SerializedProperty exportEquirectangular;
            SerializedProperty equirectangularWidth;
            void OnEnable()
            {
                cubemapSize = serializedObject.FindProperty("cubemapSize");
                targetFramerate = serializedObject.FindProperty("targetFramerate");
                duration = serializedObject.FindProperty("duration");
                startCaptureOnStart = serializedObject.FindProperty("startCaptureOnStart");
                exportEquirectangular = serializedObject.FindProperty("exportEquirectangular");
                equirectangularWidth = serializedObject.FindProperty("equirectangularWidth");
                imagePrefix = serializedObject.FindProperty("imagePrefix");
                imageFolder = serializedObject.FindProperty("imageFolder");
            }

            public override void OnInspectorGUI()
            {
                serializedObject.Update();
                EditorGUILayout.PropertyField(cubemapSize);
                EditorGUILayout.PropertyField(targetFramerate);
                EditorGUILayout.PropertyField(duration);
                EditorGUILayout.PropertyField(startCaptureOnStart);
                EditorGUILayout.PropertyField(exportEquirectangular);
                if (exportEquirectangular.boolValue)
                {
                    EditorGUILayout.PropertyField(equirectangularWidth);
                }
                EditorGUILayout.PropertyField(imagePrefix);
                GUILayout.BeginHorizontal();
                {
                    imageFolder.stringValue = EditorGUILayout.TextField("Image Folder", imageFolder.stringValue);
                    if (GUILayout.Button("Open"))
                    {
                        imageFolder.stringValue = EditorUtility.OpenFolderPanel("Select Folder to Save Frames", "", "");
                    }
                }
                GUILayout.EndHorizontal();
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
#endif
