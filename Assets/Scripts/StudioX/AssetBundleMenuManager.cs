using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEditor;
namespace StudioX
{
    public class AssetBundleMenuManager : MonoBehaviour
    {
        public List<AssetBundle> Bundles { get; set; }
        public GameObject menuPortrait;
        public GameObject menuLandscape;
        public GameObject scrollViewPortrait;
        public GameObject scrollViewLandscape;
        public GameObject toggleButton;
        public string uiAssetPrefix = "ui_";
        public GameObject CurrentMesh { get; set; }
        private int currentMenuId;
        public enum MenuOrientation { Lanscape, Portrait };
        private static readonly object managerLock = new object();
        private static AssetBundleMenuManager _instance;
        private AppManager Manager { get; set; }
        private Transform scrollViewPortraitContainer;
        private Transform scrollViewLandscapeContainer;
        private Button _toggleButton;
        ///<summary>Static thread-safe singleton instance of AssetBundleMenuManager</summary>
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

        public void Start()
        {
            Manager = AppManager.GetManager();
            if (!Manager)
            {
                Debug.LogError("There must be an instance of AppManager attached to a GameObject in the scene");
            }
            else
            {
                Bundles = Manager.ARBundles;
            }
            InitMenuLandscape();
            InitMenuPortrait();
            StartCoroutine(InitCells());
        }

        public void OnDisable()
        {
            foreach (AssetBundle bundle in Bundles)
            {
                bundle.Unload(true);
            }
            _toggleButton.onClick.RemoveListener(ToggleMenu);
        }
        public void ToggleMenu()
        {
            GameObject menu = GetActiveMenu();
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
                            AddGameObjectToMenu(true, req.asset as GameObject, b.name);
                            AddGameObjectToMenu(false, req.asset as GameObject, b.name);
                        }
                    }
                }
            }
            yield return null;
        }

        private string[] FindUIAssets(AssetBundle bundle)
        {
            string[] names = bundle.GetAllAssetNames();
            return Array.FindAll(names, n => n.Contains(uiAssetPrefix));
        }

        private void AddGameObjectToMenu(bool isLandscape, GameObject go, string bundleName)
        {
            GameObject cloned = Instantiate(go, new Vector3(0, 0, 0), Quaternion.identity);
            AssetBundleNameSelector selector = cloned.GetComponent<AssetBundleNameSelector>();
            if (selector)
            {
                selector.SetCallback((string assetName) => { HandleMenuItemClick(bundleName, assetName); });
            }
            if (isLandscape)
            {
                cloned.transform.SetParent(scrollViewLandscapeContainer);
            }
            else
            {
                cloned.transform.SetParent(scrollViewPortraitContainer);
            }
        }

        private void HandleMenuItemClick(string bundleName, string assetName)
        {
            StartCoroutine(LoadCurrentGameObjectFromBundle(bundleName, assetName));
        }

        private IEnumerator LoadCurrentGameObjectFromBundle(string bundleName, string assetName)
        {
            AssetBundle bundle = Bundles.Find(b => b.name == bundleName);
            if (bundle)
            {
                AssetBundleRequest req = bundle.LoadAssetAsync(assetName);
                yield return req;
                if (req.asset)
                {
                    if (CurrentMesh.name != assetName)
                    {
                        Destroy(CurrentMesh);
                    }
                    CurrentMesh = Instantiate(req.asset as GameObject, new Vector3(0, 0, 0), Quaternion.identity);
                    ToggleMenu();
                }
            }
        }

        private void InitToggleButton()
        {
            _toggleButton = toggleButton.GetComponent<Button>();
            if (!toggleButton)
            {
                throw new MissingComponentException(string.Format("A Button Component is required on {0}", toggleButton.name));
            }
            else
            {
                _toggleButton.onClick.AddListener(ToggleMenu);
            }
        }
        private void InitMenuPortrait()
        {
            CheckMenuForCanvas(ref menuPortrait);
            scrollViewPortraitContainer = scrollViewPortrait.transform.Find("Viewport/Content");
            if (!scrollViewPortraitContainer)
            {
                throw new MissingComponentException(string.Format("A ScrollView Component is required on {0}", scrollViewPortrait.name));
            }
        }

        private void InitMenuLandscape()
        {
            CheckMenuForCanvas(ref menuLandscape);
            scrollViewLandscapeContainer = scrollViewLandscape.transform.Find("Viewport/Content");
            if (!scrollViewLandscapeContainer)
            {
                throw new MissingComponentException(string.Format("A ScrollView Component is required on {0}", scrollViewLandscape.name));
            }
        }

        private void CheckMenuForCanvas(ref GameObject go)
        {
            if (!go.GetComponent<Canvas>())
            {
                throw new MissingComponentException(string.Format("A Canvas Component is required on menu {0}", go.name));
            }
        }

        private int GetActiveMenuId()
        {
            return GetActiveMenu().GetInstanceID();
        }

        private ref GameObject GetActiveMenu()
        {
            if (menuPortrait.activeSelf)
            {
                return ref menuPortrait;
            }
            return ref menuLandscape;
        }
    }
}
