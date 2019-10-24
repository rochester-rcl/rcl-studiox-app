using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RenderHeads.Media.AVProVideo;
namespace StudioX
{
    public class Dynamic360VideoLoader : MonoBehaviour
    {
        public GameObject sphere;
        private GameObject CurrentObject { get; set; }
        private AssetBundleMenuManager MenuManager { get; set; }
        private ApplyToMesh meshApplier;
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

        public void AddPlayerToSphere()
        {
            MediaPlayer player = FindObjectOfType<MediaPlayer>();
            meshApplier.Player = player;
        }
    }
}
