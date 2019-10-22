using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace StudioX
{
#if UNITY_EDITOR
    [CustomEditor(typeof(AppManager))]
    public class AppManagerEditorLayout : Editor
    {
        public SerializedProperty landingScene;
        private SceneAsset landingSceneObj;
        public SerializedProperty loadingScreen;
        public SerializedProperty errorScreen;
        public SerializedProperty menuScene;
        private SceneAsset menuSceneObj;
        public SerializedProperty firebaseMessagingTopic;
        public SerializedProperty remoteAssetBundleMapper;
        public void OnEnable()
        {
            landingScene = serializedObject.FindProperty("landingScene");
            landingSceneObj = LoadSceneAsset(landingScene.stringValue);
            loadingScreen = serializedObject.FindProperty("loadingScreen");
            errorScreen = serializedObject.FindProperty("errorScreen");
            menuScene = serializedObject.FindProperty("menuScene");
            menuSceneObj = LoadSceneAsset(menuScene.stringValue);
            firebaseMessagingTopic = serializedObject.FindProperty("firebaseMessagingTopic");
            remoteAssetBundleMapper = serializedObject.FindProperty("remoteAssetBundleMapper");
        }

        private SceneAsset LoadSceneAsset(string sceneName)
        {
            string[] scenes = AssetDatabase.FindAssets(sceneName);
            if (scenes.Length > 0)
            {
                return AssetDatabase.LoadAssetAtPath<SceneAsset>(AssetDatabase.GUIDToAssetPath(scenes[0]));
            }
            else
            {
                return null;
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            /*** LANDING SCENE ***/
            landingSceneObj = EditorGUILayout.ObjectField("Landing Scene", landingSceneObj, typeof(SceneAsset), false) as SceneAsset;
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

            /*** Error SCREEN ***/
            EditorGUILayout.PropertyField(errorScreen);

            /*** MENU SCENE ***/
            menuSceneObj = EditorGUILayout.ObjectField("Menu Scene", menuSceneObj, typeof(SceneAsset), true) as SceneAsset;
            if (menuSceneObj)
            {
                menuScene.stringValue = menuSceneObj.name;
            }
            else
            {
                menuScene.stringValue = null;
            }

            /*** REMOTE ASSET BUNDLE MAPPER ***/
            EditorGUILayout.PropertyField(remoteAssetBundleMapper);

            firebaseMessagingTopic.stringValue = EditorGUILayout.TextField("Firebase Messaging Topic", firebaseMessagingTopic.stringValue);

            serializedObject.ApplyModifiedProperties();
        }

    }
#endif
}
