using UnityEngine;
using System.Collections.Generic;

public class PaintableMusicZone : MonoBehaviour
{
    public static List<PaintableMusicZone> allZones = new List<PaintableMusicZone>();

    public List<Vector3> points = new List<Vector3>();
    public AudioClip music;
    public MusicMixer mixer;

    private void Reset()
    {
        // Автоматически создаем начальный прямоугольник
        if (points.Count == 0)
        {
            points = new List<Vector3>
            {
                new Vector3(-2, 0, -2),
                new Vector3(2, 0, -2),
                new Vector3(2, 0, 2),
                new Vector3(-2, 0, 2)
            };
        }
    }

    private void OnEnable() => allZones.Add(this);
    private void OnDisable() => allZones.Remove(this);

    private void Update()
    {
        if (MusicManager.Instance == null) return;

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        Vector3 playerPos = player.transform.position;
        bool insideAnyZone = false;

        foreach (var zone in allZones)
        {
            if (zone != null && zone.IsPointInsidePolygon(playerPos))
            {
                MusicManager.Instance.PlayMusic(zone.music, zone.mixer);
                insideAnyZone = true;
                break;
            }
        }

        if (!insideAnyZone)
        {
            MusicManager.Instance.PlayDefault();
        }
    }

    // Алгоритм лучевого метода для проверки точки внутри полигона
    public bool IsPointInsidePolygon(Vector3 point)
    {
        int count = points.Count;
        if (count < 3) return false;

        bool inside = false;

        for (int i = 0, j = count - 1; i < count; j = i++)
        {
            Vector3 pi = points[i];
            Vector3 pj = points[j];

            if (((pi.z > point.z) != (pj.z > point.z)) &&
                (point.x < (pj.x - pi.x) * (point.z - pi.z) / (pj.z - pi.z) + pi.x))
            {
                inside = !inside;
            }
        }

        return inside;
    }

    private void OnDrawGizmos()
    {
        if (points == null || points.Count < 2) return;

        // Рисуем точки
        Gizmos.color = Color.green;
        foreach (Vector3 point in points)
        {
            Gizmos.DrawSphere(point, 0.1f);
        }

        // Рисуем линии
        Gizmos.color = Color.yellow;
        for (int i = 0; i < points.Count; i++)
        {
            Gizmos.DrawLine(points[i], points[(i + 1) % points.Count]);
        }

        // Заливка полигона
        Gizmos.color = new Color(0, 1, 0, 0.1f);
        for (int i = 2; i < points.Count; i++)
        {
            Gizmos.DrawLine(points[0], points[i - 1]);
            Gizmos.DrawLine(points[i - 1], points[i]);
            Gizmos.DrawLine(points[i], points[0]);
        }
    }
}