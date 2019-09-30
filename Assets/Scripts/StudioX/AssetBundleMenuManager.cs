using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEditor;
namespace StudioX
{
    /// <summary>Allows for dynamically loading content from Asset Bundles through a UI menu.</summary>
    public class AssetBundleMenuManager : MonoBehaviour
    {
        /// <summary>The List of <see cref="UnityEngine.AssetBundle"/>s available in the scene.</summary>
        public List<AssetBundle> Bundles { get; set; }
        /// <summary>The menu to be displayed for selecting content from AssetBundles.</summary>
        public GameObject menu;
        // TODO Need to just go ahead and make this a required component 
        /// <summary>The GameObject containing the ScrollView Component.</summary> 
        public GameObject scrollView;
        /// <summary> The GameObject containing the Button used to toggle the menu.</summary>
        public GameObject toggleButton;
        /// <summary>The user-defined preset for extracting ui prefabs from an <see cref="UnityEngine.AssetBundle"/>.</summary>
        /// <para> This can be used to distinguish between ui elements and game elements. See README for how this is intended to work.</para>
        public string uiAssetPrefix = "ui_";
        /// <summary>Delegate for <see cref="OnPrefabLoaded" /> event.</summary>
        public delegate void PrefabLoaded(ref GameObject prefab);
        /// <summary>Event that is fired whenever a prefab is loaded from an <see cref="UnityEngine.AssetBundle" /></summary>
        public event PrefabLoaded OnPrefabLoaded;
        /// <summary> Lock used when setting and getting <see cref="AssetBundleMenuManager"/> singleton.</summary>
        private static readonly object managerLock = new object();
        /// <summary> Singleton <see cref="AssetBundleMenuManager" /> instance.</summary>
        private static AssetBundleMenuManager _instance;
        /// <summary> The <see cref="AppManager" /> instance currently loaded in the scene. </summary>
        private AppManager Manager { get; set; }
        /// <summary> The parent <see cref="UnityEngine.Transform"/> for <see cref="scrollView"/>.</summary>
        private Transform scrollViewContainer;
        /// <summary> The <see cref="UnityEngine.UI.Button"/> Component attached to <see cref="toggleButton"/>.</summary>
        private Button _toggleButton;
        // TODO maybe we could use a lock instead ... 
        /// <summary>Whether or not a prefab is currently being loaded from an <see cref="UnityEngine.AssetBundle"/>.</summary>
        private bool loadingCurrentPrefab;

