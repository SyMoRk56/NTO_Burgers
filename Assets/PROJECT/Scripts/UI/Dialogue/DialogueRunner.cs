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

    public bool IsDialogueActive => isRunning;

    bool isLetter;
    private bool isRunning;
    private bool isChoosing = false;

    void Update()
    {
        if (!isRunning) return;

        var block = isLetter ? letterDialogues[currentDialogueIndex] : defaultDialogues[currentDialogueIndex];
        bool isAtChoicePoint = currentPhraseIndex >= block.phrases.Length;

        if (!isChoosing && !isAtChoicePoint &&
           (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.E) ||
            Input.GetMouseButtonDown(0)))
        {
            NextPhrase();
        }
    }

    public void StartDialogue(bool letter)
    {
        if (isRunning)
            ForceCloseDialogue();

        if (!isRunning)
        {
            // 🔥 Останавливаем NPC
            var npc = GetComponent<NPCBehaviour>();
            if (npc != null)
            {
                npc.dialogueActive = true;
                npc.Stop();
            }

            isLetter = letter;
            currentDialogueIndex = 0;
            currentPhraseIndex = 0;
            isChoosing = false;

            var dialogues = letter ? letterDialogues : defaultDialogues;
            if (dialogues.Length == 0)
            {
                Debug.LogError($"No dialogues found for type: letter={letter}");
                return;
            }

            dialogueUI = GetComponentInChildren<DialogueUI>(true);
            dialogueUI.gameObject.SetActive(true);
            dialogueUI.nameText.text = LocalizationManager.Instance.Get(ownerName);

            ShowCurrentPhrase();
            isRunning = true;

            var player = GameManager.Instance.GetPlayer();
            if (player != null)
            {
                var playerManager = player.GetComponent<PlayerManager>();
                if (playerManager != null)
                {
                    playerManager.ShowCursor(true);
                    playerManager.CanMove = false;
                }
            }
        }
    }

    void ShowCurrentPhrase()
    {
        isChoosing = false;

        var block = isLetter ? letterDialogues[currentDialogueIndex] : defaultDialogues[currentDialogueIndex];

        if (currentPhraseIndex < block.phrases.Length)
        {
            dialogueUI.ShowPhrase(ownerName, block.phrases[currentPhraseIndex]);

            if (face != null)
            {
                if (block.emotions.Count != 0 || currentDialogueIndex < block.emotions.Count)
                {
                    face.SetFace(block.emotions[currentDialogueIndex]);
                }
            }
        }
        else
        {
            isChoosing = true;
            dialogueUI.ShowChoices(block.choices);
        }
    }

    public void NextPhrase()
    {
        var block = isLetter ? letterDialogues[currentDialogueIndex] : defaultDialogues[currentDialogueIndex];

        currentPhraseIndex++;

        if (currentPhraseIndex < block.phrases.Length)
            ShowCurrentPhrase();
        else
        {
            isChoosing = true;
            dialogueUI.ShowChoices(block.choices);
        }
    }

    public void Choose(int index)
    {
        isChoosing = false;

        var block = isLetter ? letterDialogues[currentDialogueIndex] : defaultDialogues[currentDialogueIndex];

        if (index < 0 || index >= block.choices.Length) return;

        int next = block.choices[index].nextDialogueIndex;

        if (next < 0)
        {
            EndDialogue();
            return;
        }

        if (next >= (isLetter ? letterDialogues.Length : defaultDialogues.Length))
            return;

        currentDialogueIndex = next;
        currentPhraseIndex = 0;
        ShowCurrentPhrase();
    }

    public void EndDialogue()
    {
        dialogueUI.Hide();
        isRunning = false;
        isLetter = false;

        // 🔥 Возобновляем NPC
        var npc = GetComponent<NPCBehaviour>();
        if (npc != null)
        {
            npc.dialogueActive = false;
            npc.Resume();
        }
    }

    public void ForceCloseDialogue()
    {
        if (!isRunning) return;

        isRunning = false;
        isLetter = false;
        isChoosing = false;
        currentDialogueIndex = 0;
        currentPhraseIndex = 0;

        if (dialogueUI != null)
            dialogueUI.ForceHide();

        // 🔥 Возобновляем NPC
        var npc = GetComponent<NPCBehaviour>();
        if (npc != null)
        {
            npc.dialogueActive = false;
            npc.Resume();
        }
    }

    private void Start()
    {
        dialogueUI = GetComponentInChildren<DialogueUI>(true);
    }
}
