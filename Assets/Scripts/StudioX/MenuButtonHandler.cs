using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
namespace StudioX
{
    public class MenuButtonHandler : MonoBehaviour
    {
        public Button MenuButton { get; set; }
        public string sceneName;
        public bool isVR = false;
        private AppManager Manager { get; set; }
        public void Start()
        {
            MenuButton = GetComponent<Button>();
            Manager = AppManager.GetManager();
            if (!Manager)
            {
                Debug.LogError("There must be an instance of AppManager attached to a GameObject in the scene");
            }
            if (MenuButton)
            {
                MenuButton.onClick.AddListener(ClickHandler);
            }
            else
            {
                Debug.LogError("MenuButtonHandler must be attached to a Button component!");
            }
        }
        public void ClickHandler()
        {
            if (Manager)
            {
                if (!isVR)
                {
                    Manager.LoadScene(sceneName);
                }
                else
                {
                    Manager.LoadVRScene(sceneName);
                }
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(MenuButtonHandler))]
    public class MenuButtonHandlerEditorLayout : Editor
    {
        SerializedProperty sceneName;
        SerializedProperty isVR;
        [SerializeField]
        private SceneAsset sceneObj;

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
            isVR.boolValue = EditorGUILayout.Toggle("VR Scene", isVR.boolValue);
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}

