using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MailItem
{
    public string id;
    public string reciever;
    public string adress;
}

[CreateAssetMenu(menuName = "Mail/Mail Catalog")]
public class MailCatalog : ScriptableObject
{
    [Tooltip("Список писем в нужном порядке")]
    public List<MailItem> mails = new();
}
[System.Serializable]
public class MailState
{
    public string id;
    public bool delivered;
}

[System.Serializable]
public class MailSaveData
{
    public List<MailState> mailStates = new List<MailState>();
}
