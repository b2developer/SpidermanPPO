using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpidermanEnvironment : Environment
{
    public Transform origin;

    public RagdollState ragdollState;

    public RagdollController controller;
    public ShiftingWorld world;

    public bool controlling = true;

    public float FALLING_LIMIT = 2.0f;
    public float DISTANCE_LIMIT = 2.0f;
    public float DISTANCE_SPEED = 0.5f;
    public float MAX_HEIGHT = 120.0f;

    public float currentDistance = 0.0f;
    
    public float REWARD_SCALING = 1.0f;
    public float VELOCITY_SCALING = 0.25f;
    public float VELOCITY_POWER_BONUS = 1.02f;
    public float ATTACH_PENALTY = -1.0f;
    
    public override void Start()
    {
        controller.origin = origin;

        ragdollState = new RagdollState();
        ragdollState.Capture(controller);

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

        controller.SelectAction(timestep.action);

        if (controlling)
        {
            controller.Step(deltaTime);
            world.Step(deltaTime);
        }

        float z = controller.pelvis.transform.position.z - origin.transform.position.z;
        float y = controller.pelvis.transform.position.y - origin.transform.position.y;

        currentDistance = Mathf.Max(z, currentDistance);
        currentDistance += DISTANCE_SPEED * deltaTime;

        //agent has fallen
        if (y < FALLING_LIMIT && controlling)
        {
            Store(timestep, previousReward);
            EndEpisode();
            return;
        }

        //agent is either stuck or progressing too slowly
        if (z < currentDistance - DISTANCE_LIMIT && controlling)
        {
            Store(timestep, previousReward);
            EndEpisode();
            return;
        }

        float width = controller.buildingGap;
        float x = controller.pelvis.transform.position.x - origin.transform.position.x;

        float raise = y / MAX_HEIGHT;
        raise = Mathf.Clamp01(raise);

        float raiseBonus = Mathf.Lerp(0.5f, 1.0f, raise);

        float centerBonus = Mathf.Clamp((width - Mathf.Abs(x)) / width, 0.8f, 1.0f);
        float velocityBonus = Mathf.Max(Mathf.Pow(controller.pelvis.velocity.z, VELOCITY_POWER_BONUS) * VELOCITY_SCALING, 0.0f) * centerBonus;

        float attachPenalty = 0.0f;

        if (controller.newLeft)
        {
            attachPenalty -= ATTACH_PENALTY;

        }

        if (controller.newRight)
        {
            attachPenalty -= ATTACH_PENALTY;
        }

        //total reward is equal to the distance from the start
        episode.totalReward += (velocityBonus * raiseBonus * deltaTime + attachPenalty) * REWARD_SCALING;

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
        currentDistance = origin.transform.position.z;

        ragdollState.Release(controller);

        base.ResetEnvironment();
    }

    public override Matrix StateTensor()
    {
        float x = controller.pelvis.transform.position.x - origin.transform.position.x;
        float y = controller.pelvis.transform.position.y - origin.transform.position.y;
        float z = controller.pelvis.transform.position.y - origin.transform.position.z;

        float vx = controller.pelvis.velocity.x;
        float vy = controller.pelvis.velocity.y;
        float vz = controller.pelvis.velocity.z;

        Vector3 l = -controller.leftElbow.transform.right;
        float dlx = l.x;
        float dly = l.y;
        float dlz = l.z;

        Vector3 r = controller.rightElbow.transform.right;
        float drx = r.x;
        float dry = r.y;
        float drz = r.z;

        float ls = BoolToFloat(controller.leftSpring.spring > 0.0f);
        float rs = BoolToFloat(controller.rightSpring.spring > 0.0f);

        float diff = controller.pelvis.transform.position.z - origin.transform.position.z - currentDistance;

        Vector3 la = Vector3.zero;

        if (controller.leftSpring.spring > 0.0f)
        {
            Vector3 gl = controller.leftElbow.transform.TransformPoint(controller.leftSpring.anchor);
            la = gl - controller.pelvis.transform.position;
        }

        Vector3 ra = Vector3.zero;

        if (controller.rightSpring.spring > 0.0f)
        {
            Vector3 gr = controller.rightElbow.transform.TransformPoint(controller.rightSpring.anchor);
            ra = gr - controller.pelvis.transform.position;
        }

        Matrix tensor = new Matrix(20, 1, 1);

        tensor.values[0, 0, 0] = x;
        tensor.values[1, 0, 0] = y;

        tensor.values[2, 0, 0] = vx;
        tensor.values[3, 0, 0] = vy;
        tensor.values[4, 0, 0] = vz;

        tensor.values[5, 0, 0] = dlx;
        tensor.values[6, 0, 0] = dly;
        tensor.values[7, 0, 0] = dlz;
        tensor.values[8, 0, 0] = drx;
        tensor.values[9, 0, 0] = dry;
        tensor.values[10, 0, 0] = drz;

        tensor.values[11, 0, 0] = ls;
        tensor.values[12, 0, 0] = rs;

        tensor.values[13, 0, 0] = diff;

        tensor.values[14, 0, 0] = la.x;
        tensor.values[15, 0, 0] = la.y;
        tensor.values[16, 0, 0] = la.z;

        tensor.values[17, 0, 0] = ra.x;
        tensor.values[18, 0, 0] = ra.y;
        tensor.values[19, 0, 0] = ra.z;

        return tensor;
    }

    public override PPOAgent.ActionTuple ActionTensor(Matrix state)
    {
        PPOAgent.ActionTuple tuple = agent.SelectAction(ref state);
        return tuple;
    }

    public float BoolToFloat(bool state)
    {
        if (state)
        {
            return 1.0f;
        }

        return -1.0f;
    }
}
