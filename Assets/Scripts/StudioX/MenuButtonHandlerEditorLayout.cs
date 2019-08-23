using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace StudioX
{
#if UNITY_EDITOR
    [CustomEditor(typeof(MenuButtonHandler))]
    public class MenuButtonHandlerEditorLayout : Editor
    {
        SerializedProperty sceneName;
        [SerializeField]
        private UnityEngine.Object sceneObj;
        // TODO persist the object id
        void OnEnable()
        {
            sceneName = serializedObject.FindProperty("sceneName");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            sceneObj = EditorGUILayout.ObjectField("Scene", sceneObj, typeof(SceneAsset), false);
            sceneName.stringValue = sceneObj ? sceneObj.name : null;
            serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif