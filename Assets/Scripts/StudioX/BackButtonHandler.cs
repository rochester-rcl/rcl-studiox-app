using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace StudioX
{
    public class BackButtonHandler : MonoBehaviour
    {
        // Start is called before the first frame update
        private AppManager manager;
        private Button button;
        void Start()
        {
            button = gameObject.GetComponent<Button>();
            manager = AppManager.GetManager();
            if (!button)
            {
                throw new MissingComponentException("BackButtonHandler Requires a Button Component");
            } 
            else 
            {
                button.onClick.AddListener(manager.LoadMenu);
            }
        }

        public void OnDestroy()
        {
            button.onClick.RemoveListener(manager.LoadMenu);
        }
    }
}

