using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyState
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 velocity;
    public Vector3 angularVelocity;

    public RigidbodyState()
    {

    }
}

public class RagdollState
{
    public float leftSpringValue = 0.0f;
    public float rightSpringValue = 0.0f;

    public float leftDamperValue = 0.0f;
    public float rightDamperValue = 0.0f;

    public Vector3 leftAnchor = Vector3.zero;
    public Vector3 rightAnchor = Vector3.zero;

    public Vector3 targetLeft = Vector3.zero;
    public Vector3 targetRight = Vector3.zero;

    public bool castLeft = false;
    public bool castRight = false;

    public List<RigidbodyState> states;

    public bool newLeft = false;
    public bool newRight = false;

    public float leftRecharge = 0.0f;
    public float rightRecharge = 0.0f;

    public float leftProgress = 0.0f;
    public float rightProgress = 0.0f;

    public RagdollState()
    {
        states = new List<RigidbodyState>();
    }

    public void Capture(RagdollController controller)
    {
        leftSpringValue = controller.leftSpring.spring;
        rightSpringValue = controller.rightSpring.spring;

        leftDamperValue = controller.leftSpring.damper;
        rightDamperValue = controller.rightSpring.damper;

        leftAnchor = controller.leftSpring.connectedAnchor;
        rightAnchor = controller.rightSpring.connectedAnchor;

        targetLeft = controller.targetLeft;
        targetRight = controller.targetRight;
        castLeft = controller.castLeft;
        castRight = controller.castRight;

        states = new List<RigidbodyState>();

        Rigidbody[] bodyList = controller.GetComponentsInChildren<Rigidbody>();

        for (int i = 0; i < bodyList.GetLength(0); i++)
        {
            RigidbodyState state = new RigidbodyState();

            state.position = bodyList[i].transform.position;
            state.rotation = bodyList[i].transform.rotation;
            state.velocity = bodyList[i].velocity;
            state.angularVelocity = bodyList[i].angularVelocity;

            states.Add(state);
        }

        newLeft = controller.newLeft;
        newRight = controller.newRight;

        leftRecharge = controller.leftRecharge;
        rightRecharge = controller.rightRecharge;

        leftProgress = controller.leftProgress;
        rightProgress = controller.rightProgress;
    }

    public void Release(RagdollController controller)
    {
        controller.leftSpring.spring = leftSpringValue;
        controller.rightSpring.spring = rightSpringValue;

        controller.leftSpring.damper = leftDamperValue;
        controller.rightSpring.damper = rightDamperValue;

        controller.leftSpring.connectedAnchor = leftAnchor;
        controller.rightSpring.connectedAnchor = rightAnchor;

        controller.targetLeft = targetLeft;
        controller.targetRight = targetRight;
        controller.castRight = castRight;
        controller.castLeft = castLeft;

        Rigidbody[] bodyList = controller.GetComponentsInChildren<Rigidbody>();

        for (int i = 0; i < bodyList.GetLength(0); i++)
        {
            bodyList[i].transform.position = states[i].position;
            bodyList[i].transform.rotation = states[i].rotation;
            bodyList[i].velocity = states[i].velocity;
            bodyList[i].angularVelocity = states[i].angularVelocity;
        }

        controller.newLeft = newLeft;
        controller.newRight = newRight;

        controller.leftRecharge = leftRecharge;
        controller.rightRecharge = rightRecharge;

        controller.leftProgress = leftProgress;
        controller.rightProgress = rightProgress;
    }
}
