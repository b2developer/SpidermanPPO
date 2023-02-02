using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walker : MonoBehaviour
{
    public WalkingGenerator walkingGenerator;

    public float speed = 0.0f;
    public float progress = 0.0f;

    public Node currentNode;
    public int destinationIndex;

    void Start()
    {
        
    }

    
    void Update()
    {
        float distance = currentNode.distances[destinationIndex];

        progress += speed * Time.deltaTime;

        float lerp = progress / distance;
        float oldLerp = lerp;
        lerp = Mathf.Clamp01(lerp);

        float leftover = oldLerp * distance - lerp * distance;

        progress = lerp * distance;

        Vector3 start = currentNode.position;
        Vector3 startDir = currentNode.forward;

        Vector3 end = currentNode.destinations[destinationIndex].position;
        Vector3 endDir = currentNode.destinations[destinationIndex].forward;

        transform.position = Vector3.Lerp(start, end, lerp);
        transform.forward = startDir;// Vector3.Lerp(startDir, endDir, lerp).normalized;

        if (lerp >= 1.0f)
        {
            destinationIndex = 0;
            Node nextNode = currentNode.GetNextNode(0);

            if (nextNode.IsTerminal())
            {
                currentNode = walkingGenerator.graph.spawners[Random.Range(0, walkingGenerator.graph.spawners.Count)];
                progress = leftover;

                return;
            }

            if (nextNode.destinations[0].disabled)
            {
                return;
            }

            currentNode = nextNode;
            progress = leftover;
        }
    }
}
