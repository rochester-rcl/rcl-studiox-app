using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StudioX
{
    public class AssetBundleMenuManager : MonoBehaviour
    {
        public AssetBundle[] Bundles { get; set; }
        public GameObject menuPortrait;
        public GameObject menuLandscape;
        private int currentMenuId;
        public enum MenuOrientation { Lanscape, Portrait };
        private static readonly object managerLock = new object();
        private static AssetBundleMenuManager _instance;
        private List<GameObject> cells;
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
            InitMenuLandscape();
            InitMenuPortrait();
        }

        private IEnumerator InitCells()
        {
            foreach(AssetBundle bundle in Bundles)
            {

            }
        }

        private IEnumerator PopulateCellData(ref AssetBundle bundle)
        {
            Sprite displayImage bundle.LoadAsset<Sprite>("Assets/") 
        }

        // We may be able to get away with the same layout if we use a GridLayout ... 
        private void InitMenuPortrait()
        {
            CheckMenuForCanvas(ref menuPortrait);
            GridLayout layout = menuPortrait.AddComponent<GridLayout>();

        }

        private void InitMenuLandscape()
        {
            CheckMenuForCanvas(ref menuPortrait);
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
