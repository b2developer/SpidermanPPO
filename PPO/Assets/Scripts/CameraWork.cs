using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraWork : MonoBehaviour
{
    public Transform target;

    public float backDistance = 10.0f;
    public float upDistance = 3.0f;
    public float sideDistance = 0.0f;

    void Start()
    {
        
    }

    void Update()
    {
        transform.position = target.position + Vector3.back * backDistance + Vector3.up * upDistance + Vector3.right * sideDistance;

        Vector3 relative = target.position - transform.position;

        //transform.rotation = Quaternion.LookRotation(relative.normalized);
    }
}
