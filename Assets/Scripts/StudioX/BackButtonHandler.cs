using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace StudioX
{
    /// <summary>A basic "back button" to be used with <see cref="AppManager"/>.</summary>
    [RequireComponent(typeof(Button))]
    public class BackButtonHandler : MonoBehaviour
    {
        /// <summary>The <see cref="AppManager"/> instance in the scene.</summary>
        private AppManager manager;
        /// <summary>
        /// The <see cref="UnityEngine.UI.Button"/> attached to <see cref="BackButtonHandler.gameObject"/>.
        /// </summary>
        private Button button;
        /// <summary>Initializes <see cref="button"/> and <see cref="manager"/> and adds 
        /// <see cref="AppManager.LoadMenu"/> as an onClick handler to <see cref="button"/>.
        /// </summary>
        void Start()
        {
            button = gameObject.GetComponent<Button>();
            manager = AppManager.GetManager();
            button.onClick.AddListener(manager.LoadMenu);
        }
        /// <summary>Removes <see cref="AppManager.LoadMenu"/> handler from <see cref="button"/>.</summary>
        public void OnDestroy()
        {
            button.onClick.RemoveListener(manager.LoadMenu);
        }
    }
}

