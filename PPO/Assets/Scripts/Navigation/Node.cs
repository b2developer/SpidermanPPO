using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public Vector3 position;
    public Vector3 forward;

    public List<Node> destinations;
    public List<float> distances;

    public List<Driver> occupants;

    public bool disabled = false;

    public bool isSpawner = false;

    public void Initialise()
    {
        position = transform.position;
        forward = transform.forward;

        destinations = new List<Node>();
        distances = new List<float>();

        occupants = new List<Driver>();
    }

    public void AddDestination(Node node)
    {
        destinations.Add(node);

        Vector3 relative = node.position - position;
        distances.Add(relative.magnitude);
    }

    public Node GetNextNode(int index)
    {
        if (destinations.Count == 0)
        {
            return null;
        }

        if (destinations[index].disabled)
        {
            //return null;
        }

        return destinations[index];

    }

    public bool IsTerminal()
    {
        return destinations.Count == 0;
    }

    public Driver GetLocalLeader(Driver driver)
    {
        int index = occupants.IndexOf(driver);

        if (index < 1)
        {
            return null;
        }

        return occupants[index - 1];
    }
}
