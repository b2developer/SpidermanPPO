using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkingLight : MonoBehaviour
{
    public MeshRenderer redMesh;
    public MeshRenderer greenMesh;

    public Material redMaterial;
    public Material greenMaterial;

    public Material offMaterial;

    void Start()
    {
        
    }

    
    void Update()
    {
        
    }

    public void Red()
    {
        redMesh.material = redMaterial;
        greenMesh.material = offMaterial;
    }

    public void Green()
    {
        greenMesh.material = greenMaterial;
        redMesh.material = offMaterial;
    }
}
