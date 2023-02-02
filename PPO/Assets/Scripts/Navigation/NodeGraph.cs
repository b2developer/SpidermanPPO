using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeGraph
{
    public List<Node> nodes;
    public List<Node> spawners;
    public List<Node> available;

    public NodeGraph()
    {
        nodes = new List<Node>();
        spawners = new List<Node>();
        available = new List<Node>();
    }

    public void AddNode(Node node)
    {
        nodes.Add(node);

        if (node.destinations.Count > 0)
        {
            available.Add(node);
        }

        if (node.isSpawner)
        {
            spawners.Add(node);
        }
    }

    public Node RandomSpawningPoint()
    {
        int count = available.Count;
        int random = Random.Range(0, count);

        Node randomNode = available[random];
        //available.RemoveAt(random);

        return randomNode;
    }
}
