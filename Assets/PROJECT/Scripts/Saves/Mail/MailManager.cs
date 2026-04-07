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
            try
            {
                foreach (var mail in catalog.mails[PlayerManager.instance.Day])
                    state[mail.id] = false;
            }
            catch
            {

            }
        }
        else
        {
            Debug.LogError("MailCatalog is not assigned in MailManager!");
        }
    }

    public MailItem GetNextUndelivered()
    {
        foreach (var mail in catalog.mails[PlayerManager.instance.Day])
        {
            if (!state[mail.id])
                return mail;
        }
        return null;
    }

    public List<MailItem> GetNextXUndelivered(int count)
    {
        var l = new List<MailItem>();
        foreach (var mail in catalog.mails[PlayerManager.instance.Day])
        {
            if (!state[mail.id]) l.Add(mail);
        }
        return l.Take(count).ToList();
    }

    // НОВЫЙ МЕТОД: Получить все недоставленные письма
    public List<MailItem> GetAllUndeliveredMails()
    {
        List<MailItem> undelivered = new List<MailItem>();
        foreach (var mail in catalog.mails[PlayerManager.instance.Day])
        {
            if (!state[mail.id])
                undelivered.Add(mail);
        }
        return undelivered;
    }

    // НОВЫЙ МЕТОД: Получить письмо по ID
    public MailItem GetMailById(string id)
    {
        foreach (var mail in catalog.mails[PlayerManager.instance.Day])
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

        foreach (var mail in catalog.mails[PlayerManager.instance.Day])
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
}