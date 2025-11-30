using System.Collections.Generic;
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
    public string[] phrases;
    public AudioClip[] voiceOver;
    public DialogueChoice[] choices;
    public List<FaceType> emotions;
}
