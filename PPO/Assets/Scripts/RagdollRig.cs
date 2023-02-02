using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollRig : MonoBehaviour
{
    public Transform[] locals;
    public Transform[] references;

    public int count = 0;

    public bool selfUpdate = false;

    void Start()
    {
        count = locals.GetLength(0);
    }

    public void Update()
    {
        if (selfUpdate)
        {
            UpdateRig();
        }
    }

    public void UpdateRig()
    {
        for (int i = 0; i < count; i++)
        {
            locals[i].position = references[i].position;
            locals[i].rotation = references[i].rotation;
        }
    }
}
