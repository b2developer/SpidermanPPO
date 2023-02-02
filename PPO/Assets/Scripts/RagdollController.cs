using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
* allows the AI to control the ragdoll
* the limbs can be controlled by specifying a direction to point in
* webs are created by dynamically turning joints on the arms on and off
*/
public class RagdollController : MonoBehaviour
{
    public delegate void Func();

    public Transform origin;
    public RagdollRig rig;

    public float buildingGap = 53.0f;
    public float buildingHeight = 120.0f;
    public float springStrength = 200.0f;
    public float maxAttachmentDistance = 140.0f;
    public float retractionSpeed = 3.0f;
    public float damper = 5.0f;
    
    public Rigidbody pelvis;

    public Rigidbody leftElbow;
    public Rigidbody rightElbow;

    public SpringJoint leftSpring;
    public SpringJoint rightSpring;

    public LineRenderer leftLine;
    public LineRenderer rightLine;

    public Vector3 targetLeft = Vector3.zero;
    public Vector3 targetRight = Vector3.zero;

    public bool castLeft = false;
    public bool castRight = false;

    public bool newLeft = false;
    public bool newRight = false;

    //animation variables
    public float rechargeTime = 0.25f;
    public float leftRecharge = 0.0f;
    public float rightRecharge = 0.0f;

    //1 / 10th of a second
    public float progressSpeed = 10.0f;
    public float leftProgress = 0.0f;
    public float rightProgress = 0.0f;

    public Matrix action;

    public Func onWebShotLeft;
    public Func onWebShotRight;

    void Start()
    {
        
    }

    void Update()
    {
        RenderLine(leftSpring, leftElbow, leftLine, ref leftProgress);
        RenderLine(rightSpring, rightElbow, rightLine, ref rightProgress);
        rig.UpdateRig();
    }

    public void SelectAction(Matrix _action)
    {
        action = new Matrix(_action.values);
    }

    public void Step(float deltaTime)
    {
        Matrix actionFinal = new Matrix(action.values);
        actionFinal.Clip(-1.0f, 1.0f);

        newLeft = false;
        newRight = false;

        float yawLeft = actionFinal.values[0, 0, 0];
        float pitchLeft = actionFinal.values[1, 0, 0];

        Vector3 dl = AnglesToDirection(yawLeft, pitchLeft);

        float yawRight = actionFinal.values[2, 0, 0];
        float pitchRight = actionFinal.values[3, 0, 0];

        Vector3 dr = AnglesToDirection(yawRight, pitchRight);

        targetLeft = dl;
        targetRight = dr;

        castLeft = FloatToBool(actionFinal.values[4, 0, 0]);
        castRight = FloatToBool(actionFinal.values[5, 0, 0]);

        ElbowImpulse(leftElbow, pelvis, targetLeft, 25.0f, 5.0f);
        ElbowImpulse(rightElbow, pelvis, targetRight, 25.0f, 5.0f);

        if (leftRecharge > 0.0f)
        {
            leftRecharge -= deltaTime;
        }

        if (castLeft)
        {
            if (leftSpring.spring == 0.0f && leftRecharge <= 0.0f)
            {
                newLeft = AttachSpring(leftSpring, leftElbow, -leftElbow.transform.right);
                leftProgress = 0.0f;
            }
        }
        else
        {
            //was >=
            if (leftSpring.spring > 0.0f)
            {
                DeattachSpring(leftSpring);
                leftRecharge = rechargeTime;
            }
        }

        if (rightRecharge > 0.0f)
        {
            rightRecharge -= deltaTime;
        }

        if (castRight)
        {
            if (rightSpring.spring == 0.0f && rightRecharge <= 0.0f)
            {
                newRight = AttachSpring(rightSpring, rightElbow, rightElbow.transform.right);
                rightProgress = 0.0f;
            }
        }
        else
        {
            //was >=
            if (rightSpring.spring > 0.0f)
            {
                DeattachSpring(rightSpring);
                rightRecharge = rechargeTime;
            }
        }

        RetractSpringImpulse(leftSpring, deltaTime);
        RetractSpringImpulse(rightSpring, deltaTime);

        if (newLeft && onWebShotLeft != null)
        {
            onWebShotLeft();
        }

        if (newRight && onWebShotRight != null)
        {
            onWebShotRight();
        }
    }

