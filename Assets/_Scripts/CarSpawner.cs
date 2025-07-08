using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    #region Variables

    public LayerMask Vehicle;

    [Header("Arrays and Prefabs")]
    public Car[] carPrefabs;
    public Truck[] truckPrefabs;
    public Bus[] busPrefabs;

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
        for (int i = 0; i < totalVInt.Count; i++)
        {
            spawnableCarTypeInt = Random.Range(0, 11);

            switch (spawnableCarTypeInt)
            {
                case 0:
                    if (taxiSpawnabool())
                    {
                        vehiclePrefab = carPrefabs[0].gameObject;
                        removalInt = 0;
                        canSpawn = true;
                    }
                    break;

                case 1:
                    if (sedanSpawnabool())
                    {
                        vehiclePrefab = carPrefabs[1].gameObject;
                        removalInt = 1;
                        canSpawn = true;
                    }
                    break;

                case 2:
                    if (suvSpawnabool())
                    {
                        vehiclePrefab = carPrefabs[2].gameObject;
                        removalInt = 2;
                        canSpawn = true;
                    }
                    break;

                case 3:
                    if (vanSpawnabool())
                    {
                        vehiclePrefab = carPrefabs[3].gameObject;
                        removalInt = 3;
                        canSpawn = true;
                    }
                    break;

                case 4:
                    if (uteSpawnabool())
                    {
                        vehiclePrefab = carPrefabs[4].gameObject;
                        removalInt = 4;
                        canSpawn = true;
                    }
                    break;

                case 5:
                    if (sportsSpawnabool())
                    {
                        vehiclePrefab = carPrefabs[5].gameObject;
                        removalInt = 5;
                        canSpawn = true;
                    }
                    break;

                case 6:
                    if (rozzasSpawnabool())
                    {
                        vehiclePrefab = carPrefabs[6].gameObject;
                        removalInt = 6;
                        canSpawn = true;
                    }
                    break;

                case 7:
                    if (busSpawnabool())
                    {
                        vehiclePrefab = busPrefabs[0].gameObject;
                        removalInt = 7;
                        canSpawn = true;
                    }
                    break;

                case 8:
                    if (boxTruckSpawnabool())
                    {
                        vehiclePrefab = truckPrefabs[0].gameObject;
                        removalInt = 8;
                        canSpawn = true;
                    }
                    break;

                case 9:
                    if (fluidTruckSpawnabool())
                    {
                        vehiclePrefab = truckPrefabs[1].gameObject;
                        removalInt = 9;
                        canSpawn = true;
                    }
                    break;

            }

            if (canSpawn)
            {
                //Spawns cars
                GameObject nextVechileToSpawn = Instantiate(vehiclePrefab, spawnPoint.position, spawnPoint.rotation);
                
                // Decrement the proper count
                switch (removalInt)
                {
                    case 0: taxiNo--; break;
                    case 1: sedanNo--; break;
                    case 2: suvNo--; break;
                    case 3: vanNo--; break;
                    case 4: uteNo--; break;
                    case 5: sportNo--; break;
                    case 6: rozzasNo--; break;
                    case 7: busNo--; break;
                    case 8: boxTruckNo--; break;
                    case 9: fluidTruckNo--; break;
                }
                
            }

            vehiclePrefab = null;
            removalInt = 0;
            canSpawn = false;

            totalVInt.Clear();
            
            resetTotalVInt();

            bool carToClose = Physics.CheckSphere(spawnPoint.position, 10f, Vehicle);

            if (!carToClose)
            {
                yield return new WaitForSeconds(1f);

                totalVInt.Clear();
                totalVechiles = 0;
            }

        }
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
