using UnityEngine;

public class ShadowFollow : MonoBehaviour
{
    [Header("References")]
    public Transform character;              // Персонаж
    public SpriteRenderer shadowRenderer;    // Спрайт тени

    [Header("Raycast Settings")]
    public float maxShadowDistance = 3f;     // Макс. высота, после которой тень исчезает
    public LayerMask groundMask;             // Маска земли

    [Header("Shadow Settings")]
    public float minAlpha = 0.2f;            // Прозрачность, когда персонаж максимум далеко
    public float maxAlpha = 0.7f;            // Прозрачность, когда персонаж стоит на земле
    public float heightOffset = 0.02f;       // Чтоб тень не пересекала землю

    void Update()
    {
        // Луч вниз от персонажа
        if (Physics.Raycast(character.position, Vector3.down, out RaycastHit hit, maxShadowDistance, groundMask))
        {
            // Перемещаем тень на землю
            Vector3 p = hit.point;
            p.y += heightOffset;
            transform.position = p;

            // Высота персонажа
            float distance = hit.distance;

            // Нормализуем 0..1
            float t = distance / maxShadowDistance;

            // Интерполируем прозрачность
            float alpha = Mathf.Clamp01(Mathf.Lerp(maxAlpha, minAlpha, t));

            Color c = shadowRenderer.color;
            c.a = alpha;
            shadowRenderer.color = c;
        }
        else
        {
            // Персонаж слишком высоко → тень полностью исчезает
            Color c = shadowRenderer.color;
            c.a = 0;
            shadowRenderer.color = c;
        }
    }
}
