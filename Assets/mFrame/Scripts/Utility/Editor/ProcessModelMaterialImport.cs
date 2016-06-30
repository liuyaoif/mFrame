using UnityEngine;
using System.Collections;
using UnityEditor;

public class ProcessModelMaterialImport : AssetPostprocessor
{
    public bool importMaterilFlag = false;

    void OnPreprocessModel()
    {
        if (assetPath.Contains("FBX"))
        {
            //var modelImporter : ModelImporter = assetImporter;
            Debug.Log(assetPath);
            ModelImporter modelImporter = (ModelImporter)assetImporter;

            modelImporter.importMaterials = importMaterilFlag;

            //			modelImporter.materialSearch = ModelImporterMaterialSearch.Local;
            //
            //			string[] array = assetPath.Split('/');
            //			string fbxName = array[array.Length - 1];
            //			array = fbxName.Split('.');
            //			fbxName = array[0];
            //
            //			modelImporter.materialName = ModelImporterMaterialName.BasedOnMaterialName;

            //modelImporter.name =
        }
    }
}
