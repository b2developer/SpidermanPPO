using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CartPoleEnvironment : Environment
{
    public Vector3 basePosition;
    public Vector3 polePosition;
    public Quaternion poleRotation;

    public Rigidbody baseBody;
    public Rigidbody poleBody;

    public float REWARD_MIN = 0.0f;
    public float REWARD_MAX = 5.0f;

    public float POLE_HEIGHT = 2.0f;
    public float POLE_DEATH = 0.5f;
    public float MAX_SPAN = 5.0f;
    public float SPEED_MAX = 1.5f;

    public override void Start()
    {
        ResetEnvironment();
    }

    public override void Update()
    {

    }

    public override void Step(float deltaTime)
    {
        if (!isRunning)
        {
            return;
        }

        float previousReward = episode.totalReward;

        //---------------------------------------

        Timestep timestep = new Timestep();

        timestep.state = StateTensor();
        PPOAgent.ActionTuple tuple = ActionTensor(timestep.state);

        timestep.action = tuple.action;
        timestep.e = tuple.e;
        timestep.logProb = tuple.logProb;

        float control = Mathf.Clamp(timestep.action.values[0, 0, 0], -1.0f, 1.0f) * SPEED_MAX;

        baseBody.transform.position += Vector3.right * control * deltaTime;

        float xClamp = Mathf.Clamp(baseBody.transform.position.x, -MAX_SPAN, MAX_SPAN);
        baseBody.transform.position = new Vector3(xClamp, baseBody.transform.position.y, baseBody.transform.position.z);

        if (poleBody.transform.position.y < POLE_DEATH)
        {
            episode.totalReward -= 1.0f * 10.0f;

            Store(timestep, previousReward);

            EndEpisode();
            return;
        }

        episode.totalReward += 0.1f;

        Store(timestep, previousReward);

        base.Step(deltaTime);
    }

    public void Store(Timestep timestep, float previousReward)
    {
        episode.length++;

        reward = episode.totalReward - previousReward;
        timestep.reward = reward;

        episode.samples.Add(timestep);

        float v = agent.Value(timestep.state);
        episode.values.Add(v);
    }

    public override void EndEpisode()
    {
        base.EndEpisode();
    }

    public override void ResetEnvironment()
    {
        baseBody.transform.position = basePosition;

        poleBody.transform.position = polePosition;
        poleBody.transform.rotation = poleRotation;
        poleBody.velocity = Vector3.zero;
        poleBody.angularVelocity = Vector3.zero;

        base.ResetEnvironment();
    }

    public override Matrix StateTensor()
    {
        Matrix tensor = new Matrix(3, 1, 1);

        tensor.values[0, 0, 0] = (float)System.Math.Tanh(baseBody.transform.position.x / MAX_SPAN);
        tensor.values[1, 0, 0] = (float)System.Math.Tanh(poleBody.transform.position.y / POLE_HEIGHT);
        tensor.values[2, 0, 0] = (float)System.Math.Tanh(poleBody.transform.eulerAngles.z / 90.0f);

        return tensor;
    }

    public override PPOAgent.ActionTuple ActionTensor(Matrix state)
    {
        PPOAgent.ActionTuple tuple = agent.SelectAction(ref state);
        return tuple;
    }
}