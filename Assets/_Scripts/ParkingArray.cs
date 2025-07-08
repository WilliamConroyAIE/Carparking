using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkingArray : MonoBehaviour
{
    public int parkingSpotTotal;

    /*[Header("Prefabs")]
    public GameObject carParkingSpot;
    public GameObject disabledParkingSpot;*/

    [Header("Lists")]
    public List<Transform> parkingWaypoints;
    public List<GameObject> availableStandard;
    public List<GameObject> availableDisabled;
    
    // Start is called before the first frame update
    void Awake()
    {
        GameObject[] sParks = GameObject.FindGameObjectsWithTag("standardTag");
        foreach(GameObject s in sParks)
            availableStandard.Add(s);


        GameObject[] dParks = GameObject.FindGameObjectsWithTag("disabledTag");
        foreach(GameObject d in dParks)
            availableDisabled.Add(d);

        /*GameObject[] tWaypoints = GameObject.FindGameObjectsWithTag("waypointTag");
        foreach(GameObject t in tWaypoints)
            parkingWaypoints.Add(t.transform);*/


        parkingSpotTotal = (availableStandard.Count) + (availableDisabled.Count);
    }

}
