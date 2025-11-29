using UnityEngine;

public class DialogueRunner : MonoBehaviour
{
    public string ownerName;
    public DialogueScriptableObject[] defaultDialogues;
    public DialogueScriptableObject[] letterDialogues;
    public bool random;

    public DialogueUI dialogueUI;

    int currentDialogueIndex;
    int currentPhraseIndex;

    public Face face;

    void Start()
    {
        
    }

    public void StartDialogue(bool letter)
    {
        if (!isRunning)
        {
            isLetter = letter;
            currentDialogueIndex = 0;
            currentPhraseIndex = 0;
            dialogueUI.gameObject.SetActive(true);
            ShowCurrentPhrase();
            isRunning = true;
            GameManager.Instance.GetPlayer().GetComponent<PlayerManager>().ShowCursor(true);
            GameManager.Instance.GetPlayer().GetComponent<PlayerManager>().CanMove = false;

        }
        
    }

    void ShowCurrentPhrase()
    {
        print(currentDialogueIndex);
        var block = isLetter ? letterDialogues[currentDialogueIndex] : defaultDialogues[currentDialogueIndex];

        if (currentPhraseIndex < block.phrases.Length)
        {
            dialogueUI.ShowPhrase(ownerName, block.phrases[currentPhraseIndex]);
            face?.SetFace(block.emotions[currentDialogueIndex]);
        }
        else
            dialogueUI.ShowChoices(block.choices);
    }

    public void NextPhrase()
    {
        var block = isLetter ? letterDialogues[currentDialogueIndex] : defaultDialogues[currentDialogueIndex];

        currentPhraseIndex++;

        if (currentPhraseIndex < block.phrases.Length)
            ShowCurrentPhrase();
        else
            dialogueUI.ShowChoices(block.choices);
    }

    public void Choose(int index)
    {
        var block = isLetter ? letterDialogues[currentDialogueIndex] : defaultDialogues[currentDialogueIndex];

        if (index < 0 || index >= block.choices.Length) return;

        int next = block.choices[index].nextDialogueIndex;

        if (next < 0)
        {
            dialogueUI.Hide();
            isRunning = false;
            isLetter = false;
            return;
        }

        if (next >= (isLetter ? letterDialogues.Length : defaultDialogues.Length)) return;

        currentDialogueIndex = next;
        currentPhraseIndex = 0;
        ShowCurrentPhrase();
    }
    bool isLetter;
    private bool isRunning;

   

}
