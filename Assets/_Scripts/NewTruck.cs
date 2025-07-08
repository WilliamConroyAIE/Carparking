using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewTruck : MonoBehaviour
{
    [Header("Model")]
    public Transform model;

    [Header("Clearances")]
    public float leftSideClearance;
    public float rightSideClearance, frontClearance, backClearance;

    [SerializeField] private float leftSide, rightSide, frontSide, backSide;

    [Header("Movement Settings")]
    public float speed = 1.5f;
    public float rotationSpeed = 1.5f;
    public float distanceToNextWaypoint = 1f;
    public float accelerationRate = 1.5f;
    public float decelerationRate = 2f;
    public float maxSpeed = 8f;

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
    //public Transform chosenPark;
    public List<GameObject> vehicleList;

    private NewCar oCar;
    private NewBus oBus;
    private NewTruck oTruck;
    private bool carTrue, busTrue, truckTrue;

    private bool firstPark;
    private float shortestDistance;

    void Awake()
    {
        SetSides(-1.2f, 5f, -4f);

        leftSideClearance = model.localPosition.x + leftSide + 0.5f;
        rightSideClearance = model.localPosition.x + rightSide - 0.5f;
        frontClearance = model.localPosition.z + frontSide + 0.25f;
        backClearance = model.localPosition.z + backSide;
    }

    void Update()
    {
        if (!movingToPark || waypoints == null || waypoints.Count == 0) return;

        MoveToNextPoint(waypoints[currentIndex]);
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

    void ApplySmoothMovement(Vector3 direction)
    {
        direction.Normalize();

        // Check raycast forward to avoid hitting other vehicles
        float raycastDistance = 1.5f; // adjust as needed
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, out hit, raycastDistance))
        {
            if (hit.collider.CompareTag("Car") || hit.collider.CompareTag("Bus") || hit.collider.CompareTag("Truck"))
            {
                targetSpeed = 0f; // Stop if another vehicle is too close ahead
            }
        }

        if (targetSpeed > currentSpeed)
            currentSpeed += accelerationRate * Time.deltaTime;
        else if (targetSpeed < currentSpeed)
            currentSpeed -= decelerationRate * Time.deltaTime;

        currentSpeed = Mathf.Clamp(currentSpeed, 0, maxSpeed);

        if (direction != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
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
        foreach (GameObject otherGO in vehicleList)
        {
            Transform other = otherGO.transform;
            if (other == transform) continue;

            carTrue = busTrue = truckTrue = false;

            if (other.CompareTag("Car")) { carTrue = true; oCar = other.GetComponent<NewCar>(); }
            else if (other.CompareTag("Bus")) { busTrue = true; oBus = other.GetComponent<NewBus>(); }
            else if (other.CompareTag("Truck")) { truckTrue = true; oTruck = other.GetComponent<NewTruck>(); }

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

            if (overlapX && overlapZ) return false;
        }

        return true;
    }

    public void StartDriving(List<Transform> path, ParkingArray parkingArray, CarSpawner spawner)
    {
        waypoints = path;
        pA = parkingArray;
        cS = spawner;
    }
}
