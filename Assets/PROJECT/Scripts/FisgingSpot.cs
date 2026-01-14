using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FishingSpot : MonoBehaviour, IInteractObject
{
    [Header("References")]
    public Transform fishingRod;
    public GameObject fishPrefab;
    public Transform fishSpawnPoint;
    public FishingMinigame fishingMinigame;
    public GameObject fishingCanvas;
    public TMPro.TMP_Text cooldownText;

    [Header("Rod Settings")]
    public Vector3 rodStartRotation;
    public Vector3 rodStartPosition;
    public Vector3 rodCastRotation;
    public Vector3 rodCastPosition;
    public float rodAnimationDuration = 0.3f;

    [Header("Fish Settings")]
    public float fishFlyDuration = 2f;
    public float minFishSize = 0.2f;
    public float maxFishSize = 3f;
    public float maxGrowthTime = 10f;

    [Header("Cooldown")]
    public float cooldownDuration = 30f;
    public Color activeColor = Color.white;
    public Color cooldownColor = Color.red;

    private bool isPlayerInRange = false;
    private bool isFishingActive = false;
    private bool isRodCast = false;
    private bool isOnCooldown = false;
    private float cooldownEndTime;
    private GameObject currentPlayer;
    private Coroutine rodAnimationCoroutine;

    [HideInInspector]
    public bool isFishingEnding = false;

    void Start()
    {
        currentPlayer = PlayerManager.instance.gameObject;
    }
    void Update()
    {
        if (isOnCooldown)
            UpdateCooldownUI();

        if (!isPlayerInRange || currentPlayer == null) return;
        return;
        if (Input.GetKeyDown(KeyCode.E))
        {
            Fish();
            return;
        }
    }

    private void Fish()
    {
        if (!isFishingActive)
        {
            StartFishing();
            CastRod();
        }
            
        //else if (!isRodCast)
            
    }

    void StartFishing()
    {
        currentPlayer.GetComponent<PlayerManager>().CanMove = false;

        fishingRod.localPosition = rodStartPosition;
        fishingRod.localEulerAngles = rodStartRotation;

        isFishingActive = true;
        isRodCast = false;

        Debug.Log("Рыбалка начата! Нажмите E для заброса.");
    }

    void CastRod()
    {
        fishingCanvas.SetActive(true);
        fishingMinigame.gameObject.SetActive(true);
        fishingMinigame.OnFinish += OnMinigameFinished;

        rodAnimationCoroutine = StartCoroutine(AnimateRod(rodCastRotation, rodCastPosition, rodAnimationDuration));

        isRodCast = true;
        Debug.Log("Удочка заброшена! Мини-игра началась.");
    }

    void OnMinigameFinished(bool success, FishScriptableObject fish)
    {
        fishingCanvas.SetActive(false);

        if (success)
        {
            SpawnFish(fish.prefab);
        }

        EndFishing();
    }

    void SpawnFish(GameObject fishPrefab)
    {
        if (fishPrefab == null || fishSpawnPoint == null) return;

        GameObject fish = Instantiate(fishPrefab, fishSpawnPoint.position, Quaternion.identity);
        float size = Random.Range(minFishSize, maxFishSize);
        fish.transform.localScale = Vector3.one * size;

        StartCoroutine(FlyFish(fish));
        Destroy(fish, 20f);
        Debug.Log($"Рыба поймана! Размер: {size:F2}");
    }

    IEnumerator FlyFish(GameObject fish)
    {
        Vector3 startPos = fish.transform.position;
        Vector3 targetPos = currentPlayer.transform.position + Vector3.up * 1.5f;

        float time = 0f;
        while (time < fishFlyDuration)
        {
            fish.transform.position = Vector3.Lerp(startPos, targetPos, time / fishFlyDuration);
            time += Time.deltaTime;
            yield return null;
        }

        fish.transform.position = targetPos;
    }

    void EndFishing()
    {
        currentPlayer.GetComponent<PlayerManager>().CanMove = true;
        isFishingActive = false;
        isRodCast = false;

        isFishingEnding = true; // <-- флаг для NPC

        StartCooldown();
        rodAnimationCoroutine = StartCoroutine(AnimateRod(rodStartRotation, rodStartPosition, rodAnimationDuration));
        Debug.Log("Рыбалка завершена! Кулдаун активен.");

        // Сбрасываем через секунду, чтобы NPC успел среагировать
        StartCoroutine(ResetFishingEndingAfterDelay(1f));
    }

    IEnumerator ResetFishingEndingAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isFishingEnding = false;
    }


    IEnumerator AnimateRod(Vector3 targetRotation, Vector3 targetPosition, float duration)
    {
        Vector3 startRot = fishingRod.localEulerAngles;
        Vector3 startPos = fishingRod.localPosition;

        float time = 0f;
        while (time < duration)
        {
            fishingRod.localEulerAngles = Vector3.Lerp(startRot, targetRotation, time / duration);
            fishingRod.localPosition = Vector3.Lerp(startPos, targetPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        fishingRod.localEulerAngles = targetRotation;
        fishingRod.localPosition = targetPosition;
    }

    void StartCooldown()
    {
        isOnCooldown = true;
        cooldownEndTime = Time.time + cooldownDuration;
        if (cooldownText != null)
        {
            cooldownText.gameObject.SetActive(true);
            cooldownText.color = cooldownColor;
        }

        StartCoroutine(CooldownCoroutine());
    }

    IEnumerator CooldownCoroutine()
    {
        while (Time.time < cooldownEndTime)
        {
            UpdateCooldownUI();
            yield return new WaitForSeconds(0.2f);
        }

        isOnCooldown = false;
        if (cooldownText != null)
            cooldownText.gameObject.SetActive(false);
    }

    void UpdateCooldownUI()
    {
        if (cooldownText == null) return;
        float timeLeft = Mathf.Clamp(cooldownEndTime - Time.time, 0f, cooldownDuration);

        if (timeLeft <= 0f)
        {
            cooldownText.text = "Можно рыбачить!";
            cooldownText.color = activeColor;
        }
        else
        {
            cooldownText.text = $"{Mathf.CeilToInt(timeLeft)}";
            cooldownText.color = cooldownColor;
        }
    }

   

    public int InteractPriority()
    {
        return 0;
    }

    public bool CheckInteract()
    {
        return (!isRodCast || !isFishingActive) && !isOnCooldown;
    }

    public void Interact()
    {
        Fish();
    }

    public void OnBeginInteract()
    {
        
    }

    public void OnEndInteract(bool success)
    {
        
    }
}
    