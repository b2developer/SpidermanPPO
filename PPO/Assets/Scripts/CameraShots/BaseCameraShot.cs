using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCameraShot : MonoBehaviour
{
    public Camera activeCamera;
    public Transform target;

    public float timer = 0.0f;
    public float duration = 0.0f;
    
    
    public class Locator
    {
        public Vector3 position;
        public Quaternion rotation;

        public Locator()
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;
        }


        public Locator(Vector3 _position, Quaternion _rotation)
        {
            position = _position;
            rotation = _rotation;
        }
    }

    public virtual void Initialise()
    {
        timer = 0.0f;
    }
    
    public virtual bool IsAvailable()
    {
        return false;
    }

    public virtual Locator SetCamera(float deltaTime)
    {
        return new Locator();
    }
}
