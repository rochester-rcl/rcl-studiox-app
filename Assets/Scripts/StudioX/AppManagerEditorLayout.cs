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
        /// <summary>The SerializedProperty for <see cref="AppManager.landingScene"/>.</summary>
        SerializedProperty landingScene;
        /// <summary> The <see cref="UnityEditor.SceneAsset"/> placeholder for <see cref="landingScene"/>.</summary>
        private SceneAsset landingSceneObj;
        /// <summary>The SerializedProperty for <see cref="AppManager.loadingScreen"/>.</summary>
        SerializedProperty loadingScreen;
        /// <summary>The SerializedProperty for <see cref="AppManager.menuScene"/>.</summary>
        SerializedProperty menuScene;
        /// <summary> The <see cref="UnityEditor.SceneAsset"/> placeholder for <see cref="menuScene"/>.</summary>
        private SceneAsset menuSceneObj;
        /// <summary>The SerializedProperty for <see cref="AppManager.firebaseMessagingTopic"/>.</summary>
        SerializedProperty firebaseMessagingTopic;
        /// <summary>Initializes all SerializedProperty fields from <see cref="AppManagerEditorLayout.serializedObject"/>.</summary>
        public void OnEnable()
        {
            landingScene = serializedObject.FindProperty("landingScene");
            landingSceneObj = LoadSceneAsset(landingScene.stringValue);
            loadingScreen = serializedObject.FindProperty("loadingScreen");
            menuScene = serializedObject.FindProperty("menuScene");
            menuSceneObj = LoadSceneAsset(menuScene.stringValue);
            firebaseMessagingTopic = serializedObject.FindProperty("firebaseMessagingTopic");
        }
        /// <summary>Loads a <see cref="UnityEditor.SceneAsset" /> from a scene name. </summary>
        /// <param name="sceneName">The name of the <see cref="UnityEditor.SceneAsset" /> to load.</param>
        /// <para>Note that if there are multiple scenes with the same name the first scene found will be returned</para>
        /// <returns>A <see cref="UnityEditor.SceneAsset" /> if found, or null if not found.</returns>
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
        /// <summary>Sets all of the SerializedProperty fields from the inspector GUI.</summary>
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

            firebaseMessagingTopic.stringValue = EditorGUILayout.TextField("Firebase Messaging Topic", firebaseMessagingTopic.stringValue);

            serializedObject.ApplyModifiedProperties();
        }

    }
#endif
}
