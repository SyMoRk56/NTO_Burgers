using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class RespawnPlayer : MonoBehaviour
{
    public float respawnDelay = 2f;
    private List<PositionRecord> positionHistory = new List<PositionRecord>();

    // Увеличиваем длину истории чтобы хватило на респавн
    public float historyLength = 5f;

    public LayerMask groundLayer;

    public bool IsGrounded()
    {
        float rayDistance = 1.1f;
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        return Physics.Raycast(origin, Vector3.down, rayDistance, groundLayer);
    }

    void Update()
    {
        // Записываем позицию каждый кадр, независимо от состояния
        if (IsGrounded())
        {
            positionHistory.Add(new PositionRecord(Time.time, transform.position));

            // Удаляем старые записи
            while (positionHistory.Count > 0 && Time.time - positionHistory[0].time > historyLength)
            {
                positionHistory.RemoveAt(0);
            }
        }
        
    }

    public void TriggerWater()
    {
        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        GetComponent<PlayerMovement>().enabled = false;
        yield return new WaitForSeconds(respawnDelay);
        Vector3 targetPos = GetPositionBeforeFall();
        transform.GetComponent<Rigidbody>().MovePosition(targetPos);
        print(transform.position);
        print(targetPos);
        yield return null;
        print(transform.position);
        yield return new WaitForSeconds(.5f);
        GetComponent<PlayerMovement>().enabled = true;

    }

    private Vector3 GetPositionBeforeFall()
    {
        if (positionHistory.Count == 0)
            return transform.position;

        // Время до падения (текущее время минус задержка респавна)
        float fallTime = positionHistory[0].time - respawnDelay;

        // Ищем позицию за 1 секунду до падения
        float targetTime = fallTime - 1f;

        PositionRecord closestRecord = positionHistory[0];

        // Находим ближайшую запись ко времени за 1 секунду до падения
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