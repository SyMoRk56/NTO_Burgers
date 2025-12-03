using System.Collections;
using UnityEngine;

public class FishWater : MonoBehaviour
{
    public GameObject fishPrefab;
    [Header("Параметры прыжка")]
    public float jumpHeight = 2f;         // высота дуги
    public float jumpDuration = 0.7f;    // длительность прыжка в секундах
    public float width = 1f;             // множитель для разброса по XZ

    [Header("Переворот")]
    [Range(0f, 1f)] public float flipStartT = 0.45f;   // когда начать переворот (нормализованное время 0..1)
    public float flipDurationT = 0.25f;                // длительность переворота (в долях от общей длительности, можно >0.25)
    public float initialTilt = 30f;   // начальный наклон перед прыжком

    [Header("Исчезновение")]
    public float vanishDuration = 1f;
    public AudioSource source;
    public AudioClip clip;
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.gameObject != gameObject) return;

                SpawnFish(hit.point + new Vector3(Random.Range(-1, 1), 0, Random.Range(-1, 1)));
            }
        }
    }

    void SpawnFish(Vector3 startPoint)
    {
        GameObject fish = Instantiate(fishPrefab, startPoint, Quaternion.identity);

        // Делаем рыбу менее вертикальной в начале
        fish.transform.rotation = Quaternion.Euler(initialTilt, fish.transform.eulerAngles.y, 0f);

        // Выключаем физику, если есть Rigidbody, чтобы не мешала ручному движению
        Rigidbody rb = fish.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Случайная целевая точка
        // --- НАПРАВЛЕНИЕ КАМЕРЫ В ПЛОСКОСТИ XZ ---
        Vector3 camForward = Camera.main.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        // Перпендикуляр к камере (влево)
        Vector3 camSide = new Vector3(camForward.z, 0f, -camForward.x);

        // Случайно выбираем сторону
        float sideSign = Random.value < 0.5f ? -1f : 1f;

        // Добавляем небольшой разброс вдоль бокового направления
        Vector3 sideOffset = camSide * (width * sideSign);

        // Всегда чуть вперёд по боковому направлению, но не вперёд-назад камеры
        float randomSideMul = Mathf.Lerp(0.4f, 1f, RandEdge(Random.value));
        sideOffset *= randomSideMul;

        // Итоговая цель
        Vector3 targetPoint = startPoint + sideOffset + new Vector3(0f, -2f, 0f);



        // Конечная ориентация после переворота (переворачиваем по X на 180 и случайный Y)
        Quaternion startRot = fish.transform.rotation;
        Quaternion endRot = Quaternion.Euler(startRot.eulerAngles.x + 180f, Random.Range(0f, 360f), startRot.eulerAngles.z);

        // Запускаем корутину движения
        StartCoroutine(MoveFishManual(fish, startPoint, targetPoint, jumpDuration, jumpHeight, startRot, endRot));
        source.PlayOneShot(clip);
    }

    IEnumerator MoveFishManual(GameObject fish, Vector3 start, Vector3 end, float duration, float height, Quaternion rotStart, Quaternion rotEnd)
    {
        float elapsed = 0f;
        bool flipped = false;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Горизонтальная интерполяция (линейная между start и end по XZ и базовый Y)
            Vector3 basePos = Vector3.Lerp(start, end, t);

            // Параболическая поправка по Y (симметричная парабола: 4*h*t*(1-t))
            float arc = 4f * height * t * (1f - t);
            Vector3 pos = new Vector3(basePos.x, Mathf.Lerp(start.y, end.y, t) + arc, basePos.z);

            // Принудительно ставим позицию
            fish.transform.position = pos;

            // Обработка переворота вручную:
            // Начинаем интерполировать rotation когда t >= flipStartT
            if (!flipped && t >= flipStartT)
            {
                flipped = true; // старт переворота
            }

            if (flipped)
            {
                // compute local t for flip (0..1)
                float flipLocalT = Mathf.InverseLerp(flipStartT, Mathf.Min(1f, flipStartT + flipDurationT), t);
                flipLocalT = Mathf.Clamp01(flipLocalT);

                // Плавный S-curve можно использовать для красивого easing
                float easeT = Mathf.SmoothStep(0f, 1f, flipLocalT);

                Quaternion currentRot = Quaternion.Slerp(rotStart, rotEnd, easeT);
                fish.transform.rotation = currentRot;
            }
            else
            {
                // Пока не начали переворот — удерживаем стартовую ориентацию (чтобы физика/что-то не мешали)
                fish.transform.rotation = rotStart;
            }

            yield return null;
        }

        // Под конец — ставим окончательную позицию и ориентацию
        fish.transform.position = end;
        fish.transform.rotation = rotEnd;

        // Плавное исчезновение (масштаб)
        float vanishElapsed = 0f;
        Vector3 startScale = fish.transform.localScale;
        while (vanishElapsed < vanishDuration)
        {
            vanishElapsed += Time.deltaTime;
            float vt = Mathf.Clamp01(vanishElapsed / vanishDuration);
            fish.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, Mathf.SmoothStep(0f, 1f, vt));
            yield return null;
        }

        Destroy(fish);
    }
    float RandEdge(float w)
    {
        float r = Mathf.Pow(Random.value, 4f);   // редко около 0, часто около 1
        float sign = Random.value < 0.5f ? -1f : 1f;
        return r * sign * w;
    }

}
