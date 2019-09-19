using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    public int CircleSegmentCount = 64;

    MeshFilter mesh;

    public void Start()
    {
        mesh = GetComponent<MeshFilter>();
    }
    public void Update()
    {


        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            mesh.mesh = GenerateCircleMesh(CircleSegmentCount);
        }
    }

    private static Mesh GenerateCircleMesh(int segments)
    {
        int CircleVertexCount = segments + 2;
        int CircleIndexCount = segments * 3;

        var circle = new Mesh();
        var vertices = new List<Vector3>(CircleVertexCount);
        var indices = new int[CircleIndexCount];
        var segmentWidth = Mathf.PI * 2f / segments;
        var angle = 0f;
        vertices.Add(Vector3.zero);
        for (int i = 1; i < CircleVertexCount; ++i)
        {
            vertices.Add(new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)));
            angle -= segmentWidth;
            if (i > 1)
            {
                var j = (i - 2) * 3;
                indices[j + 0] = 0;
                indices[j + 1] = i - 1;
                indices[j + 2] = i;
            }
        }
        circle.SetVertices(vertices);
        circle.SetIndices(indices, MeshTopology.Triangles, 0);
        circle.RecalculateBounds();

        circle.name = "CircleMesh";
        return circle;
    }
}
