using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-10)]
public class MailManager : MonoBehaviour
{
    public static MailManager Instance;

    [Header("Mail Order (ScriptableObject)")]
    public MailCatalog catalog;

    private Dictionary<string, bool> state = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (catalog != null)
        {
            foreach (var mail in catalog.mails)
                state[mail.id] = false;
        }
        else
        {
            Debug.LogError("MailCatalog is not assigned in MailManager!");
        }
    }

    public MailItem GetNextUndelivered()
    {
        foreach (var mail in catalog.mails)
        {
            if (!state[mail.id])
                return mail;
        }
        return null;
    }

    public List<MailItem> GetNextXUndelivered(int count)
    {
        var l = new List<MailItem>();
        foreach (var mail in catalog.mails)
        {
            if (!state[mail.id]) l.Add(mail);
        }
        return l.Take(count).ToList();
    }

    // НОВЫЙ МЕТОД: Получить все недоставленные письма
    public List<MailItem> GetAllUndeliveredMails()
    {
        List<MailItem> undelivered = new List<MailItem>();
        foreach (var mail in catalog.mails)
        {
            if (!state[mail.id])
                undelivered.Add(mail);
        }
        return undelivered;
    }

    // НОВЫЙ МЕТОД: Получить письмо по ID
    public MailItem GetMailById(string id)
    {
        foreach (var mail in catalog.mails)
        {
            if (mail.id == id)
                return mail;
        }
        return null;
    }

    public bool IsDelivered(string id)
    {
        return state.ContainsKey(id) && state[id];
    }

    public void SetDelivered(string id, bool delivered)
    {
        if (state.ContainsKey(id))
            state[id] = delivered;
    }

    public MailSaveData GetSaveData()
    {
        var save = new MailSaveData();

        foreach (var mail in catalog.mails)
        {
            save.mailStates.Add(new MailState
            {
                id = mail.id,
                delivered = state[mail.id]
            });
        }

        return save;
    }

    public void LoadSaveData(MailSaveData save)
    {
        foreach (var s in save.mailStates)
        {
            if (state.ContainsKey(s.id))
                state[s.id] = s.delivered;
        }
    }

    public Task GetMailByID(string id)
    {
        var mailData = catalog?.mails?.Find(m => m.id == id);
        if (mailData == null)
        {
            Debug.LogError($"[MailManager] Письмо с ID {id} не найдено!");
            return default(Task);
        }

        // Преобразуем MailItem в Task (или в твою внутреннюю структуру)
        Task task = new Task
        {
            id = mailData.id,
            recieverName = mailData.reciever,
            adress = mailData.adress,
            isStory = mailData.isStory
        };
        return task;
    }

    public Task GetStoryMailForDay(int day)
    {
        // Берём первое письмо с isStory = true, которое ещё не выдано
        var mailItem = catalog.mails.Find(m => m.isStory);
        if (mailItem == null) return default(Task);

        return new Task
        {
            id = mailItem.id,
            recieverName = mailItem.reciever,
            adress = mailItem.adress,
            isStory = mailItem.isStory
        };
    }

    public List<Task> GetNonStoryMailsForDay(int day)
    {
        List<Task> tasks = new List<Task>();
        var mails = catalog.mails.FindAll(m => !m.isStory);
        foreach (var m in mails)
        {
            tasks.Add(new Task
            {
                id = m.id,
                recieverName = m.reciever,
                adress = m.adress,
                isStory = m.isStory
            });
        }
        return tasks;
    }
}