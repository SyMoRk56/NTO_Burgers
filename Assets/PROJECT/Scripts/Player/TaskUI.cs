using TMPro;
using UnityEngine;

public class TaskUI : MonoBehaviour
{
    public static TaskUI Instance;

    [Header("UI References")]
    public GameObject taskCanvas;
    public GameObject taskPanel;
    public TMP_Text reciever;
    [SerializeField] TMP_Text adress;
    public TMP_Text countText;

    public string adressT;
    private void Awake()
    {
        Instance = this;

        if (taskCanvas != null)
            taskCanvas.SetActive(false);

        LocalizationManager.Instance.OnLanguageChanged += () => UpdateText();

        reciever.text = "";
        print("R " + reciever.text);
        
        
    }
    private void OnEnable()
    {
        reciever.text = "";
        print("R OnEn" + reciever.text);

        UpdateText();
    }
    private void Start()
    {
        reciever.text = "";
    }
    public void SetTask(Task task, int remainingCount)
    {
        print("R " + reciever.text);

        print("Set task " + task.recieverName + task.adress + " remaining: " + remainingCount);

        if (taskCanvas != null)
            taskCanvas.SetActive(true);

        if (string.IsNullOrEmpty( task.recieverName) && string.IsNullOrEmpty(task.adress))
        {
            if (taskCanvas != null)
                taskCanvas.SetActive(false);
            return;
        }

        reciever.text = GetRecieverText(task.adress);
        print(GetRecieverText(task.adress));
        adress.text = LocalizationManager.Instance.Get(task.adress);
        adressT = task.adress;
        if (remainingCount > 0)
            countText.text = remainingCount.ToString();
        else
            countText.text = "";
    }
    public void UpdateText()
    {
        print("R " + reciever.text);

        adress.text = LocalizationManager.Instance.Get(adressT);
        reciever.text = GetRecieverText(adressT);

    }
    public string GetRecieverText(string adress)
    {
        print("GetRecieverText: " + adress + " " + LocalizationManager.Instance.Get(adress.Contains("Tutorial") ? "" : (!adress.Contains("NPC") ? "Reciever" : "Reciever_npc")));
        return LocalizationManager.Instance.Get(adress.Contains("Tutorial") ? "" : (!adress.Contains("NPC") ? "Reciever" : "Reciever_npc"));

    }
    public void HideTask()
    {
        if (taskCanvas != null)
            taskCanvas.SetActive(false);
    }
}