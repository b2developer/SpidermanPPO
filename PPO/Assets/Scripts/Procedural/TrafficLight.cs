using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficLight : MonoBehaviour
{
    public MeshRenderer[] lightMeshes;
    public Material[] lightMaterials;
    public Material offMaterial;

    public float delayTimer = 0.0f;
    public float switchDelay = 3.5f;

    void Start()
    {
        
    }

    void Update()
    {
        if (delayTimer > 0.0f)
        {
            delayTimer -= Time.deltaTime;

            if (delayTimer <= 0.0f)
            {
                SetLights(0);
            }
            else
            {
                SetLights(1);
            }    
        }
    }

    public void Red()
    {
        delayTimer = switchDelay;
    }

    public void Green()
    {
        SetLights(2);
    }

    public void SetLights(int index)
    {
        for (int i = 0; i < 3; i++)
        {
            if (i == index)
            {
                lightMeshes[i].material = lightMaterials[i];
            }
            else
            {
                lightMeshes[i].material = offMaterial;
            }
        }
    }
}
