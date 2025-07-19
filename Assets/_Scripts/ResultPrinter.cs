using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResultPrinter : MonoBehaviour
{
    // Objectives: get number of cars parked and time for all cars to be parked
    public CarSpawner cS;

    [Space]

    public GameObject[] aVehicles;
    public bool[] areCarsParked;

    [Space] 

    // SerializeField keeps it private but it is still shown in inspector
    [SerializeField] private Vector3[] currentPositions;
    [SerializeField] private Vector3[] priorPositions;

    [SerializeField] private Quaternion[] currentRotations, priorRotations;

    private float parkedTimer;
    private bool allCarsParked = false;

    void Start()
    {
        //currently nothing
    }

    void Update()
    {
        countHowManyAreParkedFunction();
        checkIfParkedFunction();
    }

    public void addVehicleToaVehicles(GameObject gO)
    {
        // Convert existing array to list
        List<GameObject> vehicleList = new List<GameObject>(aVehicles);
        vehicleList.Add(gO);
        aVehicles = vehicleList.ToArray();

        int count = aVehicles.Length;

        areCarsParked = new bool[count];
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
        // if position and rotation didnt change, then one value in the array is true
        for (int i = 0; i < aVehicles.Length; i++)
        {
            // Update positions and rotations
            priorPositions[i] = currentPositions[i];
            currentPositions[i] = aVehicles[i].transform.position;

            priorRotations[i] = currentRotations[i];
            currentRotations[i] = aVehicles[i].transform.rotation;

            // If unchanged, mark as parked
            if (priorPositions[i] == currentPositions[i] && priorRotations[i] == currentRotations[i])
            {
                areCarsParked[i] = true;
            }
            else
            {
                areCarsParked[i] = false;
            }
        }
    }

    void checkIfParkedFunction()
    {
        /*if (AllCarsParked())
        {
            
        }*/
    }
}
