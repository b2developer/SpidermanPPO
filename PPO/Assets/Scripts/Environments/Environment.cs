using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment : MonoBehaviour
{
    public delegate void EpisodeFunc(Episode value);

    public PPOAgent agent;
    public Episode episode;

    public float reward = 0.0f;
    public bool isRunning = true;

    public EpisodeFunc episodeEndCallback;

    public virtual void Start()
    {

    }

    public virtual void Update()
    {

    }

    public virtual void Step(float deltaTime)
    {

    }

    public virtual void EndEpisode()
    {
        agent.EpisodeCompleted(episode);

        reward = 0.0f;
        isRunning = false;
    }

    public virtual void ResetEnvironment()
    {
        episode = new Episode();

        reward = 0.0f;
        isRunning = true;
    }

    public virtual Matrix StateTensor()
    {
        return new Matrix(0, 0, 0);
    }

    public virtual PPOAgent.ActionTuple ActionTensor(Matrix state)
    {
        return null;
    }
}
