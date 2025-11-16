using UnityEngine;

public class DialogueRunner : MonoBehaviour
{
    public string owner;
    public DialogueScriptableObject[] dialogues;
    public bool random;

    public DialogueUI dialogueUI;

    int currentDialogueIndex;
    int currentPhraseIndex;

    void Start()
    {
        
    }

    public void StartDialogue()
    {
        currentDialogueIndex = random ? Random.Range(0, dialogues.Length) : 0;
        currentPhraseIndex = 0;
        ShowCurrentPhrase();
    }

    void ShowCurrentPhrase()
    {
        var block = dialogues[currentDialogueIndex];

        if (currentPhraseIndex < block.phrases.Length)
            dialogueUI.ShowPhrase(owner, block.phrases[currentPhraseIndex]);
        else
            dialogueUI.ShowChoices(block.choices);
    }

    public void NextPhrase()
    {
        var block = dialogues[currentDialogueIndex];

        currentPhraseIndex++;

        if (currentPhraseIndex < block.phrases.Length)
            ShowCurrentPhrase();
        else
            dialogueUI.ShowChoices(block.choices);
    }

    public void Choose(int index)
    {
        var block = dialogues[currentDialogueIndex];

        if (index < 0 || index >= block.choices.Length) return;

        int next = block.choices[index].nextDialogueIndex;

        if (next < 0)
        {
            dialogueUI.Hide();
            return;
        }

        if (next >= dialogues.Length) return;

        currentDialogueIndex = next;
        currentPhraseIndex = 0;
        ShowCurrentPhrase();
    }
    private bool isRunning;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isRunning)
            {
                isRunning = true;
                StartDialogue();
            }
            else
            {
                isRunning = false;
                dialogueUI.Hide();
            }
        }
    }

}
