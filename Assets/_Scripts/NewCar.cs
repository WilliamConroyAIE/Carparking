using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewCar : MonoBehaviour
{
    private ResultPrinter rP;
    private GameObject rPGO;
    public float snapBuffer = 0.5f;

    public enum carType { taxi, sedan, suv, van, ute, sport, rozzas }
    public carType currentType;
    public float parkingOutDistance = 6f;

    [Header("Clearances")]
    public float leftSideClearance;
    public float rightSideClearance, frontClearance, backClearance;

    [SerializeField] private float leftSide, rightSide, frontSide, backSide;

    [Header("Movement Settings")]
    public float speed = 2f;
    public float rotationSpeed = 2f;
    public float distanceToNextWaypoint = 1f;
    public float accelerationRate = 2f;
    public float decelerationRate = 4f;
    public float maxSpeed = 10f;

    internal float currentSpeed = 0f;
    private float targetSpeed = 0f;

    [Header("State & References")]
    public bool disabled;
    internal bool movingToPark = false;
    private int currentIndex = 0;
    private Transform targetWaypoint;
    private List<Transform> waypoints;
    private ParkingArray pA;
    private CarSpawner cS;
    public Transform chosenPark;
    public List<GameObject> vehicleList;

    private NewCar oCar;
    private NewBus oBus;
    private NewTruck oTruck;
    private bool carTrue, busTrue, truckTrue;

    private bool firstPark, readyToMove;
    private float shortestDistance;

    public bool parked;

    private bool hasTriggeredSpawn = false;


    [Header("Collision CheckSphere Settings")]
    public float sphereRadius = 1.5f;
    public float sphereForwardOffset = 4f;
    public float sphereHeightOffset = 1.5f;
    public LayerMask vehicleMask;

    void Awake()
    {            
        switch (currentType)
        {
            case carType.taxi:   SetSides(-1.08f, 2.69f, -2.7f); break;
            case carType.sedan:  SetSides(-0.989f, 2.01f, -1.75f); break;
            case carType.suv:    SetSides(-1.02f, 2.06f, -1.8f); break;
            case carType.van:    SetSides(-1.17f, 1.85f, -2.56f); break;
            case carType.ute:    SetSides(-0.99f, 2.052f, -1.76f); break;
            case carType.sport:  SetSides(-1.071f, 2.297f, -1.894f); break;
            case carType.rozzas: SetSides(-1.07f, 2.76f, -2.76f); break;
        }

        leftSideClearance = leftSide + 0.5f;
        rightSideClearance = rightSide - 0.5f;
        frontClearance = frontSide + 0.25f;
        backClearance = backSide;

        rPGO = GameObject.FindWithTag("printerTag");
        rP = rPGO.GetComponent<ResultPrinter>();
        rP.addVehicleToaVehicles(this.gameObject);
    }

    public void StartDriving(List<Transform> path, ParkingArray parkingArray, CarSpawner spawner)
    {
        waypoints = path;
        pA = parkingArray;
        cS = spawner;

        bool triedStandard = false;
        bool triedDisabled = false;

        Transform bestSpot = null;
        shortestDistance = Mathf.Infinity;

        // Try finding a spot as a standard car
        if (!disabled)
        {
            triedStandard = true;
            bestSpot = GetBestAvailableSpot(pA.availableStandard);
        }

        // If no standard spot found, try as a disabled car
        if (bestSpot == null)
        {
            disabled = true;
            triedDisabled = true;
            bestSpot = GetBestAvailableSpot(pA.availableDisabled);

            if (bestSpot != null)
                Debug.Log($"{gameObject.name} could not find standard parking. Switched to DISABLED car.");
        }

        if (bestSpot != null)
        {
            chosenPark = bestSpot;
            bestSpot.GetComponent<ParkingSpot>().taken = true;
            //Debug.Log($"{gameObject.name} chose parking spot: {bestSpot.name} at distance {shortestDistance} (disabled={disabled})");

            currentIndex = 0;
            movingToPark = true;
            readyToMove = true;
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} couldn't find a valid parking spot!");
        }
    }

    private Transform GetBestAvailableSpot(List<GameObject> spotList)
    {
        Transform bestSpot = null;
        shortestDistance = Mathf.Infinity;

        if (spotList == null || spotList.Count == 0)
            return null;

        foreach (GameObject s in spotList)
        {
            ParkingSpot pS = s.GetComponent<ParkingSpot>();
            if (pS == null || pS.taken) continue;

            float newDistance = Vector3.Distance(transform.position, s.transform.position);
            if (firstPark || newDistance < shortestDistance)
            {
                shortestDistance = newDistance;
                bestSpot = s.transform;
                firstPark = false;
            }
        }

        return bestSpot;
    }


    void Update()
    {
        if (!movingToPark) return;

        vehicleList.Clear();
        vehicleList.AddRange(GameObject.FindGameObjectsWithTag("Car"));
        vehicleList.AddRange(GameObject.FindGameObjectsWithTag("Bus"));
        vehicleList.AddRange(GameObject.FindGameObjectsWithTag("Truck"));

        if (readyToMove)
            CheckingDistances();
    }

    private void SpawnNewVehicle()
    {
        if (!hasTriggeredSpawn)
        {
            cS.AllowNextSpawn();
            //print(gameObject.name + "calls CarSpawner to spawn new vehicle");
            hasTriggeredSpawn = true;
        }
    }

    void CheckingDistances()
    {
        float distanceToPark = Vector3.Distance(transform.position, chosenPark.position);

        // NEW: Immediately go to park if within 1f, regardless of next waypoint
        if (distanceToPark <= parkingOutDistance)
        {
            MoveToParkingSpot();
            return;
        }

        float distanceToNextWaypoint = Vector3.Distance(transform.position, waypoints[currentIndex].position);
        bool parkCloser = distanceToPark <= distanceToNextWaypoint;

        if (!parkCloser)
            MoveToNextPoint(waypoints[currentIndex]);
        else
            MoveToParkingSpot();
    }


    void MoveToNextPoint(Transform nextPoint)
    {
        Vector3 direction = (nextPoint.position - transform.position).normalized;
        Vector3 proposedPosition = transform.position + direction * currentSpeed * Time.deltaTime;

        targetSpeed = CanMoveTo(proposedPosition) ? speed : 0f;
        ApplySmoothMovement(direction);

        if (Vector3.Distance(transform.position, nextPoint.position) < distanceToNextWaypoint)
        {
            if (currentIndex + 1 < waypoints.Count)
                currentIndex++;

            if (currentIndex >= 10)
                SpawnNewVehicle();

        }
    }

    void MoveToParkingSpot()
    {
        Vector3 direction = (chosenPark.position - transform.position).normalized;
        Vector3 proposedPosition = transform.position + direction * currentSpeed * Time.deltaTime;

        targetSpeed = CanMoveTo(proposedPosition) ? speed : 0f;
        ApplySmoothMovement(direction);

        float dist = Vector3.Distance(transform.position, chosenPark.position);
        Quaternion flatTargetRot = Quaternion.Euler(0f, chosenPark.rotation.eulerAngles.y, 0f);
        float angleDiff = Quaternion.Angle(transform.rotation, flatTargetRot);

        // --- NEW SNAP BUFFER ---
        if (dist < snapBuffer)
        {
            // Snap instantly to position and rotation
            transform.position = chosenPark.position;
            transform.rotation = flatTargetRot;

            if (disabled) pA.availableDisabled.Remove(chosenPark.gameObject);
            else pA.availableStandard.Remove(chosenPark.gameObject);

            currentSpeed = 0f;
            targetSpeed = 0f;
            movingToPark = false;

            leftSideClearance = 0;
            rightSideClearance = 0;
            frontClearance = 0;
            backClearance = 0;


            parked = true;
            //print(gameObject.name + " has now parked (snapped).");
            SpawnNewVehicle();
            rP.countHowManyAreParkedFunction();
            this.enabled = false;

            return;
        }

        // Original gradual rotation logic (fallback if outside snap range)
        if (dist < 0.2f)
        {
            Quaternion currentFlatRot = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
            transform.rotation = Quaternion.RotateTowards(currentFlatRot, flatTargetRot, rotationSpeed * Time.deltaTime);

            if (angleDiff < 1f)
            {
                // Snap when fully aligned
                transform.position = chosenPark.position;
                transform.rotation = flatTargetRot;

                if (disabled) pA.availableDisabled.Remove(chosenPark.gameObject);
                else pA.availableStandard.Remove(chosenPark.gameObject);

                currentSpeed = 0f;
                targetSpeed = 0f;
                movingToPark = false;

                leftSideClearance = 0;
                rightSideClearance = 0;
                frontClearance = 0;
                backClearance = 0;

                parked = true;
                //print(gameObject.name + " has now parked.");
                SpawnNewVehicle();
                this.enabled = false;
            }
        }
    }


    void ApplySmoothMovement(Vector3 direction)
    {
        direction.Normalize();

        // Forward raycast collision check
        float raycastDistance = 1.5f;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, out RaycastHit hit, raycastDistance))
        {
            if (hit.collider.CompareTag("Car") || hit.collider.CompareTag("Bus") || hit.collider.CompareTag("Truck"))
            {
                targetSpeed = 0f;
            }
        }

        if (targetSpeed > currentSpeed)
            currentSpeed += accelerationRate * Time.deltaTime;
        else if (targetSpeed < currentSpeed)
            currentSpeed -= decelerationRate * Time.deltaTime;

        currentSpeed = Mathf.Clamp(currentSpeed, 0f, maxSpeed);

        Vector3 flatDirection = new Vector3(direction.x, 0f, direction.z).normalized;
        if (flatDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(flatDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        transform.position += direction * currentSpeed * Time.deltaTime;

        // Forward raycast collision check
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, out RaycastHit hitt, raycastDistance))
        {
            if (hitt.collider.CompareTag("Car") || hitt.collider.CompareTag("Bus") || hitt.collider.CompareTag("Truck"))
            {
                GameObject detected = hitt.collider.gameObject;

                bool isStoppedAndNotParked = false;

                if (detected.CompareTag("Car"))
                {
                    NewCar other = detected.GetComponent<NewCar>();
                    if (other != null && other.enabled && other.movingToPark && other.currentSpeed < 0.1f)
                        isStoppedAndNotParked = true;
                }
                else if (detected.CompareTag("Bus"))
                {
                    NewBus other = detected.GetComponent<NewBus>();
                    if (other != null && other.enabled && other.movingToPark && other.currentSpeed < 0.1f)
                        isStoppedAndNotParked = true;
                }
                else if (detected.CompareTag("Truck"))
                {
                    NewTruck other = detected.GetComponent<NewTruck>();
                    if (other != null && other.enabled && other.movingToPark && other.currentSpeed < 0.1f)
                        isStoppedAndNotParked = true;
                }

                if (isStoppedAndNotParked)
                {
                    targetSpeed = 0f;
                }
            }
        }
    }

    private void SetSides(float sideOffset, float front, float back)
    {
        leftSide = sideOffset;
        rightSide = -sideOffset;
        frontSide = front;
        backSide = back;
    }

    public bool CanMoveTo(Vector3 proposedPosition)
    {
        Vector3 sphereCenter = proposedPosition + transform.forward * sphereForwardOffset + Vector3.up * sphereHeightOffset;

        Collider[] hits = Physics.OverlapSphere(sphereCenter, sphereRadius, vehicleMask);
        foreach (Collider col in hits)
        {
            if (col.gameObject == gameObject) continue;

            string tag = col.tag;
            if (tag == "Car" || tag == "Bus" || tag == "Truck")
            {
                return false;
            }
        }

        return true;
    }



    void OnDrawGizmos()
    {
        if (!parked)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, parkingOutDistance);

            Vector3 sphereCenter = transform.position + transform.forward * sphereForwardOffset + Vector3.up * sphereHeightOffset;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(sphereCenter, sphereRadius);
        }
    }
}
