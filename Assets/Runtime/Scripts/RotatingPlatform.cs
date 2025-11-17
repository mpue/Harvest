using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingPlatform : MonoBehaviour
{
    public float speed = 1;

    public Vector3 rotationAxis = Vector3.up;

    void Update()
    {
        rotationAxis = rotationAxis.normalized;
        rotationAxis *= Time.deltaTime * speed;
        transform.Rotate(rotationAxis);
    }
}
