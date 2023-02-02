using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Randomiser : MonoBehaviour
{
    public List<Item> bodies;
    public List<Item> hats;
    public List<Item> accessories;
    public List<Item> fins;
    public List<Item> eyewear;
    public List<Item> mouths;

    public float baseProbability = 0.8f;

    void Start()
    {
        bodies = new List<Item>();
        hats = new List<Item>();
        accessories = new List<Item>();
        fins = new List<Item>();
        eyewear = new List<Item>();
        mouths = new List<Item>();

        Item[] all = GetComponentsInChildren<Item>();

        int count = all.GetLength(0);

        //sort the items into sub-arrays
        for (int i = 0; i < count; i++)
        {
            switch (all[i].type)
            {
                case Item.EType.BODY: bodies.Add(all[i]); break;
                case Item.EType.HAT: hats.Add(all[i]); break;
                case Item.EType.ACCESSORY: accessories.Add(all[i]); break;
                case Item.EType.FIN: fins.Add(all[i]); break;
                case Item.EType.EYEWEAR: eyewear.Add(all[i]); break;
                case Item.EType.MOUTH: mouths.Add(all[i]); break;
            }
        }
    }

    
    void Update()
    {
        
    }
}
