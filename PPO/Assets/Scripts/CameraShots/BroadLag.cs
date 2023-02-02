using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BroadLag : BaseCameraShot
{
    public Vector3 baseOffset;
    public float initialLag = 0.0f;
    public float finalLag = 0.0f;

    public override bool IsAvailable()
    {
        return true;
    }

    public override Locator SetCamera(float deltaTime)
    {
        timer += deltaTime;

        Vector3 targetPosition = target.position;

        Vector3 offset = baseOffset + Vector3.forward * targetPosition.z;

        float lerp = Mathf.Clamp01(timer / duration);

        Vector3 final = offset + Vector3.Lerp(Vector3.forward * initialLag, Vector3.forward * finalLag, lerp);

        Vector3 relative = targetPosition - final;
        relative.x *= 0.1f;
        relative.y *= 0.3f;
        relative.Normalize();

        Locator locator = new Locator(final, Quaternion.LookRotation(relative));

        activeCamera.fieldOfView = 70.0f;

        return locator;
    }
}
