using System.Collections.Generic;
using System.Diagnostics;
using TMPro; 
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    
    public GameObject cubeMassSpringPrefab;
    public GameObject sphereMassSpringPrefab;
    public GameObject capsuleMassSpringPrefab;
    public GameObject cylinderMassSpringPrefab; 

   
    public Transform contentPanel;
    
    public GameObject objectControlUIPrefab;

    
    public TMP_Dropdown objectTypeDropdown;

    
    private List<GameObject> activeMassSpringSystems = new List<GameObject>();
    
    private int objectCounter = 0;

    void Start()
    {
        
        objectCounter = FindObjectsOfType<MassSpringSystem>().Length;

        
        foreach (var existingSystem in FindObjectsOfType<MassSpringSystem>())
        {
            activeMassSpringSystems.Add(existingSystem.gameObject);
            CreateObjectControlUI(existingSystem);
        }

        
        if (objectTypeDropdown != null)
        {
            objectTypeDropdown.ClearOptions();
           
            List<string> options = new List<string> { "Cube", "Sphere", "Capsule", "Cylinder" };
            objectTypeDropdown.AddOptions(options);
        }

       
        if (CollisionManager.Instance == null)
        {
            GameObject collisionManagerGO = new GameObject("CollisionManager");
            collisionManagerGO.AddComponent<CollisionManager>();
            //Debug.Log("CollisionManager created and added to scene.");
        }
    }

    
    public void AddNewMassSpringSystem()
    {
        GameObject selectedPrefab = null;
        string objectNamePrefix = "MassSpringSystem_";

        if (objectTypeDropdown != null)
        {
            
            switch (objectTypeDropdown.value)
            {
                case 0:
                    selectedPrefab = cubeMassSpringPrefab;
                    objectNamePrefix = "Cube_";
                    break;
                case 1: 
                    selectedPrefab = sphereMassSpringPrefab;
                    objectNamePrefix = "Sphere_";
                    break;
                case 2: 
                    selectedPrefab = capsuleMassSpringPrefab;
                    objectNamePrefix = "Capsule_";
                    break;
                case 3: 
                    selectedPrefab = cylinderMassSpringPrefab;
                    objectNamePrefix = "Cylinder_";
                    break;
                default:
                    //Debug.LogError("Invalid object type selected in dropdown!");
                    return;
            }
        }
        else 
        {
            selectedPrefab = cubeMassSpringPrefab;
        }

        if (selectedPrefab == null)
        {
           // Debug.LogError("Selected Mass Spring System Prefab is not assigned in UIManager!");
            return;
        }

        
        GameObject newSystemGO = Instantiate(selectedPrefab);
        newSystemGO.name = objectNamePrefix + objectCounter++; 

        
        MassSpringSystem newSystem = newSystemGO.GetComponent<MassSpringSystem>();
        if (newSystem == null)
        {
            //Debug.LogError("Prefab does not have a MassSpringSystem component!", newSystemGO);
            Destroy(newSystemGO); 
            return;
        }

        activeMassSpringSystems.Add(newSystemGO); 

        
        CreateObjectControlUI(newSystem);
    }

    
    private void CreateObjectControlUI(MassSpringSystem system)
    {
        if (objectControlUIPrefab == null)
        {
           // Debug.LogError("Object Control UI Prefab is not assigned in UIManager!");
            return;
        }

        
        GameObject controlUI = Instantiate(objectControlUIPrefab, contentPanel);

        
        ObjectPropertyControl propertyControl = controlUI.GetComponent<ObjectPropertyControl>();
        if (propertyControl != null)
        {
            
            propertyControl.SetMassSpringSystem(system);
        }

        
        ObjectTransformControl transformControl = controlUI.GetComponent<ObjectTransformControl>();
        if (transformControl != null)
        {
            
            transformControl.SetTargetGameObject(system.gameObject);
        }
    }
}