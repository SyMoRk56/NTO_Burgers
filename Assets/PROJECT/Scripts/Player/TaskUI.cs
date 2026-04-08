using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskUI : MonoBehaviour
{
    public static TaskUI Instance; // Синглтон

    [Header("UI References")]
    public GameObject taskPanel;
    public TMP_Text reciever;
    [SerializeField] TMP_Text adress;
    public TMP_Text countText;
    public Image bagButton;

    public bool hasBag = false;
    private bool isOpen = false;
    private bool isReady = false; // блокировка ввода до загрузки
    private string adressT;

    private float tabHoldTime = 0f;
    private bool tabWasHeld = false;
    private const float HOLD_THRESHOLD = 0.3f; // время удержания Tab

    private void Awake()
    {
        Instance = this;

        taskPanel.SetActive(false); // скрываем панель

        LocalizationManager.Instance.OnLanguageChanged += UpdateText;

        reciever.text = "";

        if (bagButton != null)
            bagButton.gameObject.SetActive(false); // скрываем кнопку сумки
    }

    private void Start()
    {
        StartCoroutine(RestoreAfterLoad()); // восстановление после загрузки
    }

    private System.Collections.IEnumerator RestoreAfterLoad()
    {
        print("RestoreAfterLoad 1");

        // ждём инициализацию игрока
        yield return new WaitForSeconds(2f);

        print("RestoreAfterLoad 2");

        var player = GameManager.Instance?.GetPlayer();

        if (player != null)
        {
            print("RestoreAfterLoad 3");

            // ищем сумку у игрока
            foreach (Transform child in player.transform.GetChild(1))
            {
                if (child.CompareTag("Bag"))
                {
                    SetHasBag(true);
                    break;
                }
            }
        }

        isReady = true; // можно принимать ввод
    }

    private void Update()
    {
        if (!hasBag || !isReady) return;

        // переключение панели по Tab
        if (Input.GetKeyDown(KeyCode.Tab) && (PlayerManager.instance.CanMove || isOpen))
        {
            if (isOpen) ClosePanel();
            else OpenPanel();
        }

        // удержание Tab
        if (Input.GetKey(KeyCode.Tab))
        {
            tabHoldTime += Time.deltaTime;
            tabWasHeld = true;
        }

        // отпускание Tab
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            if (tabWasHeld && tabHoldTime >= HOLD_THRESHOLD)
            {
                if (isOpen) ClosePanel();
            }

            tabHoldTime = 0f;
            tabWasHeld = false;
        }

        // закрытие по Escape
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
            ClosePanel();
    }

    public void SetHasBag(bool value)
    {
        Debug.LogError("SetHasBag " + value);

        hasBag = value;

        if (bagButton != null)
            bagButton.gameObject.SetActive(value); // показываем кнопку
    }

    public void SetHasBagUI(bool value)
    {
        if (bagButton != null)
            bagButton.gameObject.SetActive(value);
    }

    public void OpenPanel()
    {
        if (PlayerManager.instance == null) return;

        taskPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        PlayerManager.instance.CanMove = false; // блокируем движение
        isOpen = true;

        if (AdressListMenu.Instance != null)
            AdressListMenu.Instance.SetVisible(false);

        if (TaskPanel.Instance != null)
            TaskPanel.Instance.Populate(); // заполняем список
    }

    public void ClosePanel()
    {
        if (PlayerManager.instance == null) return;

        taskPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        PlayerManager.instance.CanMove = true; // возвращаем управление
        isOpen = false;

        if (AdressListMenu.Instance != null)
            AdressListMenu.Instance.SetVisible(true);

        if (TaskPanel.Instance != null)
            TaskPanel.Instance.Clear(); // очищаем список
    }

    public void SetTask(Task task, int remainingCount)
    {
        // защита от пустых данных
        if (string.IsNullOrEmpty(task.recieverName) && string.IsNullOrEmpty(task.adress))
            return;

        adressT = task.adress;

        reciever.text = GetRecieverText(task);
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

        // обновляем локализацию получателя
        reciever.text = GetRecieverText(new Task(reciever.text, adressT, ""));
    }

    public string GetRecieverText(Task task)
    {
        if (task.adress.Contains("Tutorial"))
            return "";

        print("TASK RECIEVER " + task.recieverName);

        // выбор текста получателя
        return LocalizationManager.Instance.Get(
            !task.adress.Contains("NPC")
                ? (task.recieverName.Contains("Fish_") ? "Reciever_fish" : "Reciever")
                : "Reciever_npc"
        );
    }
}