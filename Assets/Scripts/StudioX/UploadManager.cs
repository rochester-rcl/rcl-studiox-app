using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NativeFilePickerNamespace;
using AsImpL;
public class UploadManager : MonoBehaviour
{
    private string path;
    private GameObject prefab;
    private string[] fileTypes;
    private ObjectImporter obj;
    private PathSettings pathSettings;
    private ImportOptions importOptions = new ImportOptions();
    void Start()
    {
        fileTypes = allowedFileTypes();
        obj = gameObject.GetComponent<ObjectImporter>();
        pathSettings = gameObject.GetComponent<PathSettings>();
    }
    public void LoadLocalModel()
    {
        Instantiate(prefab);


    }
    public void SelectModelFile()
    {
        if (NativeFilePicker.IsFilePickerBusy())
            return;

        NativeFilePicker.Permission permission = NativeFilePicker.PickFile((dir) =>
       {
           if (dir == null)
               Debug.Log("Operation cancelled");
           else
               path = dir;
       }, fileTypes);
       if(ValidModelType(path)){
            obj.ImportModelAsync("Your Object", pathSettings.RootPath + path, null, importOptions);
       }
       else{
           Debug.LogError("Wrong file type/no file selected");
       }
      

    }
    public string[] allowedFileTypes()
    {
        string[] filetypes = { "*/*" };
        return filetypes;
    }
    public bool ValidModelType(string path){
        if(!path.EndsWith("obj")){
            return false;
        }
        else{
            return true;
        }
    }
}
