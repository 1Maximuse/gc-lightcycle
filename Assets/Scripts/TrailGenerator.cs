using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailGenerator : MonoBehaviour
{
    private Vector3 prevPos;

    void Start()
    {
        prevPos = transform.position;
        InvokeRepeating("CalculateDistance", 0, 0.1f);
    }

    void CalculateDistance()
    {
        Vector3 currentPos = transform.position;

        Debug.Log((currentPos - prevPos).sqrMagnitude);

        prevPos = currentPos;
    }
}
