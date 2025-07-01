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

    private void Start()
    {
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
        leftSideClearance = model.position.x + leftSide + 0.01f;
        rightSideClearance = model.position.x + rightSide - 0.01f;
        frontClearance = model.position.z + frontSide + 0.002f;
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

    void Update()
    {
        // Input
        motor = maxMotorTorque * Input.GetAxis("Vertical");
        steer = maxSteerAngle * Input.GetAxis("Horizontal");
        brake = Input.GetKey(KeyCode.Space) ? brakeForce : 0f;
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
}
