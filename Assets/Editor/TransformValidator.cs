using UnityEngine;
using UnityEditor;

public class TransformValidator
{
    private const float MinValue = 0.0001f;
    private const float MaxValue = 10000f;

    [MenuItem("Tools/Fix Colliders")]
    static void FixColliders()
    {
        int fixedCount = 0;

        foreach (var col in GameObject.FindObjectsOfType<Collider>())
        {
            if (col is BoxCollider box)
            {
                Vector3 s = box.size;
                Vector3 newSize = ValidateVector3(s);

                if (newSize != s)
                {
                    Undo.RecordObject(box, "Fix BoxCollider Size");
                    box.size = newSize;
                    fixedCount++;
                    Debug.LogWarning($"Fixed BoxCollider size on: {box.name}", box);
                }
            }
            else if (col is SphereCollider sphere)
            {
                float r = sphere.radius;
                float newR = ValidateFloat(r);

                if (!Mathf.Approximately(r, newR))
                {
                    Undo.RecordObject(sphere, "Fix SphereCollider Radius");
                    sphere.radius = newR;
                    fixedCount++;
                    Debug.LogWarning($"Fixed SphereCollider radius on: {sphere.name}", sphere);
                }
            }
            else if (col is CapsuleCollider capsule)
            {
                float r = capsule.radius;
                float h = capsule.height;

                float newR = ValidateFloat(r);
                float newH = ValidateFloat(h);

                if (!Mathf.Approximately(r, newR) || !Mathf.Approximately(h, newH))
                {
                    Undo.RecordObject(capsule, "Fix CapsuleCollider");
                    capsule.radius = newR;
                    capsule.height = newH;
                    fixedCount++;
                    Debug.LogWarning($"Fixed CapsuleCollider on: {capsule.name}", capsule);
                }
            }
            else if (col is MeshCollider mesh)
            {
                if (mesh.sharedMesh == null)
                {
                    Debug.LogError($"MeshCollider has NULL mesh: {mesh.name}", mesh);
                    continue;
                }

                if (!mesh.sharedMesh.bounds.size.IsFinite())
                {
                    Undo.RecordObject(mesh, "Fix MeshCollider Mesh Bounds");
                    mesh.sharedMesh.RecalculateBounds();
                    fixedCount++;
                    Debug.LogWarning($"Recalculated MeshCollider bounds for: {mesh.name}", mesh);
                }
            }
        }

        EditorUtility.DisplayDialog("Collider Fixer", $"Fixed {fixedCount} colliders!", "OK");
    }

    static Vector3 ValidateVector3(Vector3 v)
    {
        return new Vector3(
            ValidateFloat(v.x),
            ValidateFloat(v.y),
            ValidateFloat(v.z)
        );
    }

    static float ValidateFloat(float f)
    {
        if (float.IsNaN(f) || float.IsInfinity(f)) return MinValue;
        if (f <= 0) return MinValue;
        if (f > MaxValue) return MaxValue;
        return f;
    }
}

public static class Vector3Extensions
{
    public static bool IsFinite(this Vector3 v) =>
        v.x.IsFinite() && v.y.IsFinite() && v.z.IsFinite();
}

public static class FloatExtensions
{
    public static bool IsFinite(this float f) =>
        !float.IsNaN(f) && !float.IsInfinity(f);
}