using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deleter : MonoBehaviour
{
    private ResultPrinter rP;

    [SerializeField] private LayerMask vehicleLayerMask;

    private void Start()
    {
        rP = GameObject.FindWithTag("printerTag").GetComponent<ResultPrinter>();
    }
    
    private void OnTriggerEnter(Collider colObject)
    {
        Transform other = colObject.transform;
        GameObject rootObject = other.root.gameObject;

        // Check if object's layer is in the vehicleLayerMask
        if (((1 << rootObject.layer) & vehicleLayerMask.value) != 0)
        {
            rootObject.SetActive(false);
            Debug.Log(rootObject.name + " has been deleted");
        }
    }
}