    //points elbow in a specific direction using simple PID algorithm, applies opposing force to pelvis (COM) to stop the arm accelerating the body
    public void ElbowImpulse(Rigidbody elbow, Rigidbody pelvis, Vector3 target, float P, float D)
    {
        Vector3 tn = Vector3.zero;

        if (target != Vector3.zero)
        {
            tn = target.normalized;
        }

        Vector3 EP = tn;
        Vector3 EV = pelvis.velocity - elbow.velocity;

        Vector3 PID = EP * P + EV * D;

        elbow.AddForce(PID, ForceMode.Force);
        pelvis.AddForce(-PID, ForceMode.Force);
    }

    public bool AttachSpring(SpringJoint joint, Rigidbody elbow, Vector3 direction)
    {
        joint.spring = 200.0f;

        //determine where the spring attached
        float xzStep = Mathf.Abs(direction.x) + Mathf.Abs(direction.z);

        //the web is pointed nearly or completely vertically and therefore should be cancelled
        if (xzStep < 0.001f)
        {
            joint.spring = 0.0f;
            return false;
        }

        float gap = buildingGap * 0.5f - (elbow.position.x - origin.transform.position.x);

        if (direction.x < 0.0f)
        {
            gap = buildingGap * -0.5f - (elbow.position.x - origin.transform.position.x);
        }

        float xStep = direction.x;

        //how long will the spring attachment take to reach it's target on the wall
        float steps = gap / xStep;

        Vector3 position = elbow.transform.TransformPoint(joint.anchor);

        Vector3 attachment = position + direction * steps;

        //attachment is too low or high
        if (attachment.y < 0.0f || attachment.y > buildingHeight)
        {
            joint.spring = 0.0f;
            return false;
        }

        Vector3 relative = attachment - position;
        float length = relative.magnitude;

        //attachment is too far away
        if (length > maxAttachmentDistance)
        {
            joint.spring = 0.0f;
            return false;
        }

        //all conditions were met, create the attachment
        joint.spring = springStrength;
        joint.damper = damper;
        joint.connectedAnchor = attachment;
        joint.minDistance = length;
        joint.maxDistance = 0.0f;

        return true;
    }

    public void DeattachSpring(SpringJoint joint)
    {
        joint.spring = 0.0f;
        joint.damper = 0.0f;
    }

    public void RetractSpringImpulse(SpringJoint joint, float deltaTime)
    {
        if (joint.minDistance > 0.0f)
        {
            joint.minDistance -= retractionSpeed * deltaTime;
        }
    }

    public void RenderLine(SpringJoint joint, Rigidbody elbow, LineRenderer lineRenderer, ref float lerp)
    {
        if (joint.spring <= 0.0f)
        {
            lineRenderer.positionCount = 0;
            return;
        }

        Vector3 anchor = elbow.transform.TransformPoint(joint.anchor);
        Vector3 attachedAnchor = joint.connectedAnchor;

        lerp += progressSpeed * Time.deltaTime;
        lerp = Mathf.Clamp01(lerp);

        Vector3 lerpedAnchor = Vector3.Lerp(anchor, attachedAnchor, lerp);

        lineRenderer.positionCount = 2;
        //lineRenderer.SetPositions(new Vector3[2] { anchor, attachedAnchor });
        lineRenderer.SetPositions(new Vector3[2] { anchor, lerpedAnchor });
    }

    public bool FloatToBool(float value)
    {
        return value > 0.0f;
    }

    //inputs are from -1 to 1
    public Vector3 AnglesToDirection(float yaw, float pitch)
    {
        yaw *= Mathf.PI;
        pitch *= Mathf.PI * 0.5f;

        float hx = Mathf.Sin(yaw);
        float hy = Mathf.Cos(yaw);

        float vx = Mathf.Cos(pitch);
        float vy = Mathf.Sin(pitch);

        Vector3 dir = new Vector3(hx * vx, vy, hy * vx);

        return dir;
    }

    public float Sigmoid(float value)
    {
        return 1.0f / (1.0f + Mathf.Exp(-value));
    }
}
