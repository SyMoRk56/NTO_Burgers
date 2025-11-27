using UnityEngine;

[System.Serializable]
public class DialogueChoice
{
    public string text;              // теперь ключ, не текст
    public int nextDialogueIndex = -1;
}


[CreateAssetMenu(menuName = "Dialogue/Dialogue Block")]
public class DialogueScriptableObject : ScriptableObject
{
    public string[] phrases;         // фразы в виде KEY
    public DialogueChoice[] choices;
}
