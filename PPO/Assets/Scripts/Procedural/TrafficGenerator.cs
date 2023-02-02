using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficGenerator : MonoBehaviour
{
    public StreetGenerator streetGenerator;

    public GameObject[] prefab;
    public float[] probabilities;

    public List<Driver> activeDrivers;

    public float driverSpeed = 12.0f;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public GameObject RandomPrefab()
    {
        float value = Random.value;

        for (int i = 0; i < prefab.GetLength(0); i++)
        {
            value -= probabilities[i];

            if (value <= 0.0f)
            {
                return prefab[i];
            }
        }

        return prefab[prefab.GetLength(0) - 1];
    }

    public void SpawnVehicle()
    {
        GameObject prefab = RandomPrefab();

        GameObject instance = Instantiate(prefab);
        Driver driverInstance = instance.GetComponent<Driver>();

        activeDrivers.Add(driverInstance);

        Node node = streetGenerator.graph.RandomSpawningPoint();

        instance.transform.position = node.position;
        instance.transform.forward = node.forward;
        instance.transform.SetParent(transform);

        driverInstance.trafficGenerator = this;
        driverInstance.currentNode = node;
        driverInstance.currentNode.occupants.Add(driverInstance);
        driverInstance.speed = driverSpeed;
        driverInstance.progress = Random.value * node.distances[0];
    }
}
