using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Dresser : MonoBehaviour
{
    public Transform body;
    public Transform tail;

    public Item[] mouths;
    public Item[] hats;
    public Item[] accessories;
    public Item[] fins;
    public Item[] eyewear;

    public float equipChance = 0.5f;

    public void Initialise()
    {
        SetBodyColour(RandomColour());

        EquipRandomFromGroup(ref mouths);
        EquipRandomFromGroup(ref hats);
        EquipRandomFromGroup(ref accessories);
        EquipRandomFromGroup(ref fins);
        EquipRandomFromGroup(ref eyewear);
    }

    void Update()
    {

    }

    public void EquipRandomFromGroup(ref Item[] items)
    {
        if (Random.value > equipChance)
        {
            return;
        }

        int random = Random.Range(0, items.GetLength(0));
        Item item = items[random];

        item.Switch(true);

        item.SetGroup(0, RandomColour());
        item.SetGroup(1, RandomColour());
        item.SetGroup(2, RandomColour());
    }

    public Color RandomColour()
    {
        return Random.ColorHSV(0.0f, 1.0f, 0.7f, 0.9f, 0.75f, 1.0f);
    }

    public void SetBodyColour(Color color)
    {
        body.GetComponent<MeshRenderer>().material.color = color;
        tail.GetComponent<MeshRenderer>().material.color = color;
    }
}
