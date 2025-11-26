using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-10)]
public class MailManager : MonoBehaviour
{
    public static MailManager Instance;

    [Header("Mail Order (ScriptableObject)")]
    public MailCatalog catalog;

    // внутреннее состояние писем
    private Dictionary<string, bool> state = new();

    private void Awake()
    {
        Instance = this;
        //catalog = Resources.LoadAll<MailCatalog>("")[0];
        if(catalog != null )
        // Инициализация — все письма недоставлены
        foreach (var mail in catalog.mails)
            state[mail.id] = false;
        
    }


    // ==========================
    //       ПОЛУЧЕНИЕ ДАННЫХ
    // ==========================

    /// Получить следующее недоставленное письмо
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
            if(!state[mail.id]) l.Add(mail);
        }
        print(l.Count);
        return l.Take(count).ToList();
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


    // ==========================
    //         SAVE
    // ==========================

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


    // ==========================
    //         LOAD
    // ==========================

    public void LoadSaveData(MailSaveData save)
    {
        foreach (var s in save.mailStates)
        {
            if (state.ContainsKey(s.id))
                state[s.id] = s.delivered;
        }
    }
}