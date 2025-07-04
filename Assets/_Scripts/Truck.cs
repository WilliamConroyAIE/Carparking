using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Truck : MonoBehaviour
{
    #region SimStuff
    [Header("SimStuff")]
    public Transform model;

    [Header("Clearances")]
    public float leftSideClearance;
    public float rightSideClearance, frontClearance, backClearance;

    [Header("Sides")]
    [SerializeField] private float leftSide;
    [SerializeField] private float rightSide, frontSide, backSide;

    private void Awake()
    {
        SetSides(-2.94f, 8.01f, -8.54f);

        // Calculate clearances
        leftSideClearance = model.position.x + leftSide + 0.5f;
        rightSideClearance = model.position.x + rightSide - 0.5f;
        frontClearance = model.position.z + frontSide + 0.25f;
        backClearance = model.position.z + backSide;
    }

    private void SetSides(float sideOffset, float front, float back)
    {
        leftSide = sideOffset;
        rightSide = -sideOffset;
        frontSide = front;
        backSide = back;
    }
    #endregion

    [Header("Wheel Colliders")]
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelRL;
    public WheelCollider wheelRR;

    [Header("Wheel Meshes")]
    public Transform meshFL;
    public Transform meshFR;
    public Transform meshRL;
    public Transform meshRR;

    [Header("Car Settings")]
    public float maxMotorTorque = 1500f;
    public float maxSteerAngle = 30f;
    public float brakeForce = 3000f;

    private float motor;
    private float steer;
    private float brake;

    private bool firstPark;
    public float shortestDistance;

    public float speed = 2f;
    public float rotationSpeed = 2f;
    public float distanceToNextWaypoint = 1f;

    private int countIndex = 0;
    private bool movingToPark = false;
    private Transform targetWaypoint;
    private List<Transform> waypoints;
    private List<GameObject> vehicleList;
    private ParkingArray pA;
    private CarSpawner cS;
    private bool carTrue, busTrue, truckTrue;

    [Header("Smooth Movement")]
    public float accelerationRate = 2f;
    public float decelerationRate = 4f;
    public float maxSpeed = 10f; // m/s, tune as needed
    private float currentSpeed = 0f;
    private float targetSpeed = 0f;


    void Start()
    {
        //Gets the parking arrays so they are only stored in one object
        pA = GameObject.FindWithTag("parkingTag").GetComponent<ParkingArray>();
        waypoints = pA.parkingWaypoints;

        firstPark = true;

        cS = GameObject.FindWithTag("spawnerTag").GetComponent<CarSpawner>();
        cS.vehicles.Add(this.gameObject);
        vehicleList = cS.vehicles;

    }


    void Update()
    {
        //If not at max waypoint
        if (currentIndex < waypoints.Count)
        {
            MoveToNextPoint(currentIndex);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void FixedUpdate()
    {
        // Apply steering
        wheelFL.steerAngle = steer;
        wheelFR.steerAngle = steer;

        // Apply motor torque
        wheelFL.motorTorque = motor;
        wheelFR.motorTorque = motor;

        // Apply brakes
        wheelFL.brakeTorque = brake;
        wheelFR.brakeTorque = brake;
        wheelRL.brakeTorque = brake;
        wheelRR.brakeTorque = brake;

        UpdateWheels();
    }

    void UpdateWheels()
    {
        UpdateWheelPose(wheelFL, meshFL);
        UpdateWheelPose(wheelFR, meshFR);
        UpdateWheelPose(wheelRL, meshRL); 
        UpdateWheelPose(wheelRR, meshRR);
    }

    void UpdateWheelPose(WheelCollider collider, Transform mesh)
    {
        Vector3 pos;
        Quaternion rot;
        collider.GetWorldPose(out pos, out rot);
        mesh.position = pos;
        mesh.rotation = rot; // or -90, depending on your mesh's orientation
    }

    private int currentIndex = 0;
    private Car oCar;
    private Bus oBus;
    private Truck oTruck;

    public bool CanMoveTo(Vector3 proposedPosition)
    {
        foreach (GameObject otherGO in vehicleList)
        {
            Transform other = otherGO.transform;

            if (other == this) continue;

            if (other.CompareTag("Car"))
            {
                carTrue = true;
                busTrue = false;
                truckTrue = false;

                oCar = other.GetComponent<Car>();
            }
            if (other.CompareTag("Bus"))
            {
                busTrue = true;
                truckTrue = false;
                carTrue = false;

                oBus = other.GetComponent<Bus>();
            }    
            if (other.CompareTag("Truck"))
            {
                truckTrue = true;
                busTrue = false;
                carTrue = false;

                oTruck = other.GetComponent<Truck>();
            }



            // Calculate bounding box using clearances
            float buffer = 0.05f; // small spacing between vehicles

            // Our proposed bounds
            float ourLeft = proposedPosition.x + leftSideClearance;
            float ourRight = proposedPosition.x + rightSideClearance;
            float ourFront = proposedPosition.z + frontClearance;
            float ourBack = proposedPosition.z + backClearance;

            if (busTrue)
            {
                Vector3 otherPos = oBus.transform.position;
                float theirLeft = otherPos.x + oBus.leftSideClearance;
                float theirRight = otherPos.x + oBus.rightSideClearance;
                float theirFront = otherPos.z + oBus.frontClearance;
                float theirBack = otherPos.z + oBus.backClearance;

                bool overlapX = !(ourRight < theirLeft || ourLeft > theirRight);
                bool overlapZ = !(ourFront < theirBack || ourBack > theirFront);

                if (overlapX && overlapZ)
                {
                    return false; // Collision risk!
                }
            }

            if (truckTrue)
            {
                Vector3 otherPos = oTruck.transform.position;
                float theirLeft = otherPos.x + oTruck.leftSideClearance;
                float theirRight = otherPos.x + oTruck.rightSideClearance;
                float theirFront = otherPos.z + oTruck.frontClearance;
                float theirBack = otherPos.z + oTruck.backClearance;

                bool overlapX = !(ourRight < theirLeft || ourLeft > theirRight);
                bool overlapZ = !(ourFront < theirBack || ourBack > theirFront);

                if (overlapX && overlapZ)
                {
                    return false; // Collision risk!
                }
            }

            if (carTrue)
            {
                Vector3 otherPos = oCar.transform.position;
                float theirLeft = otherPos.x + oCar.leftSideClearance;
                float theirRight = otherPos.x + oCar.rightSideClearance;
                float theirFront = otherPos.z + oCar.frontClearance;
                float theirBack = otherPos.z + oCar.backClearance;

                bool overlapX = !(ourRight < theirLeft || ourLeft > theirRight);
                bool overlapZ = !(ourFront < theirBack || ourBack > theirFront);

                if (overlapX && overlapZ)
                {
                    return false; // Collision risk!
                }
            }
        }

        return true;
    }

    void MoveToNextPoint(int index)
    {
        Transform nextPoint = waypoints[index];
        Vector3 direction = (nextPoint.position - transform.position);
        Vector3 proposedPosition = transform.position + direction.normalized * currentSpeed * Time.deltaTime;

        if (CanMoveTo(proposedPosition))
        {
            targetSpeed = speed;
        }
        else
        {
            targetSpeed = 0f;
        }

        ApplySmoothMovement(direction);

        if (Vector3.Distance(transform.position, nextPoint.position) < distanceToNextWaypoint)
        {
            currentIndex++;
        }
    }

    void ApplySmoothMovement(Vector3 direction)
    {
        direction.Normalize();

        // Smooth speed
        if (targetSpeed > currentSpeed)
            currentSpeed += accelerationRate * Time.deltaTime;
        else if (targetSpeed < currentSpeed)
            currentSpeed -= decelerationRate * Time.deltaTime;

        currentSpeed = Mathf.Clamp(currentSpeed, 0, maxSpeed);

        // Steering
        if (direction != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        // Set motor and brake
        motor = currentSpeed > 0 ? maxMotorTorque : 0f;
        brake = currentSpeed == 0 ? brakeForce : 0f;
    }

}