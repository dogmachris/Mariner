using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AquasEvo
{
    public class AE_ObjectUtils : MonoBehaviour
    {
        /// <summary>
        /// Checks if Ocean objects are already in the scene
        /// </summary>
        /// <returns></returns>
        public static bool CheckForOceanDuplicates()
        {
            if (FindObjectOfType<AE_OceanController>() != null) return true;

            //Check if disabled ocean object is in the scene
            AE_OceanController[] oceanControllers = AE_OceanController.FindObjectsOfType<AE_OceanController>();
            AE_OceanController disabledComponent = oceanControllers.FirstOrDefault(component => !component.gameObject.activeInHierarchy);
            if (disabledComponent != null) return true;

            return false;
        }

        /// <summary>
        /// Adds an AQUAS ocean Controller to the scene
        /// Also add all objects that are associated with the ocean controller and make sure to have them hold a reference to each other.
        /// </summary>
        public static void AddOceanController(string name)
        {
            GameObject aquasOceanController = new GameObject(name);
            aquasOceanController.AddComponent<AE_OceanController>();

            /*GameObject oceanObj = AddOceanObject(name + " Mesh Object");
            aquasOceanController.GetComponent<AE_OceanController>().m_oceanObj = oceanObj;

            GameObject localOceanObj = AddLocalOceanObject(name + " Local Mesh Object");
            aquasOceanController.GetComponent<AE_OceanController>().m_localOceanObj = localOceanObj;

            GameObject projectorObj = AddProjectorObject(name + " Grid Projector");
            aquasOceanController.GetComponent<AE_OceanController>().m_projectorObj = projectorObj;

            if (Application.isPlaying) aquasOceanController.GetComponent<AE_OceanController>().m_oceanMaterial = oceanObj.GetComponent<MeshRenderer>().material;
            else aquasOceanController.GetComponent<AE_OceanController>().m_oceanMaterial = oceanObj.GetComponent<MeshRenderer>().sharedMaterial;*/
        }
    }
}