using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using UnityEngine;
using System.Linq;
using Unity.MLAgents.Sensors;

public class PlayerAgent : Agent
{
    [SerializeField]
    private float accelerateSpeed;
    [SerializeField]
    private float brakeSpeed;
    [SerializeField]
    private float turnSpeed;
    [SerializeField]
    private float maxSpeed;
    [SerializeField]
    private float tiltAmount;
    [SerializeField]
    private Transform model;
    [SerializeField]
    private GameObject crashPrefab;
    [SerializeField]
    private Environment environment;
    [SerializeField]
    private Transform enemy;

    private Rigidbody rigidBody;

    private Vector3 initialLocalPosition;
    private Quaternion initialRotation;

    private bool alive;

    void Awake()
    {
        initialLocalPosition = transform.localPosition;
        initialRotation = transform.rotation;
    }

    public override void Initialize()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(enemy.localPosition);
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = initialLocalPosition;
        transform.rotation = initialRotation;
        rigidBody.velocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;
        alive = true;
        GetComponent<TrailGenerator>().onRestart();
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (!alive) return;

        ActionSegment<int> act = actions.DiscreteActions;
        Vector3 force = Vector3.zero;
        Vector3 rotateDir = Vector3.zero;

        float forwardSpeed = Vector3.Dot(rigidBody.velocity, transform.forward);
        if (act[0] == 1 && forwardSpeed < maxSpeed) // Accelerate
            rigidBody.AddForce(transform.forward * accelerateSpeed, ForceMode.Force);
        else if (act[0] == 2 && forwardSpeed > 0) // Brake
            rigidBody.AddForce(-transform.forward * brakeSpeed, ForceMode.Force);

        if (act[1] == 1) // Left
            rigidBody.AddTorque(transform.up * -turnSpeed);
        else if (act[1] == 2) // Right
            rigidBody.AddTorque(transform.up * turnSpeed);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = Input.GetKey(KeyCode.W) ? 1 : (Input.GetKey(KeyCode.S) ? 2 : 0);
        discreteActions[1] = Input.GetKey(KeyCode.A) ? 1 : (Input.GetKey(KeyCode.D) ? 2 : 0);
    }

    void Update()
    {
        if (alive) AddReward(+1f);
        float forwardSpeed = Vector3.Dot(rigidBody.velocity, transform.forward);
        float sidewaysSpeed = Vector3.Dot(rigidBody.velocity, transform.right);
        float rotateSpeed = rigidBody.angularVelocity.y;
        rigidBody.AddForce(-transform.right * sidewaysSpeed, ForceMode.Force);

        model.localRotation = Quaternion.Euler(0, 0, -rotateSpeed * forwardSpeed * tiltAmount);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground")) return;

        Instantiate(crashPrefab, transform.position, transform.rotation);
        alive = false;

        if (gameObject.CompareTag(collision.gameObject.tag))
        {
            AddReward(-1000f);
            if (collision.gameObject.CompareTag("Yellow"))
            {
                environment.yellowCrash();
            } 
            else if (collision.gameObject.CompareTag("Blue"))
            {
                environment.blueCrash();
            }
        }
        else
        {
            AddReward(-500f);
            if (collision.gameObject.CompareTag("Yellow"))
            {
                environment.addYellowReward();
                environment.blueCrash();
            }
            else if (collision.gameObject.CompareTag("Blue"))
            {
                environment.addBlueReward();
                environment.yellowCrash();
            }
            else
            {
                if (gameObject.CompareTag("Yellow"))
                {
                    environment.yellowCrash();
                }
                else if (gameObject.CompareTag("Blue"))
                {
                    environment.blueCrash();
                }
            }
        }
    }
}