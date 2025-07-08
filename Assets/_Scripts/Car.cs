using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour
{
    #region SimStuff
    [Header("SimStuff")]
    public Transform model;

    public enum carType
    {
        taxi, sedan, suv, van, ute, sport, rozzas
    }
    public carType currentType;

    [Header("Clearances")]
    public float leftSideClearance;
    public float rightSideClearance, frontClearance, backClearance;

    [Header("Sides")]
    [SerializeField] private float leftSide;
    [SerializeField] private float rightSide, frontSide, backSide;

    private void Awake()
    {
        int rando = Random.Range(0,15);

        switch (rando)
        {
            case 0:
                disabled = true;
                break;

            default:
                disabled = false;
                break;
        }


        // Set clearances based on vehicle type
        switch (currentType)
        {
            case carType.taxi:
                SetSides(0.02172f, 0.04288f, -0.04108f); break;
            case carType.sedan:
                SetSides(0.02102f, 0.0528f, -0.05256f); break;
            case carType.suv:
                SetSides(0.02136f, 0.04524f, -0.04576f); break;
            case carType.van:
                SetSides(0.02208f, 0.04654f, -0.04808f); break;
            case carType.ute:
                SetSides(0.01922f, 0.05038f, -0.05050f); break;
            case carType.sport:
                SetSides(0.02138f, 0.04584f, -0.0438f); break;
            case carType.rozzas:
                SetSides(0.01952f, 0.05384f, -0.043f); break;
        }

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

    public bool disabled;
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


    [SerializeField] private Transform chosenPark = null;

    void Start()
    {
        //Gets the parking arrays so they are only stored in one object
        pA = GameObject.FindWithTag("parkingTag").GetComponent<ParkingArray>();
        waypoints = pA.parkingWaypoints;

        firstPark = true;

        cS = GameObject.FindWithTag("spawnerTag").GetComponent<CarSpawner>();
        cS.vehicles.Add(this.gameObject);
        vehicleList = cS.vehicles;

        //Checks if car is disabled and checks what carparks are available for that carType
        List<GameObject> spotList = disabled ? pA.availableDisabled : pA.availableStandard;

        shortestDistance = Mathf.Infinity;

        //Checks available spots to find the closest
        foreach (GameObject s in spotList)
        {
            float newDistance = Vector3.Distance(transform.position, s.transform.position);

            if (firstPark || newDistance < shortestDistance)
            {
                shortestDistance = newDistance;
                chosenPark = s.transform;
                firstPark = false;
            }
        }

        //Fills out closest spot and initiates movement
        if (chosenPark != null)
        {
            currentIndex = 0;
            movingToPark = true;
        }
    }


    void Update()
    {
        //Check if moving
        if (!movingToPark) return;

        //If not at max waypoint
        if (currentIndex < waypoints.Count)
        {
            MoveToNextPoint(currentIndex);
        }
        else
        {
            //must near spot so park
            MoveToParkingSpot();
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



    void MoveToParkingSpot()
    {
        Vector3 direction = (chosenPark.position - transform.position);
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

        float dist = Vector3.Distance(transform.position, chosenPark.position);
        float angleDiff = Quaternion.Angle(transform.rotation, chosenPark.rotation);

        if (dist < 0.2f && angleDiff < 5f)
        {
            if (disabled)
                pA.availableDisabled.Remove(chosenPark.gameObject);
            else
                pA.availableStandard.Remove(chosenPark.gameObject);

            this.enabled = false;
        }
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