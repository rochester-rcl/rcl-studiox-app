using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace StudioX
{
    public class CanvasToggleButtonHandler : MonoBehaviour
    {
        public GameObject targetCanvas;
        public GameObject closeButtonObject;
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

