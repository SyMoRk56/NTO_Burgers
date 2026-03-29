using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskPanel : MonoBehaviour
{
    public static TaskPanel Instance;

    [Header("Left - Letters")]
    public Transform lettersContainer;
    public GameObject letterPrefab; 

    [Header("Right - Map")]
    public RectTransform mapRect;   
    public RectTransform playerDot;

    public RectTransform adressDot;

    [Header("Map Bounds (мировые координаты)")]
    public Vector2 mapWorldMin; 
    public Vector2 mapWorldMax;

    public TMP_Text moneyText;
    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (gameObject.activeSelf)
        {
            
            UpdatePlayerDot();
            UpdateAdressDot();
        }
            
    }

    public void Populate()
    {
        SpawnLetters();
    }

    public void Clear()
    {
        for (int i = lettersContainer.childCount - 1; i >= 0; i--)
            Destroy(lettersContainer.GetChild(i).gameObject);
    }
    private void OnEnable()
    {
        UpdatePlayerDot();
        UpdateAdressDot();
        UpdateMoney();
        FindFirstObjectByType<AdressListMenu>().transform.GetChild(0).gameObject.SetActive(false);
        TaskUI.Instance.bagButton.gameObject.SetActive(false);
    }
    private void OnDisable()
    {
       FindFirstObjectByType<AdressListMenu>().transform.GetChild(0).gameObject.SetActive(true);
        TaskUI.Instance.bagButton.gameObject.SetActive(true);


    }
    void UpdateMoney()
    {
        moneyText.text = PlayerManager.instance.money.ToString()+ " " + LocalizationManager.Instance.Get("Money");
    }
    private void SpawnLetters()
    {
        Clear();

        var mails = PlayerMailInventory.Instance.carriedMails;

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
                letterUI.isStory = task.isStory;
            }
            break;
        }
    }
    private void UpdatePlayerDot()
    {
        if (playerDot == null || mapRect == null) return;

        var player = PlayerManager.instance?.transform;
        if (player == null) return;
        if (player.position.magnitude > 1000) player = GameObject.Find("Cube.004Window").transform;
        float normX = Mathf.InverseLerp(mapWorldMin.x, mapWorldMax.x, player.position.x);
        float normY = 1f - Mathf.InverseLerp(mapWorldMin.y, mapWorldMax.y, player.position.z);

        Vector2 mapSize = mapRect.rect.size;
        playerDot.anchoredPosition = new Vector2(
            (normX - 0.5f) * mapSize.x - playerDot.rect.width/2,
            (normY - 0.5f) * mapSize.y
        );
    }
    public void UpdateAdressDot()
    {
        bool broke = false;
        foreach(var task in PlayerMailInventory.Instance.carriedMails)
        {
            if (!task.isStory) continue;
            foreach(var mailbox in FindObjectsByType<MailBox>(FindObjectsSortMode.None))
            {
                if(mailbox.mailboxAddress == task.adress)
                {
                    float normX = Mathf.InverseLerp(mapWorldMin.x, mapWorldMax.x, mailbox.transform.position.x);
                    float normY = 1f - Mathf.InverseLerp(mapWorldMin.y, mapWorldMax.y, mailbox.transform.position.z);

                    Vector2 mapSize = mapRect.rect.size;
                    adressDot.anchoredPosition = new Vector2(
                        (normX - 0.5f) * mapSize.x /*+ adressDot.rect.width * .5f*/ - 10,
                        (normY - 0.5f) * mapSize.y + adressDot.rect.height * .5f - 10
                    );
                    broke = true;
                    break;
                }
            }
            if (broke) break;
        }
        if (!broke)
        {
            
            foreach (var task in PlayerMailInventory.Instance.carriedMails)
            {
                foreach (var mailbox in FindObjectsByType<MailBox>(FindObjectsSortMode.None))
                {
                    if (mailbox.mailboxAddress == task.adress)
                    {
                        float normX = Mathf.InverseLerp(mapWorldMin.x, mapWorldMax.x, mailbox.transform.position.x);
                        float normY = 1f - Mathf.InverseLerp(mapWorldMin.y, mapWorldMax.y, mailbox.transform.position.z);

                        Vector2 mapSize = mapRect.rect.size;
                        adressDot.anchoredPosition = new Vector2(
                            (normX - 0.5f) * mapSize.x /*+ adressDot.rect.width * .5f*/ - 10,
                            (normY - 0.5f) * mapSize.y + adressDot.rect.height * .5f - 10
                        );
                        broke = true;
                        break;
                    }
                }
                if (broke){ adressDot.anchoredPosition = new Vector2(1000, 1000); break; }
            }
        }
    }
}