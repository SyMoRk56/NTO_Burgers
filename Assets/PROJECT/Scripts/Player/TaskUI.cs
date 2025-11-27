using TMPro;
using UnityEngine;

public class TaskUI : MonoBehaviour
{
    public static TaskUI Instance;

    [Header("UI References")]
    public GameObject taskCanvas;
    public GameObject taskPanel;
    public TMP_Text reciever;
    public TMP_Text adress;
    public TMP_Text countText;

    private void Awake()
    {
        Instance = this;

        if (taskCanvas != null)
            taskCanvas.SetActive(false);
    }

    public void SetTask(Task task, int remainingCount)
    {
        print("Set task " + task.recieverName + task.adress + " remaining: " + remainingCount);

        if (taskCanvas != null)
            taskCanvas.SetActive(true);

        if (string.IsNullOrEmpty(task.recieverName) && string.IsNullOrEmpty(task.adress))
        {
            if (taskCanvas != null)
                taskCanvas.SetActive(false);
            return;
        }

        reciever.text = task.recieverName;
        adress.text = task.adress;

        // Исправлено: показываем количество только если есть дополнительные письма
        if (remainingCount > 0)
            countText.text = remainingCount.ToString();
        else
            countText.text = ""; // Пустая строка вместо 0 или отрицательных чисел
    }

    public void HideTask()
    {
        if (taskCanvas != null)
            taskCanvas.SetActive(false);
    }
}