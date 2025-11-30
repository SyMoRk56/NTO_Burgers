using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(PaintableMusicZone))]
public class BrushZoneEditor : Editor
{
    private bool paintMode;
    private Plane paintingPlane;

    private void OnEnable()
    {
        paintingPlane = new Plane(Vector3.up, Vector3.zero);
    }

    private void OnSceneGUI()
    {
        PaintableMusicZone zone = (PaintableMusicZone)target;
        if (zone == null) return;

        // Устанавливаем плоскость рисования
        paintingPlane = new Plane(Vector3.up, zone.transform.position);

        // Рисуем точки управления
        Handles.color = Color.green;
        for (int i = 0; i < zone.points.Count; i++)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 newPos = Handles.FreeMoveHandle(
                zone.points[i],
                0.5f,
                Vector3.zero,
                Handles.SphereHandleCap
            );

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(zone, "Move Point");
                zone.points[i] = newPos;
                EditorUtility.SetDirty(zone);
            }
        }

        // Рисуем линии между точками
        Handles.color = Color.yellow;
        for (int i = 0; i < zone.points.Count; i++)
        {
            Handles.DrawLine(zone.points[i], zone.points[(i + 1) % zone.points.Count]);
        }

        // Обработка добавления точек в режиме рисования
        Event e = Event.current;

        if (paintMode && e.type == EventType.MouseDown && e.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

            if (paintingPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);

                Undo.RecordObject(zone, "Add Point");
                zone.points.Add(hitPoint);
                EditorUtility.SetDirty(zone);

                e.Use();
            }
        }

        // Обновляем сцену в режиме рисования
        if (paintMode)
        {
            SceneView.RepaintAll();
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();

        EditorGUILayout.Space();

        // Кнопка переключения режима рисования
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        if (paintMode)
        {
            buttonStyle.normal.textColor = Color.red;
        }

        if (GUILayout.Button(paintMode ? "Выйти из режима рисования" : "Войти в режим рисования", buttonStyle))
        {
            paintMode = !paintMode;
            SceneView.RepaintAll();
        }

        // Кнопка очистки точек
        if (GUILayout.Button("Очистить все точки"))
        {
            if (EditorUtility.DisplayDialog("Очистка точек", "Вы уверены, что хотите очистить все точки?", "Да", "Нет"))
            {
                Undo.RecordObject(target, "Clear Points");
                ((PaintableMusicZone)target).points.Clear();
                EditorUtility.SetDirty(target);
            }
        }

        // Информация о режиме рисования
        if (paintMode)
        {
            EditorGUILayout.HelpBox("Режим рисования активен:\n• Кликните в сцене чтобы добавить точку\n• Перетаскивайте точки чтобы переместить", MessageType.Info);
        }

        serializedObject.ApplyModifiedProperties();
    }
}