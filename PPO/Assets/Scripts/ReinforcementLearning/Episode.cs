using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//stores all data for the episode
public class Episode
{
    public int length = 0;
    public float totalReward = 0.0f;

    public List<Timestep> samples;

    public List<float> values; //V(st) estimances

    public List<float> returns; //Gt
    public List<float> advantages; //At

    public Episode()
    {
        samples = new List<Timestep>();

        values = new List<float>();

        returns = new List<float>();
        advantages = new List<float>();
    }
}