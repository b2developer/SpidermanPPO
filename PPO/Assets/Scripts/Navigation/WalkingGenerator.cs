using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkingGenerator : MonoBehaviour
{
    public int walkers = 50;

    public NodeGraph graph;
    public GameObject prefab;

    public void Initialise()
    {
        for (int i = 0; i < walkers; i++)
        {
            SpawnWalker();
        }
    }

    void Update()
    {
        
    }

    public void SpawnWalker()
    {
        GameObject instance = Instantiate(prefab);

        Dresser dresser = instance.GetComponent<Dresser>();
        dresser.Initialise();

        Node node = graph.RandomSpawningPoint();

        instance.transform.position = node.position;
        instance.transform.forward = node.forward;
        instance.transform.SetParent(transform);

        Walker walkerInstance = instance.GetComponent<Walker>();

        walkerInstance.walkingGenerator = this;
        walkerInstance.currentNode = node;
        walkerInstance.progress = Random.value * node.distances[0];
    }
}
