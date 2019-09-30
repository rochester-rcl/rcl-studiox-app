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
        
        private Button closeButton;
        private Button openButton;
        // Start is called before the first frame update
        void Start()
        {
            openButton = openButtonObject.GetComponent<Button>();
            openButton.onClick.AddListener(OpenHandler);
            closeButton = closeButtonObject.GetComponent<Button>();
            closeButton.onClick.AddListener(CloseHandler);
            targetCanvas.SetActive(false);
            closeButtonObject.SetActive(false);
        }

        public void OpenHandler()
        {
            targetCanvas.SetActive(true);
            closeButtonObject.SetActive(true);
        }

        public void CloseHandler()
        {
            targetCanvas.SetActive(false);
            closeButtonObject.SetActive(false);
        }
    }
}

