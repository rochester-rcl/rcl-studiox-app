﻿using System.Collections;
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
        public GameObject menu;
        public GameObject scrollView;
        public GameObject toggleButton;
        public string uiAssetPrefix = "ui_";
        public GameObject CurrentMesh { get; set; }
        private int currentMenuId;
        private static readonly object managerLock = new object();
        private static AssetBundleMenuManager _instance;
        private AppManager Manager { get; set; }
        private Transform scrollViewContainer;
        private Button _toggleButton;
        private bool loadingCurrentMesh;
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
            InitMenu();
            InitToggleButton();
            StartCoroutine(InitCells());
        }

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

        private string[] FindUIAssets(AssetBundle bundle)
        {
            string[] names = bundle.GetAllAssetNames();
            return Array.FindAll(names, n => n.Contains(uiAssetPrefix));
        }

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
        private void HandleMenuItemClick(string bundleName, string assetName)
        {
            // Prevent people from continually tapping on the button multiple times and starting a bunch of coroutines
            if (!loadingCurrentMesh)
            {
                StartCoroutine(LoadCurrentGameObjectFromBundle(bundleName, assetName));
            }
        }

        private IEnumerator LoadCurrentGameObjectFromBundle(string bundleName, string assetName)
        {
            AssetBundle bundle = Bundles.Find(b => b.name == bundleName);
            if (bundle)
            {
                loadingCurrentMesh = true;
                AssetBundleRequest req = bundle.LoadAssetAsync(assetName);
                yield return req;
                if (req.asset)
                {
                    if (CurrentMesh && CurrentMesh.name != assetName)
                    {
                        Destroy(CurrentMesh);
                    }
                    CurrentMesh = Instantiate(req.asset as GameObject, new Vector3(0, 0, 0), Quaternion.identity);
                    loadingCurrentMesh = false;
                    ToggleMenu();
                }
            }
        }

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

        private void InitMenu()
        {
            CheckMenuForCanvas(ref menu);
            scrollViewContainer = scrollView.transform.Find("Viewport/Content");
            if (!scrollViewContainer)
            {
                throw new MissingComponentException(string.Format("A ScrollView Component is required on {0}", scrollView.name));
            }
        }

        private void CheckMenuForCanvas(ref GameObject go)
        {
            if (!go.GetComponent<Canvas>())
            {
                throw new MissingComponentException(string.Format("A Canvas Component is required on menu {0}", go.name));
            }
        }
    }
}
