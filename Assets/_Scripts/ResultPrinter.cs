using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResultPrinter : MonoBehaviour
{
    public CarSpawner cS;
    public UIManager UIM;

    [Space]

    public GameObject[] aVehicles;
    public bool[] areCarsParked;
    private bool[] hasBeenCounted;
    public enum ParkingResult { Unchecked, Success, Fail }
    private ParkingResult[] parkingResults;


    [Space]

    [SerializeField] private Vector3[] currentPositions;
    [SerializeField] private Vector3[] priorPositions;

    [SerializeField] private Quaternion[] currentRotations, priorRotations;

    private float parkedTimer;
    private bool allCarsParked = false;

    private int failInt, successInt;

    public List<GameObject> bVehicleList;

    private bool firstAll, firstFirst, hasCountedFailures;

    void Awake()
    {
        StartDebugCheckLoop();
    }

    public void StartDebugCheckLoop()
    {
        successInt = 0;
        failInt = 0;
        hasCountedFailures = false; // Reset the flag
        firstAll = false;
        firstFirst = false;

        StartCoroutine(DebugCheckAllParked());
    }


    void Update()
    {
        /*if (!allCarsParked)
        {
            countHowManyAreParkedFunction();
            checkIfParkedFunction();
        }*/
        bVehicleList.Clear();
        bVehicleList.AddRange(GameObject.FindGameObjectsWithTag("Bus"));
        bVehicleList.AddRange(GameObject.FindGameObjectsWithTag("Truck"));

        countHowManyAreParkedFunction();
        checkIfParkedFunction();

        if (cS.firstCarHasSpawned)
        {
            if (!firstFirst)
            {
                Debug.Log("FirstVehicleSpawned = true");
                firstFirst = true;
            }
        }

        if (cS.allVehiclesSpawned)
        {
            if (!firstAll)
            {
                Debug.Log("AllVehiclesSpawned = true");
                firstAll = true;
            }
        }

    }

    IEnumerator DebugCheckAllParked()
    {
        if (AllCarsParked())
        {
            Debug.Log("AllCarsParked = true");

            yield return new WaitForSeconds(1f);

            StartCoroutine(DebugCheckAllParked());
        }
    }

    public void addVehicleToaVehicles(GameObject gO)
    {
        if (!gO.CompareTag("Car")) return;

        int oldLength = aVehicles != null ? aVehicles.Length : 0;

        List<GameObject> vehicleList = new List<GameObject>(aVehicles ?? new GameObject[0]);
        vehicleList.Add(gO);
        aVehicles = vehicleList.ToArray();

        int newLength = aVehicles.Length;

        System.Array.Resize(ref areCarsParked, newLength);
        System.Array.Resize(ref hasBeenCounted, newLength);
        System.Array.Resize(ref priorPositions, newLength);
        System.Array.Resize(ref currentPositions, newLength);
        System.Array.Resize(ref priorRotations, newLength);
        System.Array.Resize(ref currentRotations, newLength);
        System.Array.Resize(ref parkingResults, newLength);

        // Initialize only the new element (at the last index)
        int i = newLength - 1;
        areCarsParked[i] = false;
        hasBeenCounted[i] = false;
        priorPositions[i] = gO.transform.position;
        currentPositions[i] = gO.transform.position;
        priorRotations[i] = gO.transform.rotation;
        currentRotations[i] = gO.transform.rotation;
        parkingResults[i] = ParkingResult.Unchecked;
    }


    public void countHowManyAreParkedFunction()
    {
        for (int i = 0; i < aVehicles.Length; i++)
        {
            if (aVehicles[i] == null || parkingResults[i] != ParkingResult.Unchecked)
                continue;

            priorPositions[i] = currentPositions[i];
            currentPositions[i] = aVehicles[i].transform.position;

            priorRotations[i] = currentRotations[i];
            currentRotations[i] = aVehicles[i].transform.rotation;

            if (priorPositions[i] == currentPositions[i] &&
                priorRotations[i] == currentRotations[i])
            {
                areCarsParked[i] = true;

                if (!hasBeenCounted[i])
                {
                    hasBeenCounted[i] = true;
                    parkingResults[i] = ParkingResult.Success;
                    successInt++;
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
            if (cS.firstCarHasSpawned)
            {
                allCarsParked = true;
                Time.timeScale = 0f;

                if (cS.allVehiclesSpawned && !hasCountedFailures)
                {
                    CountFailures();
                    hasCountedFailures = true;

                    Debug.Log("Gone to Finished Sim");
                    UIM.FinishedSim(successInt, failInt, true);
                }
            }
        }
    }


    bool AllCarsParked()
    {
        if (bVehicleList.Count <= 0 && cS.allVehiclesSpawned)
        {
            for (int i = 0; i < areCarsParked.Length; i++)
            {
                if (!areCarsParked[i])
                {
                    return false;
                }
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    public void CountFailures()
    {
        failInt++;
    }


    public int GetTotalParked()
    {
        int count = 0;
        foreach (bool parked in areCarsParked)
        {
            if (parked) count++;
        }
        return count;
    }

}
