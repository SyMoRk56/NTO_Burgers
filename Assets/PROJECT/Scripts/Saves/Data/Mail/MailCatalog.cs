using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Mail/Mail Catalog")]
public class MailCatalog : ScriptableObject
{
    [Tooltip("Список писем в нужном порядке")]
    public List<MailItem> mails = new();
}
