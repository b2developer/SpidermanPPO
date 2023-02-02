using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public enum EType
    {
        NONE,
        MOUTH,
        HAT,
        ACCESSORY,
        FIN,
        EYEWEAR,
        BODY,
    }

    public EType type;
    public string tag = "";

    public bool visible = false;

    public MeshRenderer[] r1;
    public MeshRenderer[] r2;
    public MeshRenderer[] r3;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void Switch(bool state)
    {
        visible = state;
        gameObject.SetActive(state);
    }

    public void SetGroup(int index, Color color)
    {
        if (index == 0)
        {
            int len = r1.GetLength(0);

            for (int i = 0; i < len; i++)
            {
                r1[i].material.color = color;
            }
        }
        else if (index == 1)
        {
            int len = r2.GetLength(0);

            for (int i = 0; i < len; i++)
            {
                r2[i].material.color = color;
            }
        }
        else if (index == 2)
        {
            int len = r3.GetLength(0);

            for (int i = 0; i < len; i++)
            {
                r3[i].material.color = color;
            }
        }
    }
}
