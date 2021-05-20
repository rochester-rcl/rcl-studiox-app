using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NativeFilePickerNamespace;
using AsImpL;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;
using TMPro;
public class UploadManager : MonoBehaviour
{
    private string path;
    private GameObject prefab;
    private string[] fileTypes;
    private ObjectImporter obj;
    private PathSettings pathSettings;
    private ImportOptions importOptions = new ImportOptions();
    public TextMeshProUGUI text;
    public GameObject raycast;
    void Start()
    {
        fileTypes = allowedFileTypes();
        obj = gameObject.GetComponent<ObjectImporter>();
        pathSettings = gameObject.GetComponent<PathSettings>();
        importOptions.modelScaling = 0.3f; //For debug now, seems to spawn big
        importOptions.localPosition = new Vector3(0, 0, 10f);//Spawns on camera, moving it forward
    }
    public void SelectModelFile()
    {
        //TODO add iphone functionality. Server sided conversion to glTF and imported on runtime instead of just obj importer.
        if (NativeFilePicker.IsFilePickerBusy()) //checiking if currently picking file
            return;
        NativeFilePicker.RequestPermission(); //asking permission if not given
        NativeFilePicker.Permission permission = NativeFilePicker.PickFile((dir) =>
       {
           if (dir == null)
               Debug.Log("Operation cancelled");
           else
               path = dir;
           
           if (ValidModelType(path)) //checking if its an obj file
           {
               obj.ImportModelAsync("Your Object", pathSettings.RootPath + path, null, importOptions); //import obj

               MeshControlMain mcm = raycast.GetComponent<MeshControlMain>();
               mcm.meshAdded = true;

           }
           else
           {
               Debug.LogError("Wrong file type/no file selected"); //if it isnt, log error
           }
       }, fileTypes);


    }
    public string[] allowedFileTypes()
    {
        string[] filetypes = { "*/*" }; //MIME for obj is a glitchy.
        return filetypes;
    }
    public bool ValidModelType(string path)
    {
        if (!path.EndsWith("obj"))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

}