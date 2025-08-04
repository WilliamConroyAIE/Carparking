using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    #region Variables
    
    public GameObject requestPanel;
    private bool onRequestPanel, changeToSimAllowed;

    private ParkingArray pA;
    private CarSpawner cS;
    private ResultPrinter rP;


    [Header("Text")]
    public TextMeshProUGUI allText;
    //public TextMeshProUGUI standardCarText, disabledCarText, busText, truckText;

    [Space]

    public TextMeshProUGUI requestedTotalCarText;
    public TextMeshProUGUI requestedTotalTruckText;

    [Space]

    public TextMeshProUGUI requestedText;
    public TextMeshProUGUI changeSimText, timerText;

    
    [Header("Ints")]
    public int requestedStandardCarInt;
    public int requestedDisabledCarInt, requestedBusInt, requestedTruckInt;
    
    [Space]
    
    public int requestedTaxi;
    public int requestedSedan, requestedSUV, requestedVan, requestedUte, requestedSport, requestedRozzas; 
    
    [Space]
    
    public int requestedBoxTruck; 
    public int requestedFluidTruck;

    
    [Header("InputFields")]
    public TMP_InputField standardCarIF;
    public TMP_InputField disabledCarIF, busIF, truckIF;

    [Space]

    public TMP_InputField taxiIF;
    public TMP_InputField sedanIF, suvIF, vanIF, uteIF, sportIF, rozzasIF;

    [Space]
    
    public TMP_InputField boxIF;
    public TMP_InputField fluidIF;


    [Header("FinishedSimStuff")]
    public GameObject finishedPanel;
    public TextMeshProUGUI parkedNumberText, failedNumberText, parkEfficiencyText, timeStatementText;

    private int parkingEfficiency;


    #endregion

    private int minutes, seconds;

    private int newCount, parkedCount;

    #region Start + Update + Sim
    void Start()
    {
        //Gets the scripts to collect variables
        pA = GameObject.FindWithTag("parkingTag").GetComponent<ParkingArray>();
        cS = GameObject.FindWithTag("spawnerTag").GetComponent<CarSpawner>();
        rP = GameObject.FindWithTag("printerTag").GetComponent<ResultPrinter>();

        finishedPanel.SetActive(false);
        requestPanel.SetActive(true);
        onRequestPanel = true;

        //God actually knows what this does, thank my VET Course Instructor
        standardCarIF.onValueChanged.AddListener(OnSCarValueChanged);
        disabledCarIF.onValueChanged.AddListener(OnDCarValueChanged);
        busIF.onValueChanged.AddListener(OnBusValueChanged);
        truckIF.onValueChanged.AddListener(OnTruckValueChanged);

        //Variants
        taxiIF.onValueChanged.AddListener(OnTaxiValueChanged);
        sedanIF.onValueChanged.AddListener(OnSedanValueChanged);
        suvIF.onValueChanged.AddListener(OnSUVValueChanged);
        vanIF.onValueChanged.AddListener(OnVanValueChanged);
        uteIF.onValueChanged.AddListener(OnUteValueChanged);
        sportIF.onValueChanged.AddListener(OnSportValueChanged);
        rozzasIF.onValueChanged.AddListener(OnRozzasValueChanged);
        boxIF.onValueChanged.AddListener(OnBoxTruckValueChanged);
        fluidIF.onValueChanged.AddListener(OnFluidTruckValueChanged);
        
        parkedCount = rP.GetTotalParked();
        StartCoroutine(StartCount());
    }

    IEnumerator StartCount()
    {

        if (parkedCount != newCount)
        {
            newCount = parkedCount;
        }
        
        yield return new WaitForSeconds(1f);

        parkedCount = rP.GetTotalParked();
        //Debug.Log("newCount = " +$"{newCount}");

        StartCoroutine(StartCount());
    }

    void Update()
    {
        if (!onRequestPanel)
        {
            if (cS == null || allText == null) return;

            // Get total parked
            allText.text = $"{newCount} / {cS.totalVechiles}";
            //standardCarText.text = cS.totalSCars.ToString();
            //disabledCarText.text = cS.totalDCars.ToString();

            // Show the current time
            if (cS.timerRunning)
            {
                minutes = Mathf.FloorToInt(cS.simulationTimer / 60f);
                seconds = Mathf.FloorToInt(cS.simulationTimer % 60f);
                timerText.text = "Time: " + $"{minutes:00}:{seconds:00}";

            }
        }
        else
        {
            //Checks all vehicles
            int totalRequested = requestedStandardCarInt + requestedDisabledCarInt + requestedBusInt + requestedTruckInt;

            //Checks all car varaints.
            int requestedCarSpecTotal = requestedTaxi + requestedSedan + requestedSUV + requestedVan + requestedUte + requestedSport + requestedRozzas;

            //Both requested Car amounts by disabled or general type
            int requestedCarGenTotal = requestedDisabledCarInt + requestedStandardCarInt;
            
            //Compare specific trucks to total requests (visuals mean that people don't over do the truck amount)
            requestedTotalCarText.text = "Requested Cars: " + $"{requestedCarSpecTotal}" + " / " + $"{requestedCarGenTotal}";

            
            //Checks truck Variants
            int requestedTruckTotal = requestedBoxTruck + requestedFluidTruck;
            
            //Compare specific trucks to total requests (visuals mean that people don't over do the truck amount)
            requestedTotalTruckText.text = "Requested Trucks: " + $"{requestedTruckTotal}" + " / " + $"{requestedTruckInt}";

            requestedText.text = "Requested Vehicles Count: " + $"{totalRequested}" + " / " + $"{pA.parkingSpotTotal}";
            
            if (totalRequested > pA.parkingSpotTotal)
            {
                changeToSimAllowed = false;
                changeSimText.text = "Reduce vehicle request count to " + $"{pA.parkingSpotTotal}" + "or Less.";
            }
            else
            {
                changeToSimAllowed = true;
                changeSimText.text = "";
            }
        }

    }

    public void StartSim()
    {
        if (changeToSimAllowed)
        {
            requestPanel.SetActive(false);
            
            cS.StartTestRun(requestedTaxi, requestedSedan, requestedSUV, requestedVan, requestedUte, requestedSport, 
                requestedRozzas, requestedBusInt, requestedBoxTruck, requestedFluidTruck); 

            onRequestPanel = false;

            rP.StartDebugCheckLoop();
        }
        else
        {
            print("Ensure requests total less than or equal to available parking spots");
        }
    }

    #endregion


    #region Input Field Stuff

    private void OnSCarValueChanged(string value)
    {
        if (int.TryParse(value, out int number))
        {
            if (number > pA.parkingSpotTotal)
            {
                print("e");
            }
            else
            requestedStandardCarInt = number;
            changeToSimAllowed = true;
        }
        else
        {
            print("can't change to sim");
            changeToSimAllowed = false;
        }
    }

    private void OnDCarValueChanged(string value)
    {
        if (int.TryParse(value, out int number))
        {
            if (number > pA.parkingSpotTotal)
            {
                print("e");
            }
            else
            requestedDisabledCarInt = number;
        }
        else
        {
            print("can't change to sim");
            changeToSimAllowed = false;
        }
    }

    private void OnBusValueChanged(string value)
    {
        if (int.TryParse(value, out int number))
        {
            if (number > pA.parkingSpotTotal)
            {
                print("e");
            }
            else
            requestedBusInt = number;
        }
        else
        {
            print("can't change to sim");
            changeToSimAllowed = false;
        }
    }

    private void OnTruckValueChanged(string value)
    {
        if (int.TryParse(value, out int number))
        {
            if (number > pA.parkingSpotTotal)
            {
                print("e");
            }
            else
            requestedTruckInt = number;
        }
        else
        {
            print("can't change to sim");
            changeToSimAllowed = false;
        }
    }

    #endregion


    #region Variants
    private void OnTaxiValueChanged(string value)
    {
        if (int.TryParse(value, out int number))
        {
            if (number > (requestedStandardCarInt + requestedDisabledCarInt))
            {
                print("e");
            }
            else
            requestedTaxi = number;
            changeToSimAllowed = true;
        }
        else
        {
            print("can't change to sim");
            changeToSimAllowed = false;
        }
    }

    private void OnSedanValueChanged(string value)
    {
        if (int.TryParse(value, out int number))
        {
            if (number > (requestedStandardCarInt + requestedDisabledCarInt))
            {
                print("e");
            }
            else
            requestedSedan = number;
            changeToSimAllowed = true;
        }
        else
        {
            print("can't change to sim");
            changeToSimAllowed = false;
        }
    }

    private void OnSUVValueChanged(string value)
    {
        if (int.TryParse(value, out int number))
        {
            if (number > (requestedStandardCarInt + requestedDisabledCarInt))
            {
                print("e");
            }
            else
            requestedSUV = number;
            changeToSimAllowed = true;
        }
        else
        {
            print("can't change to sim");
            changeToSimAllowed = false;
        }
    }

    private void OnVanValueChanged(string value)
    {
        if (int.TryParse(value, out int number))
        {
            if (number > (requestedStandardCarInt + requestedDisabledCarInt))
            {
                print("e");
            }
            else
            requestedVan = number;
            changeToSimAllowed = true;
        }
        else
        {
            print("can't change to sim");
            changeToSimAllowed = false;
        }
    }

    private void OnUteValueChanged(string value)
    {
        if (int.TryParse(value, out int number))
        {
            if (number > (requestedStandardCarInt + requestedDisabledCarInt))
            {
                print("e");
            }
            else
            requestedUte = number;
            changeToSimAllowed = true;
        }
        else
        {
            print("can't change to sim");
            changeToSimAllowed = false;
        }
    }
    
    private void OnSportValueChanged(string value)
    {
        if (int.TryParse(value, out int number))
        {
            if (number > (requestedStandardCarInt + requestedDisabledCarInt))
            {
                print("e");
            }
            else
            requestedSport = number;
            changeToSimAllowed = true;
        }
        else
        {
            print("can't change to sim");
            changeToSimAllowed = false;
        }
    }

    private void OnRozzasValueChanged(string value)
    {
        if (int.TryParse(value, out int number))
        {
            if (number > (requestedStandardCarInt + requestedDisabledCarInt))
            {
                print("e");
            }
            else
            requestedRozzas = number;
            changeToSimAllowed = true;
        }
        else
        {
            print("can't change to sim");
            changeToSimAllowed = false;
        }
    }

    private void OnBoxTruckValueChanged(string value)
    {
        if (int.TryParse(value, out int number))
        {
            if (number > requestedTruckInt)
            {
                print("e");
            }
            else
            requestedBoxTruck = number;
            changeToSimAllowed = true;
        }
        else
        {
            print("can't change to sim");
            changeToSimAllowed = false;
        }
    }

    private void OnFluidTruckValueChanged(string value)
    {
        if (int.TryParse(value, out int number))
        {
            if (number > requestedTruckInt)
            {
                print("e");
            }
            else
            requestedFluidTruck = number;
            changeToSimAllowed = true;
        }
        else
        {
            print("can't change to sim");
            changeToSimAllowed = false;
        }
    }
    
    #endregion

    public void RandomizeVehicleCounts()
    {
        int maxSpots = pA.parkingSpotTotal;

        // Randomly divide maxSpots into Standard, Disabled, Bus, and Truck counts
        requestedStandardCarInt = Random.Range(0, maxSpots + 1);
        int remainingAfterStandard = maxSpots - requestedStandardCarInt;

        requestedDisabledCarInt = Random.Range(0, remainingAfterStandard + 1);
        int remainingAfterDisabled = remainingAfterStandard - requestedDisabledCarInt;

        requestedBusInt = Random.Range(0, remainingAfterDisabled + 1);
        int remainingAfterBus = remainingAfterDisabled - requestedBusInt;

        requestedTruckInt = remainingAfterBus;

        // Now distribute car variants within total cars (standard + disabled)
        int totalCars = requestedStandardCarInt + requestedDisabledCarInt;

        // To avoid variants exceeding total cars, randomly assign with sum capped at totalCars
        requestedTaxi = Random.Range(0, totalCars + 1);
        requestedSedan = Random.Range(0, totalCars - requestedTaxi + 1);
        requestedSUV = Random.Range(0, totalCars - requestedTaxi - requestedSedan + 1);
        requestedVan = Random.Range(0, totalCars - requestedTaxi - requestedSedan - requestedSUV + 1);
        requestedUte = Random.Range(0, totalCars - requestedTaxi - requestedSedan - requestedSUV - requestedVan + 1);
        requestedSport = Random.Range(0, totalCars - requestedTaxi - requestedSedan - requestedSUV - requestedVan - requestedUte + 1);
        requestedRozzas = totalCars - (requestedTaxi + requestedSedan + requestedSUV + requestedVan + requestedUte + requestedSport);

        // Similarly distribute truck variants within total trucks
        requestedBoxTruck = Random.Range(0, requestedTruckInt + 1);
        requestedFluidTruck = requestedTruckInt - requestedBoxTruck;

        // Update input fields UI to reflect these values
        standardCarIF.text = requestedStandardCarInt.ToString();
        disabledCarIF.text = requestedDisabledCarInt.ToString();
        busIF.text = requestedBusInt.ToString();
        truckIF.text = requestedTruckInt.ToString();

        taxiIF.text = requestedTaxi.ToString();
        sedanIF.text = requestedSedan.ToString();
        suvIF.text = requestedSUV.ToString();
        vanIF.text = requestedVan.ToString();
        uteIF.text = requestedUte.ToString();
        sportIF.text = requestedSport.ToString();
        rozzasIF.text = requestedRozzas.ToString();

        boxIF.text = requestedBoxTruck.ToString();
        fluidIF.text = requestedFluidTruck.ToString();

        changeToSimAllowed = true;
    }

    public void FinishedSim(int parkedNumber, int failedNumber, bool simIsFinished)
    {
        if (simIsFinished)
        {
            finishedPanel.SetActive(true);

            parkedNumberText.text = "Vehicles Parked: " + $"{parkedNumber}";
            failedNumberText.text = "Vehicles unable to find a park: " + $"{failedNumber}";

            int time = Mathf.RoundToInt(cS.simulationTimer);

            if (time >= 1)
            {
                parkingEfficiency = time % parkedNumber;
            }

            timeStatementText.text = "It took " + $"{minutes:00}:{seconds:00}" + " to park all cars";

            parkEfficiencyText.text = "Average time took to park one car: " + $"{parkingEfficiency}" + " seconds";
        }
    }
}