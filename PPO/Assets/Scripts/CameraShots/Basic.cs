using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Basic : BaseCameraShot
{
    public Vector3 offset;

    public Vector3 point;
    public bool legacy = true;

    public override bool IsAvailable()
    {
        return true;
    }

    public override Locator SetCamera(float deltaTime)
    {
        timer += deltaTime;

        Vector3 position = target.position + offset;
        Locator locator = new Locator(position, Quaternion.identity);

        if (!legacy)
        {
            locator.rotation = Quaternion.LookRotation(point);
        }

        //activeCamera.fieldOfView = 80.0f;

        return locator;
    }
}
