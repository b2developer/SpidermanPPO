using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intersection : MonoBehaviour
{
    public TrafficLight light11;
    public TrafficLight light12;
    public TrafficLight light21;
    public TrafficLight light22;

    public WalkingLight[] walking1;
    public WalkingLight[] walking2;

    public Node[] starts;
    public Node[] ends;

    public Node[] starts2;
    public Node[] ends2;

    public Node[] walking;

    public float lightTimer = 0.0f;
    public float finalTimer = 0.0f;

    public float greenDuration = 12.0f;
    public float yellowToRedDelay = 5.5f;

    public bool state = true;

    void Start()
    {
        lightTimer = greenDuration;
        finalTimer = yellowToRedDelay;

        light11.SetLights(2);
        light12.SetLights(0);
        light21.SetLights(2);
        light22.SetLights(0);

        SetAll1Green();
        SetAll2Red();
    }

    void Update()
    {
        if (lightTimer > 0.0f)
        {
            lightTimer -= Time.deltaTime;

            if (lightTimer <= 0.0f)
            {
                if (state)
                {
                    light11.Red();
                    light21.Red();

                    SetNodes(true);

                    SetAll1Red();
                }
                else
                {
                    light12.Red();
                    light22.Red();

                    SetAll2Red();
                }
            }
        }

        if (lightTimer <= 0.0f)
        {
            finalTimer -= Time.deltaTime;

            if (finalTimer <= 0.0f)
            {
                if (state)
                {
                    light12.Green();
                    light22.Green();

                    SetAll2Green();
                }
                else
                {
                    light11.Green();
                    light21.Green();

                    SetNodes(false);

                    SetAll1Green();
                }

                finalTimer = yellowToRedDelay;
                lightTimer = greenDuration;
                state = !state;
            }
        }
    }

    public void SetAll1Red()
    {
        for (int i = 0; i < walking1.GetLength(0); i++)
        {
            walking1[i].Red();
        }
    }

    public void SetAll1Green()
    {
        for (int i = 0; i < walking1.GetLength(0); i++)
        {
            walking1[i].Green();
        }
    }

    public void SetAll2Red()
    {
        for (int i = 0; i < walking2.GetLength(0); i++)
        {
            walking2[i].Red();
        }
    }

    public void SetAll2Green()
    {
        for (int i = 0; i < walking2.GetLength(0); i++)
        {
            walking2[i].Green();
        }
    }

    public void SetNodes(bool state)
    {
        for (int i = 0; i < ends.GetLength(0); i++)
        {
            if (ends[i].IsTerminal())
            {
                continue;
            }

            ends[i].destinations[0].disabled = state;
        }

        for (int i = 0; i < ends2.GetLength(0); i++)
        {
            if (ends2[i].IsTerminal())
            {
                continue;
            }

            ends2[i].destinations[0].disabled = state;
        }
    }
}
