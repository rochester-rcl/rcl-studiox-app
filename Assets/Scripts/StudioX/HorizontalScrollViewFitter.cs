using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
namespace StudioX
{
    [RequireComponent(typeof(LayoutElement))]
    public class HorizontalScrollViewFitter : MonoBehaviour
    {
        public string targetTransformName;
        private RectTransform targetTransform;
        private LayoutElement layout;
        private bool landscape = false;
        public void Start()
        {
            GameObject targetTransformObj = GameObject.Find(targetTransformName);
            targetTransform = targetTransformObj.GetComponent<RectTransform>();
            if (!targetTransform)
            {
                throw new MissingComponentException("Target Transform GameObject must have a RectTransform Component");
            }
            layout = gameObject.GetComponent<LayoutElement>();
        }

        public void Update()
        {
            if (targetTransform.rect.width != layout.preferredWidth)
            {
                layout.preferredWidth = targetTransform.rect.width;
            }
            if (targetTransform.rect.height != layout.preferredHeight)
            {
                layout.preferredHeight = targetTransform.rect.height;
                layout.minHeight = targetTransform.rect.height;
            }
        }
    }

    #if UNITY_EDITOR
    [CustomEditor(typeof(HorizontalScrollViewFitter))]
    public class HorizontalScrollViewFitterEditorLayout : Editor 
    {
        public SerializedProperty targetTransformName;
        private GameObject targetTransformObj;

        public void OnEnable()
        {
            targetTransformName = serializedObject.FindProperty("targetTransformName");
            targetTransformObj = GameObject.Find(targetTransformName.stringValue);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            targetTransformObj = EditorGUILayout.ObjectField(targetTransformObj, typeof(GameObject), true) as GameObject;
            targetTransformName.stringValue = targetTransformObj.name;
            serializedObject.ApplyModifiedProperties();
        }
    }
    #endif
}

