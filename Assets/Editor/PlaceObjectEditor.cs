using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlaceObjects))]
public class PlaceObjectsEditor : Editor
{
    private void OnSceneGUI()
    {
        PlaceObjects placer = (PlaceObjects)target;

        Event e = Event.current;

        // Реагируем только на ЛКМ в сцене
        if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 500f, placer.groundLayer))
            {
                Vector3 pos = hit.point;

                Undo.RegisterSceneUndo("Place Object");

                if (placer.prefabToPlace == null)
                {
                    GameObject obj = new GameObject("Placed Object");
                    obj.transform.position = pos;
                    Undo.RegisterCreatedObjectUndo(obj, "Create Empty");
                }
                else
                {
                    GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(placer.prefabToPlace);
                    obj.transform.position = pos;
                    obj.transform.parent = placer.transform;
                    Undo.RegisterCreatedObjectUndo(obj, "Place Prefab");
                }

                e.Use(); // предотвращает выделение объектов
            }
        }
    }
}
