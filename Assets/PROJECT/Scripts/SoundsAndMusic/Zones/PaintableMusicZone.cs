using UnityEngine;
using System.Collections.Generic;

public class PaintableMusicZone : MonoBehaviour
{
    public static List<PaintableMusicZone> allZones = new List<PaintableMusicZone>();

    public List<Vector3> points = new List<Vector3>();
    public AudioClip music;

    private void OnEnable() => allZones.Add(this);
    private void OnDisable() => allZones.Remove(this);

    private void Update()
    {
        if (MusicManager.Instance == null) return;

        Vector3 playerPos = GameObject.FindWithTag("Player").transform.position;

        bool insideAnyZone = false;

        foreach (var zone in allZones)
        {
            if (zone.IsPointInsidePolygon(playerPos))
            {
                MusicManager.Instance.PlayMusic(zone.music);
                insideAnyZone = true;
                break;
            }
        }

        if (!insideAnyZone)
        {
            MusicManager.Instance.PlayDefault();
        }
    }

    // Алгоритм лучевого метода
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
        if (points == null || points.Count == 0)
            return;

        Gizmos.color = new Color(0, 1, 0, 0.25f);

        for (int i = 0; i < points.Count; i++)
            Gizmos.DrawSphere(points[i], 0.1f);

        Gizmos.color = Color.green;
        for (int i = 0; i < points.Count; i++)
            Gizmos.DrawLine(points[i], points[(i + 1) % points.Count]);
    }
}
