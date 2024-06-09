#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace AquasEvo
{
    public class AE_MenuItems
    {
        [MenuItem("GameObject/AQUAS EVO/Ocean")]
        static void AddOceanObjToScene()
        {
            if (AE_ObjectUtils.CheckForOceanDuplicates())
                EditorUtility.DisplayDialog("Ocean Object Detected!", "An ocean object is already in the scene. Multiple ocean objects are not allowed.", "OK");
            else
                AE_ObjectUtils.AddOceanController("AE Ocean");
        }
    }
}
#endif