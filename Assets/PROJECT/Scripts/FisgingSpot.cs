using UnityEngine;

public class FishingSpot : MonoBehaviour
{
    [Header("References")]
    public GameObject fishingUI;
    public GameObject bobberPrefab;
    public GameObject npcPrefab;
    public Transform castPoint;
    public LayerMask waterLayer;

    [Header("Fishing Settings")]
    public float catchThreshold = 0.8f;
    public float fishSpawnChance = 0.8f;

    [Header("NPC Spawn Settings")]
    public Vector3 npcSpawnPosition = new Vector3(87f, 5f, 67f); // Добавили поле для координат

    private bool isPlayerNear = false;
    private Bobber currentBobber;
    private bool isFishing = false;

    void Update()
    {
        if (isPlayerNear && !isFishing)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                StartFishing();
            }
        }

        if (isFishing && currentBobber != null)
        {
            if (Input.GetKeyDown(KeyCode.E) && currentBobber.GetMovementMagnitude() > catchThreshold)
            {
                SuccessCatch();
            }
        }
    }

    void StartFishing()
    {
        isFishing = true;
        GameObject bobberObj = Instantiate(bobberPrefab, castPoint.position, Quaternion.identity);
        currentBobber = bobberObj.GetComponent<Bobber>();

        // Простой бросок в воду
        RaycastHit hit;
        if (Physics.Raycast(castPoint.position, Vector3.down, out hit, 10f, waterLayer))
        {
            bobberObj.transform.position = hit.point + Vector3.up * 0.1f;
        }
    }

    void SuccessCatch()
    {
        isFishing = false;
        if (currentBobber != null)
            Destroy(currentBobber.gameObject);

        if (Random.Range(0f, 1f) <= fishSpawnChance)
        {
            SpawnNPC();
        }
    }

    void SpawnNPC()
    {
        if (npcPrefab != null)
        {
            // Используем фиксированные координаты из инспектора
            Instantiate(npcPrefab, npcSpawnPosition, Quaternion.identity);
            Debug.Log("NPC появился в позиции: " + npcSpawnPosition);
        }
        else
        {
            Debug.LogError("NPC Prefab не назначен в инспекторе!");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            if (fishingUI != null)
                fishingUI.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            if (fishingUI != null)
                fishingUI.SetActive(false);
        }
    }
}