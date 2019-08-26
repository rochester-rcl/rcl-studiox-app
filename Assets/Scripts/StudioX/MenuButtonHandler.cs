using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StudioX
{
    public class MenuButtonHandler : MonoBehaviour
    {
        public Button MenuButton { get; set; }
        public string sceneName;
        private AppManager Manager { get; set; }
        public void Start()
        {
            MenuButton = GetComponent<Button>() as Button;
            Manager = AppManager.GetManager();
            if (!Manager)
            {
                Debug.LogError("There must be an instance of AppManager attached to a GameObject in the scene");
            }
            if (MenuButton)
            {
                MenuButton.onClick.AddListener(ClickHandler);
            }
            else
            {
                Debug.LogError("MenuButtonHandler must be attached to a Button component!");
            }
        }
        public void ClickHandler()
        {
            Debug.Log(sceneName);
            if (Manager)
            {
                Manager.LoadScene(sceneName);
            }
        }
    }
}

