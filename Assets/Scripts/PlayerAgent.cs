using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using UnityEngine;

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
    private Transform groundPlane;

    private Rigidbody rigidBody;

    public override void Initialize()
    {
        rigidBody = GetComponent<Rigidbody>();
        // crashPrefab.GetComponent<ParticleSystem>().collision.SetPlane(1, groundPlane);
    }

    // private float agentRunSpeed = 2f;

    public override void OnActionReceived(ActionBuffers actions)
    {
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
        float forwardSpeed = Vector3.Dot(rigidBody.velocity, transform.forward);
        float sidewaysSpeed = Vector3.Dot(rigidBody.velocity, transform.right);
        float rotateSpeed = rigidBody.angularVelocity.y;
        rigidBody.AddForce(-transform.right * sidewaysSpeed, ForceMode.Force);

        model.localRotation = Quaternion.Euler(0, 0, -rotateSpeed * forwardSpeed * tiltAmount);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Ground"))
        {
            Instantiate(crashPrefab, transform.position, transform.rotation);
        }
    }
}
