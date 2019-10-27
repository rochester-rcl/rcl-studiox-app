using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RenderHeads.Media.AVProVideo;
using Vimeo.Player;
namespace StudioX
{
    public class Dynamic360VideoLoader : MonoBehaviour
    {
        public GameObject sphere;
        private GameObject CurrentObject { get; set; }
        public AssetBundleMenuManager MenuManager { get; set; }
        private ApplyToMesh meshApplier;
        private MediaPlayer player;
        private VimeoPlayer vimeoPlayer;
        private AssetBundleVimeoTokenStorage storage;
        public void Start()
        {
            MenuManager = AssetBundleMenuManager.GetManager();
            if (MenuManager)
            {
                MenuManager.OnPrefabLoaded += PlaceGameObject;
            }
            else
            {
                throw new MissingComponentException("An AssetBundleMenuManager instance is required in the current scene!");
            }
            meshApplier = sphere.GetComponent<ApplyToMesh>();
            if (!meshApplier)
            {
                throw new MissingComponentException("The Sphere GameObject must have an ApplyToMesh Component attached to it.");
            }
        }

        public void PlaceGameObject(ref GameObject go)
        {
            if (CurrentObject)
            {
                Destroy(CurrentObject);
            }
            CurrentObject = Instantiate(go, Vector3.zero, Quaternion.identity);
            AddPlayerToSphere();
        }

        private void UpdateSpherePlayerSource()
        {
            storage = FindObjectOfType<AssetBundleVimeoTokenStorage>();
            vimeoPlayer = FindObjectOfType<VimeoPlayer>();
            vimeoPlayer.autoPlay = false;
            vimeoPlayer.buildMode = true;
            vimeoPlayer.vimeoToken = storage.VimeoToken;
            vimeoPlayer.OnStart += () => {
                vimeoPlayer.LoadVideo(); 
            };
            vimeoPlayer.OnVideoMetadataLoad += () => { vimeoPlayer.autoPlay = true; vimeoPlayer.Play(); };
            player = FindObjectOfType<MediaPlayer>();
        }

        private void AddPlayerToSphere()
        {
            UpdateSpherePlayerSource();
            meshApplier.Player = player;
        }
    }
}
