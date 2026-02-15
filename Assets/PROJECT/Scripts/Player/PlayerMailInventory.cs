using UnityEngine;
using System.Collections.Generic;

public class PlayerMailInventory : MonoBehaviour
{
    public static PlayerMailInventory Instance;

    public List<Task> carriedMails = new List<Task>();

    public bool complitedMainLine;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(Instance.gameObject);
            Instance = this;
        }
    }

    public bool LastMail(string id)
    {
        return carriedMails[0].id == id;
    }

    public void AddMailToInventory(Task task)
    {
        if (!ContainsTask(task.id))
        {
            carriedMails.Add(task);
            UpdateTaskUI();
            Debug.Log($"✓ Письмо добавлено в инвентарь: {task.recieverName} (ID: {task.id})");
            Debug.Log($"  Всего писем в инвентаре: {carriedMails.Count}");
        }
        else
        {
            Debug.LogWarning($"Письмо с ID {task.id} уже в инвентаре!");
        }
    }

    public void RemoveMailFromInventory(string taskId)
    {
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
        bool hasMail = carriedMails.Exists(task =>
            NormalizeAddress(task.adress) == NormalizeAddress(address));

        Debug.Log($"Поиск письма для адреса '{address}': {(hasMail ? "НАЙДЕНО" : "НЕ НАЙДЕНО")}");
        return hasMail;
    }

    public Task GetMailForAddress(string address)
    {
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

    public List<Task> GetAllMails() => new List<Task>(carriedMails);

    public bool ContainsTask(string taskId)
    {
        return carriedMails.Exists(task => task.id == taskId);
    }

    private void UpdateTaskUI()
    {
        if (carriedMails.Count > 0 && TaskUI.Instance != null)
        {
            var remain = 0;
            foreach (var task in carriedMails)
            {
                print("AD "+task.adress);
                if (!task.adress.Contains("Tutorial"))
                {
                    remain ++;  
                }
            }
            TaskUI.Instance.SetTask(carriedMails[0], remain);
            Debug.Log($"✓ UI обновлено: {carriedMails[0].recieverName} (+{carriedMails.Count - 1} других)");
        }
        else if (TaskUI.Instance != null)
        {
            TaskUI.Instance.HideTask();
            Debug.Log("✓ UI скрыто - нет писем");
        }
    }

    // Нормализация адреса для сравнения
    private string NormalizeAddress(string address)
    {
        if (string.IsNullOrEmpty(address))
            return "";

        return address.Trim().ToLower().Replace(" ", "").Replace(".", "").Replace(",", "");
    }

    public Task GetNextMailForDelivery()
    {
        return carriedMails.Count > 0 ? carriedMails[0] : default(Task);
    }

    public bool TryGetNextMailForDelivery(out Task nextMail)
    {
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
            if (carriedMails[0].recieverName == "NPC_koala")
            {
                complitedMainLine = true;
                PlayerManager.instance.SetThunder(false);
            }
            carriedMails.RemoveAt(0);
            UpdateTaskUI();
            
        }
    }

    // Методы для сохранения/загрузки
    public InventorySaveData GetSaveData()
    {
        var saveData = new InventorySaveData();
        saveData.carriedMails = new List<Task>(carriedMails);
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
        carriedMails.Clear();
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