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
        if (carriedMails.Count == 0) return false;
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

        // ✅ Для struct: возвращаем default если не найдено
        if (!string.IsNullOrEmpty(mail.id)) // или просто: if (!string.IsNullOrEmpty(mail.id))
        {
            Debug.Log($"✓ Найдено письмо для адреса '{address}': {mail.recieverName}");
            return mail;
        }

        Debug.Log($"✗ Письмо для адреса '{address}' не найдено");
        return default(Task); // ✅ Важно для struct!
    }

    public List<Task> GetAllMails() => new List<Task>(carriedMails);

    public bool ContainsTask(string taskId)
    {
        return carriedMails.Exists(task => task.id == taskId);
    }

    public void GiveTutorialMailsAtStart()
    {
        // ✅ Очищаем инвентарь перед выдачей
        ClearInventory();

        string[] tutorialIds = { "Tutorial_0", "Tutorial_1", "Tutorial_2", "Tutorial_3" };

        foreach (var id in tutorialIds)
        {
            Task mail = MailManager.Instance.GetMailByID(id);
            if (!string.IsNullOrEmpty(mail.id))
            {
                AddMailToInventory(mail);
            }
            else
            {
                Debug.LogError($"[GiveTutorialMailsAtStart] Письмо с ID {id} не найдено в MailManager.catalog!");
            }
        }

        Debug.Log("[GiveTutorialMailsAtStart] Первые 4 письма туториала добавлены в инвентарь.");
    }

    private void UpdateTaskUI()
    {
        Debug.Log($"[UpdateTaskUI] вызван, писем: {carriedMails.Count}");

        // Обновляем список квестов если существует
        QuestListMenu[] listMenus = FindObjectsOfType<QuestListMenu>();
        foreach (var menu in listMenus)
        {
            menu.RefreshList();
        }

        Debug.Log($"[UpdateTaskUI] вызван, писем: {carriedMails.Count}");
        Debug.Log($"[UpdateTaskUI] TaskUI.Instance: {TaskUI.Instance}");
        Debug.Log($"[UpdateTaskUI] AdressListMenu.Instance: {AdressListMenu.Instance}");

        if (carriedMails.Count > 0 && TaskUI.Instance != null)
        {
            var remain = 0;
            foreach (var task in carriedMails)
            {
                if (!task.adress.Contains("Tutorial"))
                    remain++;
            }
            TaskUI.Instance.SetTask(carriedMails[0], remain);
        }
        else if (TaskUI.Instance != null)
        {
            TaskUI.Instance.HideTask();
        }

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
            string mailId = carriedMails[0].id;

            if (carriedMails[0].recieverName == "NPC_koala")
            {
                complitedMainLine = true;
                PlayerManager.instance.SetThunder(false);
            }

            // ✅ Отмечаем письмо как доставленное
            if (DailyMailScheduler.Instance != null)
            {
                DailyMailScheduler.Instance.MarkMailAsDelivered(mailId);
            }

            carriedMails.RemoveAt(0);
            UpdateTaskUI();

            // ✅ Проверка: все ли доставлено?
            if (carriedMails.Count == 0)
            {
                CheckNoMailsAvailable();
            }
        }
    }

    // ✅ Новый метод для проверки
    private void CheckNoMailsAvailable()
    {
        if (DailyMailScheduler.Instance == null)
        {
            DailyMailScheduler.Instance = FindObjectOfType<DailyMailScheduler>();
            if (DailyMailScheduler.Instance == null) return;
        }

        var availableOnDesk = DailyMailScheduler.Instance.GetAvailableForDesk();

        if (availableOnDesk.Count == 0)
        {
            Debug.Log("--------------------------------------------------");
            Debug.Log("----------------на сегодня письма закончились----------------");
            Debug.Log("--------------------------------------------------");
            Debug.Log("Вернитесь домой и вздремните на кровати чтобы наступил новый день!");
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

    // Метод для очистки инвентаря
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