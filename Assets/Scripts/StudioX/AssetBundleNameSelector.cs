using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
namespace StudioX
{
    /// <summary>
    /// Basic class to store the name of an Asset on a <see cref="UnityEngine.UI.Button"/>
    /// and supply a callback for when it's clicked. Intended to be used with <see cref="AssetBundleMenuManager"/>
    /// to dynamically load content retrieved from AssetBundles.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class AssetBundleNameSelector : MonoBehaviour
    {
        /// <summary>The name of the asset.</summary>
        public string assetName;
        /// <summary>Delegate for <see cref="menuButton"/> onClick handlers.</summary> 
        public delegate void ClickCallback(string asset);
        /// <summary>
        /// The <see cref="UnityEngine.UI.Button"/> attached to <see cref="AssetBundleNameSelector.gameObject"/>.
        /// </summary>
        private Button menuButton;
        /// <summary>Sets <see cref="menuButton"/>.</summary>
        public void Awake()
        {
            menuButton = gameObject.GetComponent<Button>();
        }
        /// <summary>
        /// Sets method with signature <see cref="ClickCallback"/> as 
        /// <see cref="menuButton"/>'s onClick handler.
        /// </summary>
        /// <param name="callback">The method to be set. Must have signature that matches <see cref="ClickCallback"/>.</param>
        public void SetCallback(ClickCallback callback)
        {
            menuButton.onClick.AddListener(delegate { callback(assetName); });
        }
    }
#if UNITY_EDITOR
    /// <summary>
    /// Custom inspector layout for <see cref="AssetBundleNameSelector"/>. Only works in Editor.
    /// </summary>
    [CustomEditor(typeof(AssetBundleNameSelector))]
    public class AssetBundleNameSelectorEditorLayout : Editor
    {
        /// <summary>The SerializedProperty for <see cref="AssetBundleNameSelector.assetName"/>.</summary>
        public SerializedProperty assetName;
        /// <summary> The <see cref="UnityEngine.GameObject"/> placeholder for <see cref="assetName"/>.</summary>
        private GameObject assetObj;
        /// <summary>Initializes all SerializedProperty fields from <see cref="AssetBundleNameSelectorEditorLayout.serializedObject"/>.</summary>
        public void OnEnable()
        {
            assetName = serializedObject.FindProperty("assetName");
            string[] assets = AssetDatabase.FindAssets(assetName.stringValue);
            if (assets.Length > 0)
            {
                assetObj = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(assets[0]));
            }
        }
        /// <summary>Sets all of the SerializedProperty fields from the inspector GUI.</summary>
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