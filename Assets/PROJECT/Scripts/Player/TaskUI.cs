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
    public Image bagButton; // ← назначь Image сумки в инспекторе

    public bool hasBag = false;
    private bool isOpen = false;
    private string adressT;

    private void Awake()
    {
        Instance = this;
        taskPanel.SetActive(false);
        LocalizationManager.Instance.OnLanguageChanged += UpdateText;
        reciever.text = "";

        // Скрываем сумку по умолчанию
        if (bagButton != null)
            bagButton.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            TogglePanel();
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
            ClosePanel();
    }

    public void SetHasBag(bool value)
    {
        hasBag = value;
        if (bagButton != null)
            bagButton.gameObject.SetActive(value);
    }

    public void TogglePanel()
    {
        if (!hasBag) return;
        if (isOpen) ClosePanel();
        else OpenPanel();
    }

    public void OpenPanel()
    {
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