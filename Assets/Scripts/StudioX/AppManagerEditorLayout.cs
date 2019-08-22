using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
namespace StudioX
{
#if UNITY_EDITOR
    [CustomEditor(typeof(AppManager))]
    public class AppManagerEditorLayout : Editor
    {
        SerializedProperty landingScene;
        [SerializeField]
        private UnityEngine.Object landingSceneObj;
        SerializedProperty loadingScreen;
        SerializedProperty menuScene;
        [SerializeField]
        private UnityEngine.Object menuSceneObj;
        SerializedProperty firebaseMessagingTopic;
        [SerializeField]
        private string messagingTopic;
        private const string EditorPrefsKey = "AppManagerEditorLayoutKey";
        void OnEnable()
        {
            landingScene = serializedObject.FindProperty("landingScene");
            Debug.Log(landingScene.stringValue);
            loadingScreen = serializedObject.FindProperty("loadingScreen");
            menuScene = serializedObject.FindProperty("menuScene");
            firebaseMessagingTopic = serializedObject.FindProperty("firebaseMessagingTopic");

            // Hydrate the private objects set here
            string data = EditorPrefs.GetString(EditorPrefsKey, JsonUtility.ToJson(this, false));
            if (!string.IsNullOrEmpty(data))
            {
                JsonUtility.FromJsonOverwrite(data, this);
            }
        }

        void SavePrefs()
        {
            string data = JsonUtility.ToJson(this, false);
            Debug.Log(data);
            if (!string.IsNullOrEmpty(data))
            {
                EditorPrefs.SetString(EditorPrefsKey, data);
            }
        }

        void OnDisable()
        {
            SavePrefs();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            /*** LANDING SCENE ***/
            landingSceneObj = EditorGUILayout.ObjectField("Landing Scene", landingSceneObj, typeof(SceneAsset), false);
            if (landingSceneObj)
            {
                landingScene.stringValue = landingSceneObj.name;
            }
            else
            {
                landingScene.stringValue = null;
            }

            /*** LOADING SCREEN ***/
            EditorGUILayout.PropertyField(loadingScreen);

            /*** MENU SCENE ***/
            menuSceneObj = EditorGUILayout.ObjectField("Menu Scene", menuSceneObj, typeof(SceneAsset), true);
            if (menuSceneObj)
            {
                menuScene.stringValue = menuSceneObj.name;
            }
            else
            {
                menuScene.stringValue = null;
            }

            // Also circumvent the odd control character bug
            messagingTopic = EditorGUILayout.TextField("Firebase Messaging Topic", messagingTopic);
            firebaseMessagingTopic.stringValue = messagingTopic;

            serializedObject.ApplyModifiedProperties();
        }

    }
#endif
}
