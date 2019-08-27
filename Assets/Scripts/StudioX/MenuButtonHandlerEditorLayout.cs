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
        private UnityEngine.Object sceneObj;
        [SerializeField]
        private bool vrState;
        private string EditorPrefsKey;
        private string EditorPrefsKeyPrefix = "MenuButtonHander_";

        void OnEnable()
        {
            sceneName = serializedObject.FindProperty("sceneName");
            isVR = serializedObject.FindProperty("isVR");
            EditorPrefsKey = string.Format("{0}{1}", EditorPrefsKeyPrefix, serializedObject.targetObject.name);
            LoadPrefs();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            sceneObj = EditorGUILayout.ObjectField("Scene", sceneObj, typeof(SceneAsset), false);
            sceneName.stringValue = sceneObj ? sceneObj.name : null;
            vrState = EditorGUILayout.Toggle("VR Scene", vrState);
            isVR.boolValue = vrState;
            serializedObject.ApplyModifiedProperties();
        }

        void OnDisable()
        {
            SavePrefs();
        }

        private void SavePrefs()
        {
            string data = JsonUtility.ToJson(this, false);
            if (!string.IsNullOrEmpty(data))
            {
                EditorPrefs.SetString(EditorPrefsKey, data);
            }
        }

        private void LoadPrefs()
        {
            string data = EditorPrefs.GetString(EditorPrefsKey, JsonUtility.ToJson(this, false));
            if (!string.IsNullOrEmpty(data))
            {
                JsonUtility.FromJsonOverwrite(data, this);
            }
        }
    }
}
#endif
