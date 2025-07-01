using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AxleInfo
{
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public GameObject leftWheelGO;
    public GameObject rightWheelGO;
    public bool motor;
    public bool steering;
}

public class Car : MonoBehaviour
{
    #region SimStuff
    [Header("SimStuff")]
    public Transform model;
    public enum carType
    {
        taxi,
        sedan,
        suv,
        van,
        ute,
        sport,
        rozzas
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
        switch (currentType)
        {
            case carType.taxi:
                leftSide = 0.01086f;
                rightSide = -0.01086f;
                frontSide = 0.02144f;
                backSide = -0.02054f;
            break;


            case carType.sedan:
                leftSide = 0.01051f;
                rightSide = -0.01051f;
                frontSide = 0.0264f;
                backSide = -0.02628f;
            break;

            case carType.suv:
                leftSide = 0.01068f;
                rightSide = -0.01068f;
                frontSide = 0.02262f;
                backSide = -0.02288f;
            break;

            case carType.van:
                leftSide = 0.01104f;
                rightSide = -0.01104f;
                frontSide = 0.02327f;
                backSide = -0.02404f;
            break;

            case carType.ute:
                leftSide = 0.00961f;
                rightSide = -0.00961f;
                frontSide = 0.02519f;
                backSide = -0.02525f;
            break;

            case carType.sport:
                leftSide = 0.01069f;
                rightSide = -0.01069f;
                frontSide = 0.02292f;
                backSide = -0.0229f;
            break;

            case carType.rozzas:
                leftSide = 0.00976f;
                rightSide = -0.00976f;
                frontSide = 0.02692f;
                backSide = -0.0215f;
            break;
        }

        leftSideClearance = model.position.x + leftSide + .005f;
        rightSideClearance = model.position.x + rightSide + .005f;
        frontClearance = model.position.y + frontSide + .001f;
        backClearance = model.position.y + backSide;
    }
    #endregion



    [Header("MakeCarWorkInterestingThings")]
    public AxleInfo[] axleInfo;
    public float maxMotorTorque = 500f, maxSteeringAngle = 65f, brakeTorque = 1000, deceleration = 800;   

    [Header("Control")]
    public float motorInput;
    public float steeringInput;
    public bool brakeInput;

    void Update()
    {
        motorInput = Input.GetAxis("Vertical");
        steeringInput = Input.GetAxis("Horizontal");
        brakeInput = Input.GetKey(KeyCode.Space);
    }

    void FixedUpdate()
    {
        float motor = maxMotorTorque * motorInput;
        float steering = maxSteeringAngle * steeringInput;

        for (int i = 0; i < axleInfo.Length; i++)
        {
            if (brakeInput == true)
            {
                axleInfo[i].leftWheel.brakeTorque = brakeTorque;
                axleInfo[i].rightWheel.brakeTorque = brakeTorque;
            }
            else
            {
                axleInfo[i].leftWheel.brakeTorque = 0;
                axleInfo[i].rightWheel.brakeTorque = 0;
            }
            
            if (axleInfo[i].steering)
            {
                axleInfo[i].leftWheel.steerAngle = steering;
                axleInfo[i].rightWheel.steerAngle = steering;
            }

            if (axleInfo[i].motor)
            {
                if (brakeInput == true)
                {
                    axleInfo[i].leftWheel.brakeTorque = brakeTorque;
                    axleInfo[i].rightWheel.brakeTorque = brakeTorque;
                }
                else
                {
                    axleInfo[i].leftWheel.brakeTorque = 0;
                    axleInfo[i].rightWheel.brakeTorque = 0;
                }

                if (motorInput == 0)
                {
                    axleInfo[i].leftWheel.brakeTorque = deceleration;
                    axleInfo[i].rightWheel.brakeTorque = deceleration;
                }
                else
                {
                    axleInfo[i].leftWheel.motorTorque = motor;
                    axleInfo[i].rightWheel.motorTorque = motor;
                }

            }

            ApplyPositionToVisuals(axleInfo[i].leftWheel, axleInfo[i].leftWheelGO);
            ApplyPositionToVisuals(axleInfo[i].rightWheel, axleInfo[i].rightWheelGO);
        }
    }


    void ApplyPositionToVisuals(WheelCollider collider, GameObject wheelGO)
    {
        if (wheelGO == null) return;

        Vector3 tempPosition;
        Quaternion tempRotation;

        collider.GetWorldPose(out tempPosition, out tempRotation);

        wheelGO.transform.position = tempPosition;
        wheelGO.transform.rotation = tempRotation;

    }
}
