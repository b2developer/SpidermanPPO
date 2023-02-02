using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grounded : BaseCameraShot
{
    public Vector3 fixedPosition;
    public float lead = 30.0f;

    public override void Initialise()
    {
        base.Initialise();

        fixedPosition = new Vector3(-15.0f, 2.0f, target.position.z + lead);
    }

    public override bool IsAvailable()
    {
        return true;
    }

    public override Locator SetCamera(float deltaTime)
    {
        timer += deltaTime;

        Vector3 direction = (target.position - fixedPosition).normalized;

        Locator locator = new Locator(fixedPosition, Quaternion.LookRotation(direction));

        activeCamera.fieldOfView = 50.0f;

        return locator;
    }
}
