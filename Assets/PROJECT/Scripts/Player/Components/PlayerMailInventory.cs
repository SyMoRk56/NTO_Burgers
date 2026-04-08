using UnityEngine;
using System.Collections.Generic;

public class PlayerMailInventory : MonoBehaviour
{
    public static PlayerMailInventory Instance; // Синглтон

    public List<Task> carriedMails = new List<Task>(); // Список писем

    public bool complitedMainLine; // Завершена ли основная линия

    private void Awake()
    {
        // Инициализация синглтона
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Не уничтожается между сценами
        }
        else
        {
            Destroy(Instance.gameObject); // Удаляем старый объект
            Instance = this;
        }
    }

    public bool LastMail(string id)
    {
        return carriedMails[0].id == id; // Проверка первого письма
    }

    public void AddMailToInventory(Task task)
    {
        // Добавляем, если такого нет
        if (!ContainsTask(task.id))
        {
            carriedMails.Add(task);
            UpdateTaskUI(); // Обновляем UI

            Debug.Log($"✓ Письмо добавлено в инвентарь: {task.recieverName} (ID: {task.id}) + {task.isStory}");
            Debug.Log($"  Всего писем в инвентаре: {carriedMails.Count}");
        }
        else
        {
            Debug.LogWarning($"Письмо с ID {task.id} уже в инвентаре!");
        }
    }

    public void RemoveMailFromInventory(string taskId)
    {
        // Удаляем по ID
        int removed = carriedMails.RemoveAll(task => task.id == taskId);

        if (removed > 0)
        {
            UpdateTaskUI();
            Debug.Log($"✓ Письмо с ID {taskId} удалено из инвентаря");
            Debug.Log($"  Осталось писем: {carriedMails.Count}");
        }
        else
        {
            Debug.LogWarning($"Письмо с ID {taskId} не найдено в инвентаре!");
        }
    }

    public bool HasMailForAddress(string address)
    {
        // Проверка наличия письма по адресу
        bool hasMail = carriedMails.Exists(task =>
            NormalizeAddress(task.adress) == NormalizeAddress(address));

        Debug.Log($"Поиск письма для адреса '{address}': {(hasMail ? "НАЙДЕНО" : "НЕ НАЙДЕНО")}");
        return hasMail;
    }

    public Task GetMailForAddress(string address)
    {
        // Поиск письма по адресу
        var mail = carriedMails.Find(task =>
            NormalizeAddress(task.adress) == NormalizeAddress(address));

        if (mail.adress != null)
        {
            Debug.Log($"✓ Найдено письмо для адреса '{address}': {mail.recieverName}");
        }
        else
        {
            Debug.Log($"✗ Письмо для адреса '{address}' не найдено");
        }

        return mail;
    }

    public List<Task> GetAllMails() => new List<Task>(carriedMails); // Копия списка

    public bool ContainsTask(string taskId)
    {
        return carriedMails.Exists(task => task.id == taskId); // Проверка по ID
    }

    private void UpdateTaskUI()
    {
        Debug.Log($"[UpdateTaskUI] вызван, писем: {carriedMails.Count}");
        Debug.Log($"[UpdateTaskUI] TaskUI.Instance: {TaskUI.Instance}");
        Debug.Log($"[UpdateTaskUI] AdressListMenu.Instance: {AdressListMenu.Instance}");

        // Обновление текущего задания
        if (carriedMails.Count > 0 && TaskUI.Instance != null)
        {
            var remain = 0;

            foreach (var task in carriedMails)
            {
                if (!task.adress.Contains("Tutorial"))
                    remain++; // Считаем обычные задания
            }

            TaskUI.Instance.SetTask(carriedMails[0], remain);
        }
        else if (TaskUI.Instance != null)
        {
            TaskUI.Instance.HideTask(); // Скрываем UI
        }

        // Обновление списка адресов
        if (AdressListMenu.Instance != null)
        {
            Debug.Log("[UpdateTaskUI] вызываем AdressListMenu.UpdateTasks()");
            AdressListMenu.Instance.UpdateTasks();
        }
        else
        {
            Debug.LogError("[UpdateTaskUI] AdressListMenu.Instance == null!");
        }
    }

    // Нормализация адреса для сравнения
    private string NormalizeAddress(string address)
    {
        if (string.IsNullOrEmpty(address))
            return "";

        // Убираем пробелы, точки, запятые и приводим к нижнему регистру
        return address.Trim().ToLower().Replace(" ", "").Replace(".", "").Replace(",", "");
    }

    public Task GetNextMailForDelivery()
    {
        return carriedMails.Count > 0 ? carriedMails[0] : default(Task); // Первое письмо
    }

    public bool TryGetNextMailForDelivery(out Task nextMail)
    {
        // Безопасное получение первого письма
        if (carriedMails.Count > 0)
        {
            nextMail = carriedMails[0];
            return true;
        }
        else
        {
            nextMail = default(Task);
            return false;
        }
    }

    public void RemoveFirstMail()
    {
        if (carriedMails.Count > 0)
        {
            // Проверка на сюжетное письмо
            if (carriedMails[0].recieverName == "NPC_koala")
            {
                complitedMainLine = true;
                PlayerManager.instance.SetThunder(false);
            }

            carriedMails.RemoveAt(0); // Удаляем первое письмо
            UpdateTaskUI();
        }
    }

    // Методы для сохранения/загрузки
    public InventorySaveData GetSaveData()
    {
        var saveData = new InventorySaveData();
        saveData.carriedMails = new List<Task>(carriedMails); // Копируем список
        return saveData;
    }

    public void LoadSaveData(InventorySaveData saveData)
    {
        if (saveData != null && saveData.carriedMails != null)
        {
            carriedMails = new List<Task>(saveData.carriedMails);
            UpdateTaskUI();
            Debug.Log($"Загружено {carriedMails.Count} писем в инвентарь");
        }
    }

    // ДОБАВЛЕНО: Метод для очистки инвентаря
    public void ClearInventory()
    {
        carriedMails.Clear(); // Очищаем список
        Debug.Log("Инвентарь писем очищен");

        // Обновляем UI
        if (TaskUI.Instance != null)
        {
            TaskUI.Instance.HideTask();
        }
    }

    // Метод для отладки
    public void DebugInventory()
    {
        Debug.Log($"=== ИНВЕНТАРЬ ИГРОКА ===");
        Debug.Log($"Всего писем в инвентаре: {carriedMails.Count}");

        foreach (var mail in carriedMails)
        {
            Debug.Log($" - {mail.recieverName} -> {mail.adress} (ID: {mail.id})");
        }
    }
}