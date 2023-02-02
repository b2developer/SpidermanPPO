using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShiftingWorld : MonoBehaviour
{
    public Transform agent;
    public Vector3 offset = Vector3.zero;

    void Start()
    {
        offset = transform.position;       
    }

    public void Step(float deltaTime)
    {
        transform.position = offset + Vector3.forward * agent.position.z;
    }
}
