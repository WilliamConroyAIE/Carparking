using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    #region Variables

    public LayerMask Vehicle;

    [Header("Arrays and Prefabs")]
    public NewCar[] carPrefabs;
    public NewTruck[] truckPrefabs;
    public NewBus[] busPrefabs;

    [Header("VehicleList")]
    public List<GameObject> vehicles;

    public Transform spawnPoint;

    public List<int> totalVInt;
    private List<GameObject> standardCars, disabledCars;

    public int totalVechiles, totalSCars, totalDCars, totalBuses, totalTrucks;

    private int totalPInt, removalInt;

    private int taxiNo, sedanNo, suvNo, vanNo, uteNo, sportNo, rozzasNo, busNo, boxTruckNo, fluidTruckNo;

    private bool taBool, seBool, suBool, vaBool, utBool, spBool, roBool, buBool, boBool, flBool, allBool;

    private bool isSpawning = false;
    private bool spawnAllowed = true;

    private GameObject vehiclePrefab = null;

    private ParkingArray pA;

    [Header("Timer")]
    public float simulationTimer = 0f;
    private bool timerRunning = false;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        totalVInt = new List<int>();
        standardCars = new List<GameObject>();
        disabledCars = new List<GameObject>();
        pA = GameObject.FindWithTag("parkingTag").GetComponent<ParkingArray>();
    }

    private void Update()
    {
        if (timerRunning)
        {
            simulationTimer += Time.deltaTime;
            if (vehicles.Count == 0)
                timerRunning = false;
        }
    }

    #endregion

    #region Spawn Control

    public void StartTestRun(int taNo, int seNo, int suNo, int vaNo, int utNo, int spNo, int roNo, int buNo, int boNo, int flNo)
    {
        totalVInt.AddRange(new int[] { taNo, seNo, suNo, vaNo, utNo, spNo, roNo, buNo, boNo, flNo });

        taxiNo = taNo; sedanNo = seNo; suvNo = suNo; vanNo = vaNo; uteNo = utNo;
        sportNo = spNo; rozzasNo = roNo; busNo = buNo; boxTruckNo = boNo; fluidTruckNo = flNo;

        totalPInt = GameObject.FindWithTag("parkingTag").GetComponent<ParkingArray>().parkingSpotTotal;

        GameObject[] cars = GameObject.FindGameObjectsWithTag("Car");
        foreach (GameObject car in cars)
        {
            Car c = car.GetComponent<Car>();
            if (c.disabled) disabledCars.Add(car);
            else standardCars.Add(car);
        }

        totalSCars = standardCars.Count;
        totalDCars = disabledCars.Count;
        totalBuses = busNo;
        totalTrucks = boxTruckNo + fluidTruckNo;

        if (totalVInt.Count <= totalPInt)
        {
            taBool = taNo >= 1; seBool = seNo >= 1; suBool = suNo >= 1;
            vaBool = vaNo >= 1; utBool = utNo >= 1; spBool = spNo >= 1;
            roBool = roNo >= 1; buBool = buNo >= 1; boBool = boNo >= 1; flBool = flNo >= 1;

            allBool = taBool && seBool && suBool && vaBool && utBool && spBool && roBool && buBool && boBool && flBool;

            StartCoroutine(SpawnCars());
        }
        else
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    public void AllowNextSpawn()
    {
        spawnAllowed = true;
    }

    private IEnumerator SpawnCars()
    {
        if (isSpawning) yield break;
        isSpawning = true;

        int remainingVehicles = taxiNo + sedanNo + suvNo + vanNo + uteNo + sportNo + rozzasNo + busNo + boxTruckNo + fluidTruckNo;

        while (remainingVehicles > 0)
        {
            List<int> availableTypes = new List<int>();
            if (taxiNo > 0) availableTypes.Add(0);
            if (sedanNo > 0) availableTypes.Add(1);
            if (suvNo > 0) availableTypes.Add(2);
            if (vanNo > 0) availableTypes.Add(3);
            if (uteNo > 0) availableTypes.Add(4);
            if (sportNo > 0) availableTypes.Add(5);
            if (rozzasNo > 0) availableTypes.Add(6);
            if (busNo > 0) availableTypes.Add(7);
            if (boxTruckNo > 0) availableTypes.Add(8);
            if (fluidTruckNo > 0) availableTypes.Add(9);

            if (availableTypes.Count == 0)
                break;

            int chosenType = availableTypes[Random.Range(0, availableTypes.Count)];
            vehiclePrefab = null;

            switch (chosenType)
            {
                case 0: vehiclePrefab = carPrefabs[0].gameObject; removalInt = 0; break;
                case 1: vehiclePrefab = carPrefabs[1].gameObject; removalInt = 1; break;
                case 2: vehiclePrefab = carPrefabs[2].gameObject; removalInt = 2; break;
                case 3: vehiclePrefab = carPrefabs[3].gameObject; removalInt = 3; break;
                case 4: vehiclePrefab = carPrefabs[4].gameObject; removalInt = 4; break;
                case 5: vehiclePrefab = carPrefabs[5].gameObject; removalInt = 5; break;
                case 6: vehiclePrefab = carPrefabs[6].gameObject; removalInt = 6; break;
                case 7: vehiclePrefab = busPrefabs[0].gameObject; removalInt = 7; break;
                case 8: vehiclePrefab = truckPrefabs[0].gameObject; removalInt = 8; break;
                case 9: vehiclePrefab = truckPrefabs[1].gameObject; removalInt = 9; break;
            }

            // Wait until previous vehicle signals spawn ready
            while (!spawnAllowed)
                yield return null;

            GameObject nextVehicle = Instantiate(vehiclePrefab, spawnPoint.position, spawnPoint.rotation);
            vehicles.Add(nextVehicle);

            switch (removalInt)
            {
                case 0: taxiNo--; nextVehicle.GetComponent<NewCar>().StartDriving(pA.parkingWaypoints, pA, this); break;
                case 1: sedanNo--; nextVehicle.GetComponent<NewCar>().StartDriving(pA.parkingWaypoints, pA, this); break;
                case 2: suvNo--; nextVehicle.GetComponent<NewCar>().StartDriving(pA.parkingWaypoints, pA, this); break;
                case 3: vanNo--; nextVehicle.GetComponent<NewCar>().StartDriving(pA.parkingWaypoints, pA, this); break;
                case 4: uteNo--; nextVehicle.GetComponent<NewCar>().StartDriving(pA.parkingWaypoints, pA, this); break;
                case 5: sportNo--; nextVehicle.GetComponent<NewCar>().StartDriving(pA.parkingWaypoints, pA, this); break;
                case 6: rozzasNo--; nextVehicle.GetComponent<NewCar>().StartDriving(pA.parkingWaypoints, pA, this); break;
                case 7: busNo--; nextVehicle.GetComponent<NewBus>().StartDriving(pA.parkingWaypoints, pA, this); break;
                case 8: boxTruckNo--; nextVehicle.GetComponent<NewTruck>().StartDriving(pA.parkingWaypoints, pA, this); break;
                case 9: fluidTruckNo--; nextVehicle.GetComponent<NewTruck>().StartDriving(pA.parkingWaypoints, pA, this); break;
            }

            spawnAllowed = false;
            remainingVehicles--;
        }

        totalVInt.Clear();
        totalVechiles = 0;
        isSpawning = false;
    }

    #endregion

    #region Debug

    private void OnDrawGizmosSelected()
    {
        if (spawnPoint != null)
        {
            Gizmos.color = Color.red;
            Vector3 boxSize = new Vector3(3f, 3f, 6f);
            Gizmos.matrix = Matrix4x4.TRS(spawnPoint.position + Vector3.up * 1.5f, spawnPoint.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, boxSize);
        }
    }

    #endregion
}
