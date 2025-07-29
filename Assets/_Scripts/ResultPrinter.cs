using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResultPrinter : MonoBehaviour
{
    public CarSpawner cS;

    [Space]

    public GameObject[] aVehicles;
    public bool[] areCarsParked;
    private bool[] hasBeenCounted;

    [Space]

    [SerializeField] private Vector3[] currentPositions;
    [SerializeField] private Vector3[] priorPositions;

    [SerializeField] private Quaternion[] currentRotations, priorRotations;

    private float parkedTimer;
    private bool allCarsParked = false;

    private int failInt, successInt;

    void Update()
    {
        if (!allCarsParked)
        {
            countHowManyAreParkedFunction();
            checkIfParkedFunction();
        }
    }

    public void addVehicleToaVehicles(GameObject gO)
    {
        List<GameObject> vehicleList = new List<GameObject>(aVehicles);
    
        if (gO.CompareTag("Car"))
        {
            vehicleList.Add(gO);
        }

        aVehicles = vehicleList.ToArray();

        int count = aVehicles.Length;

        areCarsParked = new bool[count];
        hasBeenCounted = new bool[count];
        priorPositions = new Vector3[count];
        currentPositions = new Vector3[count];
        priorRotations = new Quaternion[count];
        currentRotations = new Quaternion[count];

        for (int i = 0; i < count; i++)
        {
            currentPositions[i] = priorPositions[i] = aVehicles[i].transform.position;
            currentRotations[i] = priorRotations[i] = aVehicles[i].transform.rotation;
            areCarsParked[i] = false;
        }
    }

    void countHowManyAreParkedFunction()
    {
        for (int i = 0; i < aVehicles.Length; i++)
        {
            priorPositions[i] = currentPositions[i];
            currentPositions[i] = aVehicles[i].transform.position;

            priorRotations[i] = currentRotations[i];
            currentRotations[i] = aVehicles[i].transform.rotation;

            if (priorPositions[i] == currentPositions[i] && priorRotations[i] == currentRotations[i])
            {
                areCarsParked[i] = true;

                if (!hasBeenCounted[i])
                {
                    successInt++;
                    hasBeenCounted[i] = true;
                    Debug.Log($"Vehicle #{i} has successfully parked. Total: {successInt}");
                }
            }
            else
            {
                areCarsParked[i] = false;
            }
        }
    }

    void checkIfParkedFunction()
    {
        if (AllCarsParked())
        {
            allCarsParked = true;
            Debug.Log("All cars are parked!");
            // You could start parkedTimer here
        }
    }

    bool AllCarsParked()
    {
        for (int i = 0; i < areCarsParked.Length; i++)
        {
            if (!areCarsParked[i])
                return false;
        }
        return true;
    }
}
