using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace StudioX
{
    /// <summary>Toggles a Canvas on and off.</summary>
    public class CanvasToggleButtonHandler : MonoBehaviour
    {
        /// <summary>The <see cref="UnityEngine.GameObject"/> containing
        /// the <see cref="UnityEngine.Canvas"/> to toggle.
        /// </summary>
        public GameObject targetCanvas;
        /// <summary>The <see cref="UnityEngine.GameObject"/> containing
        /// the <see cref="UnityEngine.UI.Button"/> that will hide <see cref="targetCanvas"/>.
        /// </summary>
        public GameObject closeButtonObject;
        /// <summary>The <see cref="UnityEngine.GameObject"/> containing
        /// the <see cref="UnityEngine.UI.Button"/> that will show <see cref="targetCanvas"/>.
        /// </summary>
        public GameObject openButtonObject;
        /// <summary>The <see cref="UnityEngine.UI.Button"/> attached to <see cref="openButtonObject"/>.</summary>
        private Button closeButton;
        /// <summary>The <see cref="UnityEngine.UI.Button"/> attached to <see cref="openButtonObject"/>.</summary>
        private Button openButton;
        /// <summary>Adds onClick listeners to <see cref="openButton"/> and <see cref="closeButton"/>.</summary>
        void Start()
        {
            openButton = openButtonObject.GetComponent<Button>();
            openButton.onClick.AddListener(OpenHandler);
            closeButton = closeButtonObject.GetComponent<Button>();
            closeButton.onClick.AddListener(CloseHandler);
            targetCanvas.SetActive(false);
            closeButtonObject.SetActive(false);
        }
        /// <summary>
        /// Activates <see cref="targetCanvas"/>, deactivates <see cref="openButtonObject"/> and activates <see cref="closeButtonObject"/>.
        /// </summary> 
        public void OpenHandler()
        {
            targetCanvas.SetActive(true);
            closeButtonObject.SetActive(true);
        }
        /// <summary>
        /// Deactivates <see cref="targetCanvas"/>, activates <see cref="openButtonObject"/> and deactivates <see cref="closeButtonObject"/>.
        /// </summary> 
        public void CloseHandler()
        {
            targetCanvas.SetActive(false);
            closeButtonObject.SetActive(false);
        }
    }
}

