using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewCar : MonoBehaviour
{
    private Rigidbody rb;
    private ResultPrinter rP;
    private GameObject rPGO;

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

    private float currentSpeed = 0f;
    private float targetSpeed = 0f;

    [Header("State & References")]
    public bool disabled;
    private bool movingToPark = false;
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

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
            
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

        List<GameObject> spotList = disabled ? pA.availableDisabled : pA.availableStandard;
        shortestDistance = Mathf.Infinity;
        Transform bestSpot = null;

        foreach (GameObject s in spotList)
        {
            ParkingSpot pS = s.GetComponent<ParkingSpot>();
            if (pS.taken) continue;

            float newDistance = Vector3.Distance(transform.position, s.transform.position);
            if (firstPark || newDistance < shortestDistance)
            {
                shortestDistance = newDistance;
                bestSpot = s.transform;
                firstPark = false;
            }
        }

        if (bestSpot != null)
        {
            chosenPark = bestSpot;
            bestSpot.GetComponent<ParkingSpot>().taken = true;
            currentIndex = 0;
            movingToPark = true;
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} couldn't find a valid parking spot!");
        }

        readyToMove = true;
    }

    void Update()
    {
        if (!movingToPark) return;

        vehicleList.Clear(); // Add this at the start to prevent growth

        GameObject[] cars = GameObject.FindGameObjectsWithTag("Car");
        foreach(GameObject c in cars)
            vehicleList.Add(c);

        GameObject[] buses = GameObject.FindGameObjectsWithTag("Bus");
        foreach(GameObject b in buses)
            vehicleList.Add(b);

        GameObject[] trucks = GameObject.FindGameObjectsWithTag("Truck");
        foreach(GameObject t in trucks)
            vehicleList.Add(t);


        if (readyToMove)
            CheckingDistances();
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

        if (currentIndex >= waypoints.Count)
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
        }
    }

    void MoveToParkingSpot()
    {
        Vector3 direction = (chosenPark.position - transform.position).normalized;
        Vector3 proposedPosition = transform.position + direction * currentSpeed * Time.deltaTime;

        targetSpeed = CanMoveTo(proposedPosition) ? speed : 0f;
        ApplySmoothMovement(direction);

        float dist = Vector3.Distance(transform.position, chosenPark.position);

        if (dist < 0.2f)
        {
            Quaternion flatTargetRot = Quaternion.Euler(0f, chosenPark.rotation.eulerAngles.y, 0f);

            // Only rotate in Y axis
            Quaternion currentFlatRot = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
            Quaternion targetFlatRot = Quaternion.Euler(0f, chosenPark.rotation.eulerAngles.y, 0f);
            transform.rotation = Quaternion.RotateTowards(currentFlatRot, targetFlatRot, rotationSpeed * Time.deltaTime);

            float angleDiff = Quaternion.Angle(transform.rotation, flatTargetRot);

            if (angleDiff < 1f)
            {
                // --- Replace this section with a snap ---
                transform.position = chosenPark.position;
                transform.rotation = Quaternion.Euler(0f, chosenPark.rotation.eulerAngles.y, 0f);

                if (disabled) pA.availableDisabled.Remove(chosenPark.gameObject);
                else pA.availableStandard.Remove(chosenPark.gameObject);

                currentSpeed = 0f;
                targetSpeed = 0f;
                movingToPark = false;

                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.constraints = RigidbodyConstraints.FreezeAll;
                }

                leftSideClearance = 0;
                rightSideClearance = 0;
                frontClearance = 0;
                backClearance = 0;

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
        if (vehicleList == null || vehicleList.Count <= 1)
            return true;

        foreach (GameObject otherGO in vehicleList)
        {
            if (otherGO == null || otherGO == gameObject || !otherGO.activeInHierarchy)
                continue;

            Transform other = otherGO.transform;

            carTrue = busTrue = truckTrue = false;

            if (other.CompareTag("Car"))
            {
                oCar = other.GetComponent<NewCar>();
                if (oCar == null) continue;
                carTrue = true;
            }
            else if (other.CompareTag("Bus"))
            {
                oBus = other.GetComponent<NewBus>();
                if (oBus == null) continue;
                busTrue = true;
            }
            else if (other.CompareTag("Truck"))
            {
                oTruck = other.GetComponent<NewTruck>();
                if (oTruck == null) continue;
                truckTrue = true;
            }

            float ourLeft = proposedPosition.x + leftSideClearance;
            float ourRight = proposedPosition.x + rightSideClearance;
            float ourFront = proposedPosition.z + frontClearance;
            float ourBack = proposedPosition.z + backClearance;

            float theirLeft = 0, theirRight = 0, theirFront = 0, theirBack = 0;

            if (carTrue)
            {
                var pos = oCar.transform.position;
                theirLeft = pos.x + oCar.leftSideClearance;
                theirRight = pos.x + oCar.rightSideClearance;
                theirFront = pos.z + oCar.frontClearance;
                theirBack = pos.z + oCar.backClearance;
            }
            else if (busTrue)
            {
                var pos = oBus.transform.position;
                theirLeft = pos.x + oBus.leftSideClearance;
                theirRight = pos.x + oBus.rightSideClearance;
                theirFront = pos.z + oBus.frontClearance;
                theirBack = pos.z + oBus.backClearance;
            }
            else if (truckTrue)
            {
                var pos = oTruck.transform.position;
                theirLeft = pos.x + oTruck.leftSideClearance;
                theirRight = pos.x + oTruck.rightSideClearance;
                theirFront = pos.z + oTruck.frontClearance;
                theirBack = pos.z + oTruck.backClearance;
            }

            bool overlapX = !(ourRight < theirLeft || ourLeft > theirRight);
            bool overlapZ = !(ourFront < theirBack || ourBack > theirFront);

            if (overlapX && overlapZ)
                return false;
        }

        return true;
    }


    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, parkingOutDistance);
    }
}
