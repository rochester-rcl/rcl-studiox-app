using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
namespace StudioX
{
    // TODO should change this name because it doesn't reflect what the class actually does

    /// <summary>
    /// Component for loading a Scene with <see cref="AppManager"/> via a <see cref="UnityEngine.UI.Button"/>.
    /// </summary> 
    [RequireComponent(typeof(Button))]
    public class MenuButtonHandler : MonoBehaviour
    {
        /// <summary>
        /// The <see cref="UnityEngine.UI.Button"/> to attach <see cref="ClickHandler"/> to.
        /// </summary>
        public Button MenuButton { get; set; }
        /// <summary>
        /// The name of the scene that will be loaded when <see cref="MenuButton"/> is clicked.
        /// </summary>
        public string sceneName;
        /// <summary>
        /// Whether or not the scene to be loaded is a VR scene.
        /// </summary>
        public bool isVR = false;
        /// <summary>
        /// The <see cref="AppManager"/> instance currently loaded in the scene.
        /// </summary>
        private AppManager Manager { get; set; }
        /// <summary>
        /// Initializes <see cref="Manager"/> and <see cref="MenuButton"/>.
        /// Adds <see cref="ClickHandler"/> to <see cref="MenuButton"/>.
        /// </summary>
        /// <exception cref="UnityEngine.MissingComponentException"/>
        /// Will be thrown if no AppManager instance is present in the scene.
        /// </exception>
        public void Start()
        {
            MenuButton = gameObject.GetComponent<Button>();
            Manager = AppManager.GetManager();
            if (!Manager)
            {
                throw new MissingComponentException("There must be an instance of AppManager attached to a GameObject in the scene");
            }
            MenuButton.onClick.AddListener(ClickHandler);
        }
        /// <summary>
        /// Loads <see cref="sceneName"/> when <see cref="MenuButton" /> is clicked.
        /// </summary>
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
    /// <summary>
    /// Custom inspector layout for <see cref="MenuButtonHandler"/>. Only works in Editor.
    /// </summary> 
    public class MenuButtonHandlerEditorLayout : Editor
    {
        /// <summary>
        /// The SerializedProperty for <see cref="MenuButtonHandler.sceneName"/>.
        /// </summary>
        SerializedProperty sceneName;
        /// <summary> 
        /// The SerializedProperty for <see cref="MenuButtonHandler.isVR"/>.
        /// </summary>
        SerializedProperty isVR;
        /// <summary>
        /// The <see cref="UnityEditor.SceneAsset"/> placeholder for <see cref="sceneName"/>.
        /// </summary>
        private SceneAsset sceneObj;
        /// <summary>
        /// Initializes all SerializedProperty fields from MenuButtonHandlerEditorLayout.serializedObject.
        /// </summary>
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
        /// <summary>Sets all of the SerializedProperty fields from the inspector GUI.</summary>
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

