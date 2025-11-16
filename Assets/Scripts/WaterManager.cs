using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class WaterManager : MonoBehaviour
{
    private Mesh mesh;
    private Vector3[] baseVertices;

    private void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        baseVertices = mesh.vertices;
    }

    private void Update()
    {
        Vector3[] vertices = new Vector3[baseVertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 localPos = baseVertices[i];
            Vector3 worldPos = transform.TransformPoint(localPos);
            float height = WaveManager.instance.GetWaveHeight(worldPos);
            localPos.y = height - transform.position.y;
            vertices[i] = localPos;
        }
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }
}
