using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//tuple for storing the environment and agent's data
public class Timestep
{
    public Matrix state = null;
    public Matrix action = null;
    public float reward = 0.0f;

    public Matrix e; //baseline samples
    public Matrix logProb;

    public Timestep()
    {

    }
}

//tuple for storing a more comprehensive version of a timestep for back propagation
public class DetailedTimestep
{
    public Matrix state = null;
    public Matrix action = null;
    public float reward = 0.0f;

    public float value = 0.0f; //V(St) estimate
    public float returns = 0.0f; //Gt
    public float advantage = 0.0f; //At

    public float delta = 0.0f;
    public float gae = 0.0f; //general advantage estimate

    public Matrix e; //baseline samples
    public Matrix logProb;

    public DetailedTimestep()
    {

    }
}
