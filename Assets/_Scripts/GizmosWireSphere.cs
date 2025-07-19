using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmosWireSphere : MonoBehaviour
{
    public float approxDistance = 6.25f;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, approxDistance);
    }
}
