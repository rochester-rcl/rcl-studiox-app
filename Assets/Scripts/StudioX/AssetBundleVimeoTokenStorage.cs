using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vimeo.Player;
using UnityEditor;
namespace StudioX
{
    public class AssetBundleVimeoTokenStorage : MonoBehaviour
    {
        public string VimeoToken { get { return _token; } set { _token = value; } }
        public GameObject vimeoPlayerContainer; 
        [SerializeField]
        private string _token;
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(AssetBundleVimeoTokenStorage))]
    public class AssetBundleVimeoTokenStorageEditorLayout : Editor {

        public SerializedProperty token;
        public SerializedProperty vimeoPlayerContainer;
        private GameObject currentPlayerContainer;
        private VimeoPlayer player;

        public void OnEnable()
        {
            token = serializedObject.FindProperty("_token");
            vimeoPlayerContainer = serializedObject.FindProperty("vimeoPlayerContainer");
            UpdateToken();
        }

        public void UpdateToken()
        {
            currentPlayerContainer = vimeoPlayerContainer.objectReferenceValue as GameObject;
            player = currentPlayerContainer.GetComponent<VimeoPlayer>();
            token.stringValue = player.GetVimeoToken();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.PropertyField(vimeoPlayerContainer);
            }
            if (EditorGUI.EndChangeCheck())
            {
                UpdateToken();
            }
            EditorGUILayout.LabelField(string.Format("Vimeo Token: {0}", token.stringValue));
            serializedObject.ApplyModifiedProperties();
        }

    }
    #endif
}

