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
        private Rect lastRect;
        public SerializedProperty firebaseMessagingTopics;
        public SerializedProperty remoteAssetBundleMapper;
        public SerializedProperty fbTopicArraySize;
        public void OnEnable()
        {
            landingScene = serializedObject.FindProperty("landingScene");
            landingSceneObj = LoadSceneAsset(landingScene.stringValue);
            loadingScreen = serializedObject.FindProperty("loadingScreen");
            errorScreen = serializedObject.FindProperty("errorScreen");
            menuScene = serializedObject.FindProperty("menuScene");
            menuSceneObj = LoadSceneAsset(menuScene.stringValue);
            firebaseMessagingTopics = serializedObject.FindProperty("firebaseMessagingTopics");
            fbTopicArraySize = firebaseMessagingTopics.FindPropertyRelative("Array.size");
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

        private void DrawFirebaseTopicSelector()
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Firebase Cloud Messaging Topics", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Note: all topics must be lower case and have no spaces or special characters.");
            EditorGUILayout.Space();
            {
                if (GUILayout.Button("Add", GUILayout.Width(100)))
                {
                    fbTopicArraySize.intValue++;
                }
                EditorGUILayout.Space();
                for (int i = 0; i < fbTopicArraySize.intValue; i++)
                {
                    SerializedProperty fbMessagingTopic = firebaseMessagingTopics.GetArrayElementAtIndex(i);
                    GUILayout.BeginHorizontal();
                    {
                        fbMessagingTopic.stringValue = EditorGUILayout.TextField(fbMessagingTopic.stringValue);
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Remove"))
                        {
                            firebaseMessagingTopics.DeleteArrayElementAtIndex(i);
                        }
                    }
                    GUILayout.EndHorizontal();
                    if (i % 2 == 0)
                    {
                        DrawSeparator();
                    }
                    GUILayout.Space(10);
                }
            }
            GUILayout.EndHorizontal();
        }

        public void DrawSeparator()
        {
            lastRect = GUILayoutUtility.GetLastRect();
            float h = lastRect.height;
            lastRect.y = (lastRect.y + h) + 5;
            lastRect.height = 1;
            EditorGUI.DrawRect(lastRect, Color.gray);
            GUILayout.Space(10);
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
            DrawFirebaseTopicSelector();
            // firebaseMessagingTopic.stringValue = EditorGUILayout.TextField("Firebase Messaging Topic", firebaseMessagingTopic.stringValue);

            serializedObject.ApplyModifiedProperties();
        }

    }
#endif
}
