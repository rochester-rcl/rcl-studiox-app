using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace StudioX
{
    public class AssetBundleMenuManager : MonoBehaviour
    {
        public List<AssetBundle> Bundles { get; set; }
        public GameObject menuPortrait;
        public GameObject menuLandscape;
        public GameObject scrollViewPortrait;
        public GameObject scrollViewLandscape;
        private int currentMenuId;
        public enum MenuOrientation { Lanscape, Portrait };
        private static readonly object managerLock = new object();
        private static AssetBundleMenuManager _instance;
        private AppManager Manager { get; set; }
        private Transform scrollViewPortraitContainer;
        private Transform scrollViewLandscapeContainer;
        // TODO figure out how best to load display images from the bundle as well as their names

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
        // TODO add animated loader while this runs
        private IEnumerator InitCells()
        {
            GameObject[] localAssets;
            if (Bundles.Count > 0)
            {
                foreach (AssetBundle b in Bundles)
                {
                    AssetBundleRequest req = b.LoadAllAssetsAsync<GameObject>();
                    yield return req;
                    localAssets = req.allAssets as GameObject[];
                    foreach (GameObject asset in localAssets)
                    {
                        AddGameObjectToMenu(true, asset);
                        AddGameObjectToMenu(false, asset);
                    }
                }
            }
            yield return null;
        }

        private void AddGameObjectToMenu(bool isLandscape, GameObject go)
        {
            Instantiate(go, new Vector3(0, 0, 0), Quaternion.identity);
            if (isLandscape)
            {
                go.transform.parent = scrollViewLandscapeContainer;
            }
            else
            {
                go.transform.parent = scrollViewPortraitContainer;
            }
        }

        // We may be able to get away with the same layout if we use a GridLayout ... 
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
