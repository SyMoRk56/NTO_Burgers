using TMPro;
using UnityEngine;

public class TaskUI : MonoBehaviour
{
    public static TaskUI Instance;

    [Header("UI References")]
    public GameObject taskCanvas; // Ссылка на канвас задачи
    public GameObject taskPanel; // Ссылка на панель задачи (опционально)
    public TMP_Text reciever;
    public TMP_Text adress;

    private void Awake()
    {
        Instance = this;

        // Скрываем канвас при старте
        if (taskCanvas != null)
            taskCanvas.SetActive(false);
    }

    public void SetTask(Task task)
    {
        // Активируем канвас задачи
        if (taskCanvas != null)
            taskCanvas.SetActive(true);

        // Проверяем на пустые значения и скрываем если нужно
        if (string.IsNullOrEmpty(task.recieverName) || string.IsNullOrEmpty(task.adress))
        {
            if (taskCanvas != null)
                taskCanvas.SetActive(false);
            return;
        }

        // Устанавливаем текст
        reciever.text = task.recieverName;
        adress.text = task.adress;
    }

    // Метод для скрытия канваса задачи (можно вызвать из кнопки закрытия)
    public void HideTask()
    {
        if (taskCanvas != null)
            taskCanvas.SetActive(false);
    }
}