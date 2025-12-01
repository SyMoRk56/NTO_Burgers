using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(PaintableMusicZone))]
public class PrintableMusicZoneEditor : Editor
{
    private bool paintMode;

    private void OnSceneGUI()
    {
        PaintableMusicZone zone = (PaintableMusicZone)target;

        Handles.color = Color.green;

        // ������ ����� ��������
        for (int i = 0; i < zone.points.Count; i++)
        {
            var fmh_21_17_638991795042245010 = Quaternion.identity; Vector3 newPos = Handles.FreeMoveHandle(
                zone.points[i],
                0.15f,
                Vector3.zero,
                Handles.SphereHandleCap
            );

            if (newPos != zone.points[i])
            {
                Undo.RecordObject(zone, "Move Point");
                zone.points[i] = newPos;
            }
        }

        // ������ �����
        Handles.color = Color.yellow;
        for (int i = 0; i < zone.points.Count; i++)
        {
            Handles.DrawLine(zone.points[i], zone.points[(i + 1) % zone.points.Count]);
        }

        // ������� ������� � ���� ����� ��������� �����
        Event e = Event.current;
        if (paintMode && e.type == EventType.MouseDown && e.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.Log("Add point");
                Undo.RecordObject(zone, "Add Point");
                zone.points.Add(hit.point);
                e.Use();
            }
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Toggle Paint Mode"))
            paintMode = !paintMode;

        if (paintMode)
            EditorGUILayout.HelpBox("Paint mode active: Click in Scene to add points", MessageType.Info);
    }
}
