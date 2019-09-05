using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
namespace StudioX
{
    [CustomEditor(typeof(MenuButtonHandler))]
    public class MenuButtonHandlerEditorLayout : Editor
    {
        SerializedProperty sceneName;
        SerializedProperty isVR;
        [SerializeField]
        private SceneAsset sceneObj;
        [SerializeField]
        private bool vrState;

        void OnEnable()
        {
            sceneName = serializedObject.FindProperty("sceneName");
            isVR = serializedObject.FindProperty("isVR");
            string[] scenes = AssetDatabase.FindAssets(sceneName.stringValue);
            if (scenes.Length > 0)
            {
                sceneObj = AssetDatabase.LoadAssetAtPath<SceneAsset>(AssetDatabase.GUIDToAssetPath(scenes[0]));
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            sceneObj = EditorGUILayout.ObjectField("Scene", sceneObj, typeof(SceneAsset), false) as SceneAsset;
            sceneName.stringValue = sceneObj ? sceneObj.name : null;
            vrState = EditorGUILayout.Toggle("VR Scene", vrState);
            isVR.boolValue = vrState;
            serializedObject.ApplyModifiedProperties();
        }

    }
}
#endif
