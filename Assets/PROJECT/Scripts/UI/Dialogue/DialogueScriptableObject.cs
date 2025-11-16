using UnityEngine;

[System.Serializable]
public class DialogueChoice
{
    public string text;                // Текст варианта ответа
    public int nextDialogueIndex = -1; // На какой диалог переходить (-1 — конец)
}

[CreateAssetMenu(menuName = "Dialogue/Dialogue Block")]
public class DialogueScriptableObject : ScriptableObject
{
    public string[] phrases;
    public DialogueChoice[] choices;
}
