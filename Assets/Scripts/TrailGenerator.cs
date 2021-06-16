using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailGenerator : MonoBehaviour
{
    [SerializeField]
    private string tag;
    [SerializeField]
    private GameObject wallPrefab;
    [SerializeField]
    private float threshold;

    private Vector3 initialScale;
    private Vector3 prevPos;

    public void onRestart()
    {
        initialScale = wallPrefab.transform.localScale;
        prevPos = transform.position;
        InvokeRepeating("CalculateDistance", 0, 0.01f);
    }

    void CalculateDistance()
    {
        Vector3 currentPos = transform.position - 3 * transform.forward;

        Vector3 delta = currentPos - prevPos;
        float distance = delta.magnitude;
        if (distance > threshold)
        {
            GameObject wall = Instantiate(wallPrefab, transform.position - 6 * transform.forward, Quaternion.LookRotation(delta));
            wall.tag = tag;
            wall.transform.localScale = new Vector3(initialScale.x, initialScale.y, distance + 0.05f);
        }
        prevPos = currentPos;
    }
}
