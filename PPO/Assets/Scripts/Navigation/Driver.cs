using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Driver : MonoBehaviour
{
    public TrafficGenerator trafficGenerator;

    public float speed = 0.0f;
    public float progress = 0.0f;

    public float length = 0.0f;

    public Node currentNode;
    public int destinationIndex;
    
    void Start()
    {
        
    }

    void Update()
    {
        float queue = QueueDistance();
        float distance = currentNode.distances[destinationIndex];

        progress += speed * Time.deltaTime;

        if (queue > 0.0f && progress > distance - queue)
        {
            progress = distance - queue;
        }

        Driver localLeader = currentNode.GetLocalLeader(this);

        if (localLeader != null)
        {
            if (progress > localLeader.progress - localLeader.length)
            {
                progress = localLeader.progress - localLeader.length;
            }
        }

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
        transform.forward = Vector3.Lerp(startDir, endDir, lerp).normalized;

        if (lerp >= 1.0f)
        {
            destinationIndex = 0;
            Node nextNode = currentNode.GetNextNode(0); 

            if (nextNode.IsTerminal())
            {
                currentNode.occupants.Remove(this);

                currentNode = trafficGenerator.streetGenerator.graph.spawners[Random.Range(0, trafficGenerator.streetGenerator.graph.spawners.Count)];
                currentNode.occupants.Add(this);
                progress = leftover;

                return;
            }

            if (nextNode.destinations[0].disabled)
            {
                return;
            }

            currentNode.occupants.Remove(this);

            currentNode = nextNode;
            currentNode.occupants.Add(this);
            progress = leftover;
        }
    }

    public float QueueDistance()
    {
        float sum = 0.0f;

        for (int i = 0; i < currentNode.occupants.Count; i++)
        {
            if (currentNode.occupants[i] == this)
            {
                break;
            }

            sum += currentNode.occupants[i].length;
        }

        return sum;
    }
}
