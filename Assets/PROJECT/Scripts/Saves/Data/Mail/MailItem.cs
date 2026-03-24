using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MailItem
{
    public string id;
    public string reciever;
    public string adress;
    [Tooltip("—южетное задание Ч показывает звезду в UI")]
    public bool isStory = false;
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