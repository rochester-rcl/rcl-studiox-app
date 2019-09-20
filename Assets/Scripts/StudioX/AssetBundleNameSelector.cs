using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
namespace StudioX
{
    public class AssetBundleNameSelector : MonoBehaviour
    {
        public string assetName;
        private Button menuButton;

        public void Awake()
        {
            menuButton = gameObject.GetComponent<Button>();
            if (!menuButton)
            {
                Debug.LogWarning("A Button Component much be attached in order to use AssetBundeNameSelector");
            }
        }

        public delegate void ClickCallback(string asset);
        public void SetCallback(ClickCallback callback)
        {
            menuButton.onClick.AddListener(delegate { callback(assetName); });
        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(AssetBundleNameSelector))]
    public class AssetBundeNameSelectorEditorLayout : Editor
    {
        public SerializedProperty assetName;
        private GameObject assetObj;

        public void OnEnable()
        {
            assetName = serializedObject.FindProperty("assetName");
            string[] assets = AssetDatabase.FindAssets(assetName.stringValue);
            if (assets.Length > 0)
            {
                assetObj = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(assets[0]));
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            assetObj = EditorGUILayout.ObjectField("Asset Bundle Asset", assetObj, typeof(GameObject), false) as GameObject;
            assetName.stringValue = assetObj.name;
            serializedObject.ApplyModifiedProperties();

        }
    }
#endif
}