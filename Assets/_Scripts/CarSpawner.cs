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

    [Space]

    [Header("VehicleList")]
    public List<GameObject> vehicles;

    [Space]

    public Transform spawnPoint;

    public List<int> totalVInt;
    private List<GameObject> standardCars, disabledCars;

    public int totalVechiles, totalSCars, totalDCars, totalBuses, totalTrucks;

    private int totalPInt, availableCarTypeInt, spawnableCarTypeInt, removalInt;

    private int taxiNo, sedanNo, suvNo, vanNo, uteNo, sportNo, rozzasNo, busNo, boxTruckNo, fluidTruckNo;

    private bool taBool, seBool, suBool, vaBool, utBool, spBool, roBool, buBool, boBool, flBool, allBool;

    private bool canSpawn;

    private GameObject vehiclePrefab = null;

    private ParkingArray pA;

    [Header("Timer")]
    public float simulationTimer = 0f;
    private bool timerRunning = false;

    #endregion


    #region Spawning

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
            {
                timerRunning = false;
                
            }
        }
    }

    public void StartTestRun(int taNo, int seNo, int suNo, int vaNo, int utNo, int spNo, int roNo, int buNo, int boNo, int flNo)
    {
        totalVInt.Add(taNo);
        totalVInt.Add(seNo);
        totalVInt.Add(suNo);
        totalVInt.Add(vaNo);
        totalVInt.Add(utNo);
        totalVInt.Add(spNo);
        totalVInt.Add(roNo);
        totalVInt.Add(buNo);
        totalVInt.Add(boNo);
        totalVInt.Add(flNo);
        
        //Changes to a universal int instead of a local int 
        // (local int is an int only usable in StartTestRun() by changing it it no works where in the script necessary)
        taxiNo = taNo;
        sedanNo = seNo;
        suvNo = suNo;
        vanNo = vaNo;
        uteNo = utNo;
        sportNo = spNo;
        rozzasNo = roNo;
        busNo = buNo;
        boxTruckNo = boNo;
        fluidTruckNo = flNo;

        //Resets
        taNo = 0;
        seNo = 0;
        suNo = 0;
        vaNo = 0;
        utNo = 0;
        spNo = 0;
        roNo = 0;
        buNo = 0;
        boNo = 0;
        flNo = 0;

        totalPInt = GameObject.FindWithTag("parkingTag").GetComponent<ParkingArray>().parkingSpotTotal;

        totalVechiles = totalVInt.Count;

        //Checks for disabled cars (this may break)
        GameObject[] cars = GameObject.FindGameObjectsWithTag("Car");
        foreach(GameObject car in cars)
        {
            Car c = car.GetComponent<Car>();

            if (c.disabled)
                disabledCars.Add(car);
            else
                standardCars.Add(car);
        }

        
        totalSCars = standardCars.Count;
        totalDCars = disabledCars.Count;
        totalBuses = busNo;
        totalTrucks = boxTruckNo + fluidTruckNo;

        
        if (totalVInt.Count <= totalPInt)
        {
            taBool = taNo >= 1;
            seBool = seNo >= 1;
            suBool = suNo >= 1;
            vaBool = vaNo >= 1;
            utBool = utNo >= 1;
            spBool = spNo >= 1;
            roBool = roNo >= 1;
            buBool = buNo >= 1;
            boBool = boNo >= 1;
            flBool = flNo >= 1;

            allBool = taBool && seBool && suBool && vaBool && utBool && spBool && roBool && buBool && boBool && flBool;

            StartCoroutine("SpawnCars");
        }
        else //Crashes Intentionally
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }

    private IEnumerator SpawnCars()
    {
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
                break; // Safety

            int chosenType = availableTypes[Random.Range(0, availableTypes.Count)];
            vehiclePrefab = null;
            canSpawn = false;

            switch (chosenType)
            {
                case 0: vehiclePrefab = carPrefabs[0].gameObject; removalInt = 0; canSpawn = true; break;
                case 1: vehiclePrefab = carPrefabs[1].gameObject; removalInt = 1; canSpawn = true; break;
                case 2: vehiclePrefab = carPrefabs[2].gameObject; removalInt = 2; canSpawn = true; break;
                case 3: vehiclePrefab = carPrefabs[3].gameObject; removalInt = 3; canSpawn = true; break;
                case 4: vehiclePrefab = carPrefabs[4].gameObject; removalInt = 4; canSpawn = true; break;
                case 5: vehiclePrefab = carPrefabs[5].gameObject; removalInt = 5; canSpawn = true; break;
                case 6: vehiclePrefab = carPrefabs[6].gameObject; removalInt = 6; canSpawn = true; break;
                case 7: vehiclePrefab = busPrefabs[0].gameObject; removalInt = 7; canSpawn = true; break;
                case 8: vehiclePrefab = truckPrefabs[0].gameObject; removalInt = 8; canSpawn = true; break;
                case 9: vehiclePrefab = truckPrefabs[1].gameObject; removalInt = 9; canSpawn = true; break;
            }

            if (canSpawn && !Physics.CheckSphere(spawnPoint.position, 10f, Vehicle))
            {
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

                remainingVehicles--;
            }

            yield return new WaitForSeconds(1f);
        }

        totalVInt.Clear();
        totalVechiles = 0;
    }


    private void resetTotalVInt()
    {
        totalVInt.Add(taxiNo);
        totalVInt.Add(sedanNo);
        totalVInt.Add(suvNo);
        totalVInt.Add(vanNo);
        totalVInt.Add(uteNo);
        totalVInt.Add(sportNo);
        totalVInt.Add(rozzasNo);
        totalVInt.Add(busNo);
        totalVInt.Add(boxTruckNo);
        totalVInt.Add(fluidTruckNo);
    }

    #endregion

    #region Spawner Bools

    private bool taxiSpawnabool()
    {
        if (taxiNo < 1)
            return false;
        else
            return true;
    }

    private bool sedanSpawnabool()
    {
        if (sedanNo < 1)
            return false;
        else
            return true;
    }

    private bool suvSpawnabool()
    {
        if (suvNo < 1)
            return false;
        else
            return true;
    }

    private bool vanSpawnabool()
    {
        if (vanNo < 1)
            return false;
        else
            return true;
    }

    private bool uteSpawnabool()
    {
        if (uteNo < 1)
            return false;
        else
            return true;
    }

    private bool sportsSpawnabool()
    {
        if (sportNo < 1)
            return false;
        else
            return true;
    }

    private bool rozzasSpawnabool()
    {
        if (rozzasNo < 1)
            return false;
        else
            return true;
    }

    private bool busSpawnabool()
    {
        if (busNo < 1)
            return false;
        else
            return true;
    }

    private bool boxTruckSpawnabool()
    {
        if (boxTruckNo < 1)
            return false;
        else
            return true;
    }

    private bool fluidTruckSpawnabool()
    {
        if (fluidTruckNo < 1)
            return false;
        else
            return true;
    }

    #endregion

}
