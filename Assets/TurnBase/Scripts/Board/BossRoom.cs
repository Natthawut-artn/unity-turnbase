using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossRoom : MonoBehaviour
{
    void Start()
    {
        Mesh mesh = new Mesh();

        // จุด 4 มุม (ตามเข็มนาฬิกา)
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f, 0),
            new Vector3(0.5f, -0.5f, 0),
            new Vector3(-0.5f,  0.5f, 0),
            new Vector3(0.5f,  0.5f, 0)
        };

        // กำหนดสามเหลี่ยม (2 อัน)
        int[] triangles = new int[]
        {
            0, 2, 1, // สามเหลี่ยมแรก
            2, 3, 1  // สามเหลี่ยมสอง
        };

        // UV สำหรับ texture
        Vector2[] uv = new Vector2[]
        {
            new Vector2(0,0),
            new Vector2(1,0),
            new Vector2(0,1),
            new Vector2(1,1)
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
    }
}
