using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ShotPoint : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Color color = Color.yellow;
        color.a = 0.8f;
        Gizmos.color = color;
        //Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(transform.position, transform.localScale);
        Gizmos.matrix = Matrix4x4.identity;
    }
}
