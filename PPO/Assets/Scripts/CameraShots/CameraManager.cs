using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Camera activeCamera;
    public Transform target;

    public BaseCameraShot[] shots;

    public int index = 0;

    void Start()
    {
        int count = shots.GetLength(0);

        for (int i = 0; i < count; i++)
        {
            //shots[i].activeCamera = activeCamera;
            shots[i].target = target;
        }
    }

    void Update()
    {
        BaseCameraShot shot = shots[index];

        BaseCameraShot.Locator locator = shot.SetCamera(Time.deltaTime);

        transform.position = locator.position;
        transform.rotation = locator.rotation;

        if (shot.timer >= shot.duration)
        {
            SelectNewShot();
        }
    }

    public void SelectNewShot()
    {
        shots[index].Initialise();

        List<BaseCameraShot> available = new List<BaseCameraShot>();
        List<int> indices = new List<int>();

        int count = shots.GetLength(0);

        for (int i = 0; i < count; i++)
        {
            //the same camera angle can't appear twice in a row
            if (i == index)
            {
                continue;
            }

            if (shots[i].IsAvailable())
            {
                available.Add(shots[i]);
                indices.Add(i);
            }
        }

        int selection = Random.Range(0, available.Count);
        //index = indices[selection];
        //index++;

        if (index == count)
        {
            index = 0;
        }

        shots[index].Initialise();
    }
}
