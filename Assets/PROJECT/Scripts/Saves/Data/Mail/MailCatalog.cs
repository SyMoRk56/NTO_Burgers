using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Mail/Mail Catalog")]
public class MailCatalog : ScriptableObject
{
    [Tooltip("Список писем в нужном порядке")]
    public List<List<MailItem>> mails { get { return GetMailLists(); } }

    public List<MailGroup> mailGroups = new();
    List<List<MailItem>> GetMailLists()
    {
        var result = new List<List<MailItem>>();

        foreach (var group in mailGroups)
        {
            var m = group.mails;
            //Debug.Log(group.mails);

            //m.Reverse();
            //Debug.Log(m);
            result.Add(m);
        }
        
        return result;
    }
}
[System.Serializable]
public class MailGroup
{
    public List<MailItem> mails;
}