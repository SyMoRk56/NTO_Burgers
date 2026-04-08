using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class WaterManager : MonoBehaviour
{
    private Mesh mesh;              // Меш воды
    private Vector3[] baseVertices; // Исходные вершины

    private void Awake()
    {
        // Получаем меш и запоминаем стартовые вершины
        mesh = GetComponent<MeshFilter>().mesh;
        baseVertices = mesh.vertices;
    }

    private void Update()
    {
        // Создаём новый массив вершин
        Vector3[] vertices = new Vector3[baseVertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 localPos = baseVertices[i];

            // Переводим вершину в мировые координаты
            Vector3 worldPos = transform.TransformPoint(localPos);

            // Получаем высоту волны в этой точке
            float height = WaveManager.instance.GetWaveHeight(worldPos);

            // Меняем только Y (высоту)
            localPos.y = height - transform.position.y;

            vertices[i] = localPos;
        }

        // Применяем новые вершины к мешу
        mesh.vertices = vertices;

        // Пересчитываем нормали (для освещения)
        mesh.RecalculateNormals();
    }
}