        /// <summary>Static thread-safe singleton instance of AssetBundleMenuManager</summary>
        public static AssetBundleMenuManager Instance
        {
            get
            {
                lock (managerLock)
                {
                    return _instance;
                }
            }
        }
        /// <summary> Retrieves the active AssetBundleManager instance in the scene.</summary>
        /// <returns> <see cref="AssetBundleMenuManager"/> if it exists in the scene, otherwise null.</returns>
        public static AssetBundleMenuManager GetManager()
        {
            return GameObject.FindObjectOfType<AssetBundleMenuManager>();
        }
        /// <summary>Initializes <see cref="AssetBundleMenuManager"/> and ensures only one instance is present in the scene.</summary>
        /// <para>Note: if another <see cref="AssetBundleMenuManager"/> is already present in the scene, a warning will be logged and 
        /// the GameObject this instance is attached to will be destroyed.</para>
        public void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning("Another instance of AssetBundleMenuManager was found in the scene. Destroying the current instance.");
                Destroy(gameObject);
            }
            else
            {
                _instance = this;
            }
        }
        /// <summary>Initializes UI Components and attempts to set <see cref="Manager"/> and <see cref="Bundles"/>.</summary>
        /// <exception cref="UnityEngine.MissingComponentException">Will be thrown if no <see cref="AppManager"/> instance is present in the scene.</exception>
        public void Start()
        {
            Manager = AppManager.GetManager();
            if (!Manager)
            {
                throw new MissingComponentException("There must be an instance of AppManager attached to a GameObject in the scene");
            }
            else
            {
                Bundles = Manager.ARBundles;
            }
            InitMenu();
            InitToggleButton();
            StartCoroutine(InitCells());
        }
        /// <summary>Toggles the visibility of <see cref="menu"/>.</summary>
        public void ToggleMenu()
        {
            if (menu.activeSelf)
            {
                menu.SetActive(false);
                toggleButton.SetActive(true);
            }
            else
            {
                menu.SetActive(true);
                toggleButton.SetActive(false);
            }
        }
        // TODO add animated loader while this runs
        /// <summary>Populates <see cref="scrollView"/> with UI prefabs with prefix <see cref="uiPrefix"/> from <see cref="Bundles"/>.</summary>
        /// <para>See README for implementation details</para> 
        private IEnumerator InitCells()
        {
            if (Bundles.Count > 0)
            {
                string[] uiAssets;
                foreach (AssetBundle b in Bundles)
                {
                    uiAssets = FindUIAssets(b);
                    foreach (string asset in uiAssets)
                    {
                        AssetBundleRequest req = b.LoadAssetAsync<GameObject>(asset);
                        yield return req;
                        if (req.asset)
                        {
                            AddGameObjectToMenu(req.asset as GameObject, b.name);
                        }
                    }
                }
            }
            yield return null;
        }
        /// <summary>Searches <see cref="UnityEngine.AssetBundle"/> for assets with <see cref="uiPrefix"/>.</summary>
        /// <param name="bundle">The <see cref="UnityEngine.AssetBundle"/> to search.</param>
        /// <returns>An array of asset names, or an empty array.</returns>
        private string[] FindUIAssets(AssetBundle bundle)
        {
            string[] names = bundle.GetAllAssetNames();
            return Array.FindAll(names, n => n.Contains(uiAssetPrefix));
        }
        /// <summary> Adds a UI GameObject to <see cref="scrollViewContainer"/>.</summary>
        /// <param name="go">The GameObject to add to <see cref="scrollViewContainer"/>.</param>
        /// <param name="bundleName">The name of the <see cref="UnityEngine.AssetBundle"/> the GameObject is stored in.</param>
        /// <para>See README for implementation details.</para>
        private void AddGameObjectToMenu(GameObject go, string bundleName)
        {
            GameObject cloned = Instantiate(go, Vector3.zero, Quaternion.identity);
            AssetBundleNameSelector selector = cloned.GetComponent<AssetBundleNameSelector>();
            if (selector)
            {
                selector.SetCallback((string assetName) => { HandleMenuItemClick(bundleName, assetName); });
            }
            cloned.transform.SetParent(scrollViewContainer);
            cloned.transform.localScale = Vector3.one;
            cloned.transform.localRotation = Quaternion.identity;
        }

        // TODO replace scroll view with spinner while loading
        /// <summary>Callback method for UI menu item set in <see cref="AddGameObjectToMenu"/>.</summary>
        /// <para>When a menu item is clicked (or tapped) <see cref="assetName"/> is extracted from <see cref="bundleName"/>.
        /// Asynchronously calls <see cref="LoadGameObjectFromBundle"/> as a Coroutine behind the scenes.
        /// See README for implementation details.</para>
        /// <param name="bundleName">The name of the <see cref="UnityEngine.AssetBundle"/> that <see cref="assetName"/> belongs to.</param>
        /// <param name="assetName">The name of the prefab to be loaded from <see cref="bundleName"/>.</param>  
        private void HandleMenuItemClick(string bundleName, string assetName)
        {
            // Prevent people from continually tapping on the button multiple times and starting a bunch of coroutines
            if (!loadingCurrentPrefab)
            {
                StartCoroutine(LoadGameObjectFromBundle(bundleName, assetName));
            }
        }
        /// <summary>
        /// Loads a prefab with name <see cref="assetName"/> from an <see cref="UnityEngine.AssetBundle"/> 
        /// with name <see cref="bundleName"/>.
        /// </summary>
        /// <param name="bundleName">The name of the <see cref="UnityEngine.AssetBundle"/> that <see cref="assetName"/> belongs to.</param>
        /// <param name="assetName">The name of the prefab to be loaded from <see cref="bundleName"/>.</param>
        /// <remarks>Will fire <see cref="OnPrefabLoaded" /> if the asset is found.</remarks>
        /// <returns>IEnumerator to be used with StartCoroutine</returns>  
        private IEnumerator LoadGameObjectFromBundle(string bundleName, string assetName)
        {
            AssetBundle bundle = Bundles.Find(b => b.name == bundleName);
            if (bundle)
            {
                loadingCurrentPrefab = true;
                AssetBundleRequest req = bundle.LoadAssetAsync(assetName);
                yield return req;
                if (req.asset)
                {
                    GameObject go = req.asset as GameObject;
                    if (OnPrefabLoaded != null)
                    {
                        OnPrefabLoaded(ref go);
                    }
                    loadingCurrentPrefab = false;
                    ToggleMenu();
                }
            }
        }
        /// <summary>Initializes <see cref="_toggleButton" />.</summary>
        /// <exception cref="UnityEngine.MissingComponentException">Thrown if <see cref="UnityEngine.UI.Button"/> Component 
        /// is missing from <see cref="toggleButton"/> GameObject.</exception>
        private void InitToggleButton()
        {
            _toggleButton = toggleButton.GetComponent<Button>();
            if (!_toggleButton)
            {
                throw new MissingComponentException(string.Format("A Button Component is required on {0}", toggleButton.name));
            }
            else
            {
                _toggleButton.onClick.AddListener(ToggleMenu);
            }
        }
        /// <summary>Initializes <see cref="scrollViewContainer" />.</summary>
        /// <exception cref="UnityEngine.MissingComponentException">Thrown if <see cref="UnityEngine.ScrollView"/> Component 
        /// is missing from <see cref="scrollView"/> GameObject.</exception>
        private void InitMenu()
        {
            CheckMenuForCanvas(ref menu);
            scrollViewContainer = scrollView.transform.Find("Viewport/Content");
            if (!scrollViewContainer)
            {
                throw new MissingComponentException(string.Format("A ScrollView Component is required on {0}", scrollView.name));
            }
        }
        /// <summary>Checks GameObject <see cref="go"/> for a <see cref="UnityEngine.Canvas"/> Component.</summary>
        /// <exception cref="UnityEngine.MissingComponentException">Thrown if 
        /// <see cref="UnityEngine.Canvas"/> is missing from <see cref="go"/>.</exception>
        /// <param name="go">The <see cref="UnityEngine.GameObject"/> to check for a <see cref="UnityEngine.Canvas"/>.</param>
        private void CheckMenuForCanvas(ref GameObject go)
        {
            if (!go.GetComponent<Canvas>())
            {
                throw new MissingComponentException(string.Format("A Canvas Component is required on menu {0}", go.name));
            }
        }
    }
}
