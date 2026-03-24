using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskUI : MonoBehaviour
{
    public static TaskUI Instance;

    [Header("UI References")]
    public GameObject taskPanel;
    public TMP_Text reciever;
    [SerializeField] TMP_Text adress;
    public TMP_Text countText;
    public Image bagButton;

    public bool hasBag = false;
    private bool isOpen = false;
    private bool isReady = false; // блокируем ввод пока игра не загрузилась
    private string adressT;

    private float tabHoldTime = 0f;
    private bool tabWasHeld = false;
    private const float HOLD_THRESHOLD = 0.3f;

    private void Awake()
    {
        Instance = this;
        taskPanel.SetActive(false);
        LocalizationManager.Instance.OnLanguageChanged += UpdateText;
        reciever.text = "";

        if (bagButton != null)
            bagButton.gameObject.SetActive(false);
    }

    private void Start()
    {
        StartCoroutine(RestoreAfterLoad());
    }

    private System.Collections.IEnumerator RestoreAfterLoad()
    {
        // Ждём пока PlayerManager полностью инициализируется
        yield return new WaitForSeconds(2f);

        var player = GameManager.Instance?.GetPlayer();
        if (player != null)
        {
            foreach (Transform child in player.transform)
            {
                if (child.CompareTag("Bag"))
                {
                    SetHasBag(true);
                    break;
                }
            }
        }

        isReady = true;
    }

    private void Update()
    {
        if (!hasBag || !isReady) return;

        if (Input.GetKeyDown(KeyCode.Tab) && (PlayerManager.instance.CanMove || isOpen))
        {
            if (isOpen) ClosePanel();
            else OpenPanel();
        }
        if (Input.GetKey(KeyCode.Tab))
        {
            tabHoldTime += Time.deltaTime;
            tabWasHeld = true;
        }
        
        if (Input.GetKeyUp(KeyCode.Tab) )
        {
            if (tabWasHeld && tabHoldTime >= HOLD_THRESHOLD)
            {
                if (isOpen) ClosePanel();
            }

            tabHoldTime = 0f;
            tabWasHeld = false;
        }

        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
            ClosePanel();
    }

    public void SetHasBag(bool value)
    {
        hasBag = value;
        if (bagButton != null)
            bagButton.gameObject.SetActive(value);
    }

    public void OpenPanel()
    {
        if (PlayerManager.instance == null) return;
        taskPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        PlayerManager.instance.CanMove = false;
        isOpen = true;
        if (AdressListMenu.Instance != null)
            AdressListMenu.Instance.SetVisible(false);
        if (TaskPanel.Instance != null)
            TaskPanel.Instance.Populate();
    }

    public void ClosePanel()
    {
        if (PlayerManager.instance == null) return;
        taskPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        PlayerManager.instance.CanMove = true;
        isOpen = false;
        if (AdressListMenu.Instance != null)
            AdressListMenu.Instance.SetVisible(true);
        if (TaskPanel.Instance != null)
            TaskPanel.Instance.Clear();
    }

    public void SetTask(Task task, int remainingCount)
    {
        if (string.IsNullOrEmpty(task.recieverName) && string.IsNullOrEmpty(task.adress))
            return;
        adressT = task.adress;
        reciever.text = GetRecieverText(task.adress);
        adress.text = LocalizationManager.Instance.Get(task.adress);
        countText.text = remainingCount > 0 ? remainingCount.ToString() : "";
    }

    public void HideTask()
    {
        adressT = "";
        reciever.text = "";
        adress.text = "";
        countText.text = "";
    }

    public void UpdateText()
    {
        if (string.IsNullOrEmpty(adressT)) return;
        adress.text = LocalizationManager.Instance.Get(adressT);
        reciever.text = GetRecieverText(adressT);
    }

    public string GetRecieverText(string adress)
    {
        if (adress.Contains("Tutorial"))
            return "";
        return LocalizationManager.Instance.Get(!adress.Contains("NPC") ? "Reciever" : "Reciever_npc");
    }
}