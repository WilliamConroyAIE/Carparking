using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBus : MonoBehaviour
{
    private Rigidbody rb;
    private ResultPrinter rP;
    private GameObject rPGO;

    public float parkingOutDistance = 6f;

    [Header("Clearances")]
    public float leftSideClearance = -1.5f;
    public float rightSideClearance = 1.5f;
    public float frontClearance = 3f;
    public float backClearance = -3f;

    [Header("Movement Settings")]
    public float speed = 2f;
    public float rotationSpeed = 2f;
    public float distanceToNextWaypoint = 1f;
    public float accelerationRate = 2f;
    public float decelerationRate = 4f;
    public float maxSpeed = 8f;

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

    private bool firstPark, readyToMove;
    private float shortestDistance;

    private bool hasTriggeredSpawn = false;


    [Header("Collision CheckSphere Settings")]
    public float sphereRadius = 1.5f;
    public float sphereForwardOffset = 4f;
    public float sphereHeightOffset = 1.5f;
    public LayerMask vehicleMask;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rPGO = GameObject.FindWithTag("printerTag");
        rP = rPGO.GetComponent<ResultPrinter>();
        rP.addVehicleToaVehicles(this.gameObject);
    }

    void Update()
    {
        if (!movingToPark || waypoints == null || waypoints.Count == 0) return;

        MoveToNextPoint(waypoints[currentIndex]);

        vehicleList.Clear();
        vehicleList.AddRange(GameObject.FindGameObjectsWithTag("Car"));
        vehicleList.AddRange(GameObject.FindGameObjectsWithTag("Bus"));
        vehicleList.AddRange(GameObject.FindGameObjectsWithTag("Truck"));

    }

    void MoveToNextPoint(Transform nextPoint)
    {
        Vector3 direction = nextPoint.position - transform.position;
        direction.y = 0f; // Prevent vertical rotation
        direction.Normalize();

        Vector3 proposedPosition = transform.position + direction * currentSpeed * Time.deltaTime;
        
        if (nextPoint.position == transform.position)
        {
            gameObject.SetActive(false);
        }

        targetSpeed = CanMoveTo(proposedPosition) ? speed : 0f;
        ApplySmoothMovement(direction);

        if (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(nextPoint.position.x, 0, nextPoint.position.z)) < distanceToNextWaypoint)
        {
            if (currentIndex + 1 < waypoints.Count)
                currentIndex++;

            if (currentIndex >= 18)
                SpawnNewVehicle();
        }
    }

    private void SpawnNewVehicle()
    {
        if (!hasTriggeredSpawn)
        {
            cS.AllowNextSpawn();
            print(gameObject.name + "calls CarSpawner to spawn new vehicle");
            hasTriggeredSpawn = true;
        }
    }


    void ApplySmoothMovement(Vector3 direction)
    {
        direction.Normalize();

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

    public void StartDriving(List<Transform> path, ParkingArray parkingArray, CarSpawner spawner)
    {
        waypoints = path;
        pA = parkingArray;
        cS = spawner;

        currentIndex = 0;
        movingToPark = true; // <--- THIS IS WHAT'S MISSING
    }

    
    private void OnDrawGizmosSelected()
    {
        Vector3 sphereCenter = transform.position + transform.forward * sphereForwardOffset + Vector3.up * sphereHeightOffset;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(sphereCenter, sphereRadius);
    }
}
