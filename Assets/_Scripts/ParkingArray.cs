using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkingArray : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject carParkingSpot;
    public GameObject disabledParkingSpot;

    [Header("Lists")]
    public List<Transform> parkingWaypoints;
    public List<GameObject> availableStandard;
    public List<GameObject> availableDisabled;
    
    // Start is called before the first frame update
    void Start()
    {
        GameObject[] sParks = GameObject.FindGameObjectsWithTag("standardTag");
        foreach(GameObject s in sParks)
            availableStandard.Add(s);


        GameObject[] dParks = GameObject.FindGameObjectsWithTag("disabledTag");
        foreach(GameObject d in dParks)
            availableDisabled.Add(d);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
