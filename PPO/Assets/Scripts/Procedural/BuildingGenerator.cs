using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingGenerator : MonoBehaviour
{
    public static float EPSILON = 1e-2f;

    public Transform parent;

    public Vector3 centre = Vector3.zero;
    public GameObject cubePrefab;
    public GameObject quadPrefab;

    public Material[] buildingMaterials;
    public float[] buildingProbabilities;

    public Material[] windowMaterials;
    public float[] windowProbabilities;

    public Material buildingMaterial;

    public float buildingHeight = 0.0f;
    public float raiseHeight = 0.0f;

    public static float storeyHeight = 4.2672f;

    public Vector3 localRight = Vector3.right;
    public Vector3 localForward = Vector3.forward;

    public void Initialise()
    {
        centre = transform.position;
    }

    void Update()
    {
        
    }

    public Material RandomBuildingMaterial()
    {
        float random = Random.value;
        float sum = 0.0f;
        int count = buildingMaterials.GetLength(0);

        for (int i = 0; i < count; i++)
        {
            sum += buildingProbabilities[i];

            if (random < sum)
            {
                return buildingMaterials[i];
            }
        }

        return buildingMaterials[count - 1];
    }

    public Material RandomWindowMaterial()
    {
        float random = Random.value;
        float sum = 0.0f;
        int count = windowMaterials.GetLength(0); 

        for (int i = 0; i < count; i++)
        {
            sum += windowProbabilities[i];

            if (random < sum)
            {
                return windowMaterials[i];
            }
        }

        return windowMaterials[count - 1];
    }

    public void GenerateEfficientBuilding()
    {
        Material buildingMaterial = RandomBuildingMaterial();
        Material windowMaterial = RandomWindowMaterial();

        GameObject root = Instantiate<GameObject>(cubePrefab);

        root.transform.position = centre + Vector3.up * (buildingHeight * 0.5f + raiseHeight);
        root.transform.localScale = localRight * StreetGenerator.buildingLength + Vector3.up * (buildingHeight + raiseHeight * 2.0f) + localForward * StreetGenerator.buildingLength;
        root.transform.SetParent(parent);

        root.GetComponent<MeshRenderer>().material = buildingMaterial;

        GameObject face = Instantiate<GameObject>(quadPrefab);
        face.transform.position = centre + Vector3.up * (buildingHeight * 0.5f + raiseHeight) + localForward * (StreetGenerator.buildingLength * 0.5f + EPSILON);
        face.transform.forward = -localForward;
        face.transform.localScale = new Vector3(StreetGenerator.buildingLength, buildingHeight, 1.0f);
        face.transform.SetParent(parent);

        face.GetComponent<MeshRenderer>().material = windowMaterial;

        Mesh quadMesh = face.GetComponent<MeshFilter>().mesh;

        Vector2[] uvs = quadMesh.uv;
        uvs[2].y = buildingHeight / storeyHeight;
        uvs[3].y = buildingHeight / storeyHeight;

        quadMesh.SetUVs(0, uvs);

        GameObject face2 = Instantiate<GameObject>(quadPrefab);
        face2.transform.position = centre + Vector3.up * (buildingHeight * 0.5f + raiseHeight) + localRight * (StreetGenerator.buildingLength * 0.5f + EPSILON);
        face2.transform.forward = -localRight;
        face2.transform.localScale = new Vector3(StreetGenerator.buildingLength, buildingHeight, 1.0f);
        face2.transform.SetParent(parent);

        face2.GetComponent<MeshRenderer>().material = windowMaterial;

        Mesh quadMesh2 = face2.GetComponent<MeshFilter>().mesh;

        Vector2[] uvs2 = quadMesh2.uv;
        uvs2[2].y = buildingHeight / storeyHeight;
        uvs2[3].y = buildingHeight / storeyHeight;

        quadMesh2.SetUVs(0, uvs2);

        GameObject face3 = Instantiate<GameObject>(quadPrefab);
        face3.transform.position = centre + Vector3.up * (buildingHeight * 0.5f + raiseHeight) + -localRight * (StreetGenerator.buildingLength * 0.5f + EPSILON);
        face3.transform.forward = localRight;
        face3.transform.localScale = new Vector3(StreetGenerator.buildingLength, buildingHeight, 1.0f);
        face3.transform.SetParent(parent);

        face3.GetComponent<MeshRenderer>().material = windowMaterial;

        Mesh quadMesh3 = face3.GetComponent<MeshFilter>().mesh;

        Vector2[] uvs3 = quadMesh3.uv;
        uvs3[2].y = buildingHeight / storeyHeight;
        uvs3[3].y = buildingHeight / storeyHeight;

        quadMesh3.SetUVs(0, uvs3);
    }
}
