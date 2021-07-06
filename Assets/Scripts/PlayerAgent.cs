using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using UnityEngine;
using System.Linq;
using Unity.MLAgents.Sensors;
using UnityEngine.Audio;

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
    private float minSpeed;
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
    [SerializeField]
    private AudioMixer audioMixer;
    [SerializeField]
    private bool remote;

    private Rigidbody rigidBody;

    private Vector3 initialLocalPosition;
    private Quaternion initialRotation;

    private bool alive;

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        initialLocalPosition = transform.localPosition;
        initialRotation = transform.rotation;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.rotation * (enemy.position - transform.position));
    }

    public override void OnEpisodeBegin()
    {
        if (environment.isTraining)
        {
            float spawnRadius = 200f;
            Quaternion rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            Vector3 position = rotation * -Vector3.forward * spawnRadius;
            transform.rotation = rotation;
            transform.localPosition = position;
        } else
        {
            transform.localPosition = initialLocalPosition;
            transform.rotation = initialRotation;
        }
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
        else if (act[0] == 2 && forwardSpeed > minSpeed) // Brake
            rigidBody.AddForce(-transform.forward * brakeSpeed, ForceMode.Force);

        if (act[1] == 1) // Left
            rigidBody.AddTorque(transform.up * -turnSpeed);
        else if (act[1] == 2) // Right
            rigidBody.AddTorque(transform.up * turnSpeed);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        if (remote)
        {
            discreteActions[0] = Input.GetKey(KeyCode.I) ? 1 : (Input.GetKey(KeyCode.K) ? 2 : 0);
            discreteActions[1] = Input.GetKey(KeyCode.J) ? 1 : (Input.GetKey(KeyCode.L) ? 2 : 0);
        }
        else
        {
            discreteActions[0] = Input.GetKey(KeyCode.W) ? 1 : (Input.GetKey(KeyCode.S) ? 2 : 0);
            discreteActions[1] = Input.GetKey(KeyCode.A) ? 1 : (Input.GetKey(KeyCode.D) ? 2 : 0);
        }
    }

    void Update()
    {
        if (alive) AddReward(+1f);
        float forwardSpeed = Vector3.Dot(rigidBody.velocity, transform.forward);
        float sidewaysSpeed = Vector3.Dot(rigidBody.velocity, transform.right);
        float rotateSpeed = rigidBody.angularVelocity.y;
        rigidBody.AddForce(-transform.right * sidewaysSpeed * 4f, ForceMode.Force);
        rigidBody.AddForce(transform.forward * Mathf.Max(0, minSpeed - forwardSpeed), ForceMode.Force);

        model.localRotation = Quaternion.Euler(0, 0, -rotateSpeed * forwardSpeed * tiltAmount);
        float inverseLerp = Mathf.InverseLerp(minSpeed, maxSpeed, forwardSpeed);
        if (gameObject.CompareTag("Yellow"))
        {
            audioMixer.SetFloat("AccelerationPitch", Mathf.Lerp(0.5f, 1.5f, inverseLerp));
            audioMixer.SetFloat("AccelerationVolume", Mathf.Lerp(-40f, -20f, inverseLerp));
        } else
        {
            audioMixer.SetFloat("BluePitch", Mathf.Lerp(0.5f, 1.5f, inverseLerp));
            audioMixer.SetFloat("BlueVolume", Mathf.Lerp(-40f, -20f, inverseLerp));
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground")) return;

        Instantiate(crashPrefab, transform.position, transform.rotation);
        alive = false;

        if (gameObject.CompareTag(collision.gameObject.tag))
        {
            AddReward(-1000f);
            if (gameObject.CompareTag("Yellow"))
            {
                environment.yellowCrash();
            } 
            else if (gameObject.CompareTag("Blue"))
            {
                environment.blueCrash();
            }
        }
        else
        {
            if (collision.gameObject.CompareTag("Yellow"))
            {
                AddReward(-500f);
                environment.addYellowReward();
                environment.blueCrash();
            }
            else if (collision.gameObject.CompareTag("Blue"))
            {
                AddReward(-500f);
                environment.addBlueReward();
                environment.yellowCrash();
            }
            else
            {
                // Wall
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