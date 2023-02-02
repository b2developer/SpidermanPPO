using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor;

public class WindowFixer : MonoBehaviour
{
    public GameObject[] buildings;
    public Mesh plane;

    void Start()
    {
        int buildingCount = buildings.GetLength(0);
        int counter = 0;

        for (int i = 0; i < buildingCount; i++)
        {
            Transform[] windows = buildings[i].GetComponentsInChildren<Transform>();

            int windowCount = windows.GetLength(0);

            for (int j = 0; j < windowCount; j++)
            {
                if (windows[j] == buildings[i].transform)
                {
                    continue;
                }

                if (windows[j].name == "Cube(Clone)")
                {
                    continue;
                }

                MeshFilter filter = windows[j].GetComponent<MeshFilter>();

                filter.mesh = new Mesh();
                filter.mesh.vertices = new List<Vector3>(plane.vertices).ToArray();
                filter.mesh.normals = new List<Vector3>(plane.normals).ToArray();
                filter.mesh.uv = new List<Vector2>(plane.uv).ToArray();
                filter.mesh.tangents = new List<Vector4>(plane.tangents).ToArray();

                filter.mesh = plane;

                Mesh quadMesh = filter.mesh;

                float buildingHeight = windows[j].localScale.y;
                float storeyHeight = 4.2672f;

                Vector2[] uvs = quadMesh.uv;
                uvs[2].y = buildingHeight / storeyHeight;
                uvs[3].y = buildingHeight / storeyHeight;

                quadMesh.SetUVs(0, uvs);

                //AssetDatabase.CreateAsset(quadMesh, "Assets\\Scripts\\Animations\\GeneratedMeshes\\" + "mesh" + counter.ToString() + ".asset");
                counter++;
            }
        }

        //AssetDatabase.SaveAssets();
    }

    void Update()
    {
        
    }
}
