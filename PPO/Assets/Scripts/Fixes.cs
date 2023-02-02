using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//allows the reflections to take on one skybox while we dynamically set another with a better floor
public class Fixes : MonoBehaviour
{
    public Material skybox;

    void Start()
    {
        RenderSettings.skybox = skybox;
    }

    void Update()
    {
        
    }
}
