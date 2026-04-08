using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class RespawnPlayer : MonoBehaviour
{
    public static RespawnPlayer instance;

    public float respawnDelay = 2f; // Задержка перед респавном
    private List<PositionRecord> positionHistory = new List<PositionRecord>(); // История позиций

    // ����������� ����� ������� ����� ������� �� �������
    public float historyLength = 5f; // Сколько секунд хранить историю

    public LayerMask groundLayer;

    private void Start()
    {
        instance = this; // Синглтон
    }

    public bool IsGrounded()
    {
        float rayDistance = 1.1f;
        Vector3 origin = transform.position + Vector3.up * 0.1f;

        // Проверка земли под игроком
        return Physics.Raycast(origin, Vector3.down, rayDistance, groundLayer);
    }

    void Update()
    {
        // ���������� ������� ������ ����, ���������� �� ���������
        if (IsGrounded())
        {
            // Запоминаем позицию
            positionHistory.Add(new PositionRecord(Time.time, transform.position));

            // Чистим старые записи
            while (positionHistory.Count > 0 && Time.time - positionHistory[0].time > historyLength)
            {
                positionHistory.RemoveAt(0);
            }
        }

    }

    public void TriggerWater()
    {
        StartCoroutine(RespawnRoutine()); // Запуск респавна
    }

    IEnumerator RespawnRoutine()
    {
        GetComponent<PlayerMovement>().enabled = false; // Отключаем управление

        yield return new WaitForSeconds(respawnDelay);

        Vector3 targetPos = GetPositionBeforeFall(); // Получаем безопасную позицию

        transform.GetComponent<Rigidbody>().MovePosition(targetPos);

        print(transform.position);
        print(targetPos);

        yield return null;

        print(transform.position);

        yield return new WaitForSeconds(.5f);

        GetComponent<PlayerMovement>().enabled = true; // Возвращаем управление
    }

    public Vector3 GetPositionBeforeFall()
    {
        if (positionHistory.Count == 0)
            return transform.position;

        // ����� �� ������� (������� ����� ����� �������� ��������)
        float fallTime = positionHistory[0].time - respawnDelay;

        // ���� ������� �� 1 ������� �� �������
        float targetTime = fallTime - 1f;

        PositionRecord closestRecord = positionHistory[0];

        // Ищем ближайшую позицию до падения
        foreach (var record in positionHistory)
        {
            if (Mathf.Abs(record.time - targetTime) < Mathf.Abs(closestRecord.time - targetTime))
            {
                closestRecord = record;
            }
        }

        return closestRecord.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        print("TRIGGER " + other.name);

        // Попадание в воду
        if (other.CompareTag("Water"))
        {
            TriggerWater();
        }
    }
}

[System.Serializable]
public class PositionRecord
{
    public float time;
    public Vector3 position;

    public PositionRecord(float t, Vector3 p)
    {
        time = t;
        position = p;
    }
}