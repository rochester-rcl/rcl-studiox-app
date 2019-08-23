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
        public void Start()
        {
            MenuButton = GetComponent<Button>();
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
        }
    }
}

