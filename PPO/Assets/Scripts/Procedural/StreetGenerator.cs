using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StreetGenerator : MonoBehaviour
{
    public TrafficGenerator trafficGenerator;
    public WalkingGenerator walkingGenerator;

    public GameObject buildingPrefab;
    public GameObject roadPrefab;

    public int vehicles = 60;

    public int buildingsPerBlock = 10;
    public static float buildingLength = 20.0f;
    public float streetWidth = 25.0f;

    public float intersectionGap = 13.5f;

    public float heightMean = 143.5f;
    public float heightStd = 57.4f;
    public float heightMin = 21.45f;

    public float blockLength = 200.0f;
    public float intersectionLength = 13.5f;
    public float randomOffsetLength = 7.0f;

    public NormalDistribution buildingDistribution;

    public NodeGraph graph;

    void Start()
    {
        int BLOCKS = 10;

        buildingDistribution = new NormalDistribution(heightMean, heightStd);

        Intersection[] intersection = new Intersection[BLOCKS];

        float start = 10.0f;

        for (int block = 0; block < BLOCKS; block++)
        {
            for (int i = 0; i < buildingsPerBlock; i++)
            {
                for (int j = -1; j <= 1; j += 2)
                {
                    float randomOffset = 0.0f;

                    if (i != 0 && i != buildingsPerBlock - 1)
                    {
                        randomOffset = Random.value * randomOffsetLength * j;
                    }

                    float position = start + buildingLength * i + (blockLength + intersectionLength) * block;
                    float height = buildingDistribution.Sample();

                    if (height < heightMin)
                    {
                        height = heightMin;
                    }

                    GameObject instance = Instantiate<GameObject>(buildingPrefab);
                    BuildingGenerator generator = instance.GetComponent<BuildingGenerator>();

                    generator.transform.SetParent(transform);
                    generator.parent = generator.transform;
                    generator.transform.position = new Vector3(j * (streetWidth / 2.0f + buildingLength / 2.0f) + randomOffset, 0.0f, position);
                    generator.buildingHeight = height;

                    generator.buildingHeight /= BuildingGenerator.storeyHeight;
                    generator.buildingHeight = Mathf.Floor(generator.buildingHeight);
                    generator.buildingHeight *= BuildingGenerator.storeyHeight;

                    generator.localRight = Vector3.forward;
                    generator.localForward = Vector3.right * -j;
                    generator.Initialise();
                    generator.GenerateEfficientBuilding();

                }

                if (i == 0 || i == buildingsPerBlock - 1)
                {
                    for (int j = -1; j <= 1; j += 2)
                    {
                        float jtrue = j * 2.0f;

                        float position = start + buildingLength * i + (blockLength + intersectionLength) * block;
                        float height = buildingDistribution.Sample();

                        if (height < heightMin)
                        {
                            height = heightMin;
                        }

                        GameObject instance = Instantiate<GameObject>(buildingPrefab);
                        BuildingGenerator generator = instance.GetComponent<BuildingGenerator>();

                        float jminus = jtrue - Mathf.Sign(jtrue);

                        generator.transform.SetParent(transform);
                        generator.parent = generator.transform;
                        generator.transform.position = new Vector3(Mathf.Sign(jtrue) * (streetWidth / 2.0f + buildingLength / 2.0f) + buildingLength * jminus, 0.0f, position);
                        generator.buildingHeight = height;

                        generator.buildingHeight /= BuildingGenerator.storeyHeight;
                        generator.buildingHeight = Mathf.Floor(generator.buildingHeight);
                        generator.buildingHeight *= BuildingGenerator.storeyHeight;

                        generator.localRight = Vector3.forward;
                        generator.localForward = Vector3.right * -Mathf.Sign(jtrue);
                        generator.Initialise();
                        generator.GenerateEfficientBuilding();

                    }

                    for (int j = -1; j <= 1; j += 2)
                    {
                        float jtrue = j * 3.0f;

                        float position = start + buildingLength * i + (blockLength + intersectionLength) * block;
                        float height = buildingDistribution.Sample();

                        if (height < heightMin)
                        {
                            height = heightMin;
                        }

                        GameObject instance = Instantiate<GameObject>(buildingPrefab);
                        BuildingGenerator generator = instance.GetComponent<BuildingGenerator>();

                        float jminus = jtrue - Mathf.Sign(jtrue);

                        generator.transform.SetParent(transform);
                        generator.parent = generator.transform;
                        generator.transform.position = new Vector3(Mathf.Sign(jtrue) * (streetWidth / 2.0f + buildingLength / 2.0f) + buildingLength * jminus, 0.0f, position);
                        generator.buildingHeight = height;

                        generator.buildingHeight /= BuildingGenerator.storeyHeight;
                        generator.buildingHeight = Mathf.Floor(generator.buildingHeight);
                        generator.buildingHeight *= BuildingGenerator.storeyHeight;

                        generator.localRight = Vector3.forward;
                        generator.localForward = Vector3.right * -Mathf.Sign(jtrue);
                        generator.Initialise();
                        generator.GenerateEfficientBuilding();

                    }
                }
            }

            GameObject roadInstance = Instantiate<GameObject>(roadPrefab);

            float roadPosition = 100.0f + (blockLength + intersectionLength) * block;
            roadInstance.transform.position = new Vector3(0.0f, 0.0f, roadPosition);
            roadInstance.transform.SetParent(transform);

            intersection[block] = roadInstance.GetComponent<Intersection>();

            for (int j = 0; j < 3; j++)
            {
                intersection[block].starts[j].Initialise();
                intersection[block].ends[j].Initialise();

                intersection[block].starts2[j].Initialise();
                intersection[block].ends2[j].Initialise();
            }
        }

        graph = new NodeGraph();
        walkingGenerator.graph = new NodeGraph();

        for (int block = 0; block < BLOCKS - 1; block++)
        {
            for (int j = 0; j < 3; j++)
            {
                intersection[block].starts[j].AddDestination(intersection[block].ends[j]);
                intersection[block].ends[j].AddDestination(intersection[block + 1].starts[j]);
                intersection[block].starts[j].disabled = true;

                if (block == 0)
                {
                    intersection[block].starts[j].isSpawner = true;
                }

                graph.AddNode(intersection[block].starts[j]);
                graph.AddNode(intersection[block].ends[j]);
            }

            for (int j = 0; j < 8; j++)
            {
                intersection[block].walking[j].position = intersection[block].walking[j].transform.position;
                intersection[block].walking[j].forward = intersection[block].walking[j].transform.forward;
                walkingGenerator.graph.AddNode(intersection[block].walking[j]);
            }
        }

        for (int j = 0; j < 3; j++)
        {
            intersection[BLOCKS - 1].starts[j].AddDestination(intersection[BLOCKS - 1].ends[j]);
            intersection[BLOCKS - 1].starts[j].disabled = true;

            graph.AddNode(intersection[BLOCKS - 1].starts[j]);
            graph.AddNode(intersection[BLOCKS - 1].ends[j]);
        }

        for (int block = BLOCKS - 1; block > 0; block--)
        {
            for (int j = 0; j < 3; j++)
            {
                intersection[block].starts2[j].AddDestination(intersection[block].ends2[j]);
                intersection[block].ends2[j].AddDestination(intersection[block - 1].starts2[j]);
                intersection[block].starts2[j].disabled = true;

                if (block == BLOCKS - 1)
                {
                    intersection[block].starts2[j].isSpawner = true;
                }

                graph.AddNode(intersection[block].starts2[j]);
                graph.AddNode(intersection[block].ends2[j]);
            }
        }

        for (int j = 0; j < 3; j++)
        {
            intersection[0].starts2[j].AddDestination(intersection[0].ends2[j]);
            intersection[0].starts2[j].disabled = true;

            graph.AddNode(intersection[0].starts2[j]);
            graph.AddNode(intersection[0].ends2[j]);
        }

        trafficGenerator.activeDrivers = new List<Driver>();

        for (int i = 0; i < vehicles; i++)
        {
            trafficGenerator.SpawnVehicle();
        }

        walkingGenerator.Initialise();
    }

    void Update()
    {
        
    }
}
