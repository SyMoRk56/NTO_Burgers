using UnityEngine;
using UnityEngine.UI;

public class TaskPanel : MonoBehaviour
{
    public static TaskPanel Instance;

    [Header("Left - Letters")]
    public Transform lettersContainer;
    public GameObject letterPrefab; // префаб с DeskLetterUI

    [Header("Right - Map")]
    public RectTransform mapRect;   // RectTransform MapImage
    public RectTransform playerDot; // RectTransform PlayerDot

    [Header("Map Bounds (мировые координаты)")]
    public Vector2 mapWorldMin; // левый нижний угол карты в мире
    public Vector2 mapWorldMax; // правый верхний угол карты в мире

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (gameObject.activeSelf)
            UpdatePlayerDot();
    }

    // Вызывается при открытии панели
    public void Populate()
    {
        SpawnLetters();
    }

    // Вызывается при закрытии панели
    public void Clear()
    {
        for (int i = lettersContainer.childCount - 1; i >= 0; i--)
            Destroy(lettersContainer.GetChild(i).gameObject);
    }

    private void SpawnLetters()
    {
        Clear();

        var mails = PlayerMailInventory.Instance.carriedMails;

        // Получаем размеры зоны спавна
        Rect containerRect = ((RectTransform)lettersContainer).rect;
        float padding = 50f;
        float minX = containerRect.xMin + padding;
        float maxX = containerRect.xMax - padding;
        float minY = containerRect.yMin + padding;
        float maxY = containerRect.yMax - padding;

        foreach (var task in mails)
        {
            if (task.adress.Contains("Tutorial")) continue;

            var go = Instantiate(letterPrefab, lettersContainer);
            var rect = go.GetComponent<RectTransform>();

            // Ставим в рандомную позицию
            rect.anchoredPosition = new Vector2(
                Random.Range(minX, maxX),
                Random.Range(minY, maxY)
            );

            var letterUI = go.GetComponent<DeskLetterUI>();
            if (letterUI != null)
            {
                letterUI.recipient = task.recieverName;
                letterUI.address = task.adress;
                letterUI.id = task.id;
            }
        }
    }

    private void UpdatePlayerDot()
    {
        if (playerDot == null || mapRect == null) return;

        var player = PlayerManager.instance?.transform;
        if (player == null) return;

        // Нормализуем позицию игрока относительно границ карты
        float normX = Mathf.InverseLerp(mapWorldMin.x, mapWorldMax.x, player.position.x);
        float normY = 1f - Mathf.InverseLerp(mapWorldMin.y, mapWorldMax.y, player.position.z);

        // Переводим в локальные координаты mapRect
        Vector2 mapSize = mapRect.rect.size;
        playerDot.anchoredPosition = new Vector2(
            (normX - 0.5f) * mapSize.x,
            (normY - 0.5f) * mapSize.y
        );
    }
}