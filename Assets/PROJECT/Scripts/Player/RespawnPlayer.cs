using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class RespawnPlayer : MonoBehaviour
{
    public float respawnDelay = 2f;

    private List<PositionRecord> positionHistory = new List<PositionRecord>();
    private float historyLength = 1.2f; // храним чуть больше чем 1 сек

    public LayerMask groundLayer;

    public bool IsGrounded()
    {
        float rayDistance = 1.1f;
        Vector3 origin = transform.position + Vector3.up * 0.1f;

        return Physics.Raycast(origin, Vector3.down, rayDistance, groundLayer);
    }

    void Update()
    {
        if(IsGrounded())
        // Записываем позицию каждый кадр
        positionHistory.Add(new PositionRecord(Time.time, transform.position));

        // Удаляем старые записи
        while (positionHistory.Count > 1 && Time.time - positionHistory[0].time > historyLength)
        {
            positionHistory.RemoveAt(0);
        }
    }

    public void TriggerWater()
    {
        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        Vector3 targetPos = GetPositionOneSecondAgo();

        transform.position = targetPos;
    }

    private Vector3 GetPositionOneSecondAgo()
    {
        float targetTime = Time.time - 1f;

        PositionRecord best = positionHistory[0];

        // Находим ближайшую запись к времени 1 секунду назад
        foreach (var rec in positionHistory)
        {
            if (Mathf.Abs(rec.time - targetTime) < Mathf.Abs(best.time - targetTime))
            {
                best = rec;
            }
        }

        return best.position;
    }

    private void OnTriggerEnter(Collider other)
    {
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
