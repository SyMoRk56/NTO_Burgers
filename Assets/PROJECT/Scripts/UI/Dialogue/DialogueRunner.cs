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

        // Проверяем, показываются ли сейчас варианты выбора
        var block = isLetter ? letterDialogues[currentDialogueIndex] : defaultDialogues[currentDialogueIndex];
        bool isAtChoicePoint = currentPhraseIndex >= block.phrases.Length;

        // Если показывается текст - Space/E/клик для продолжения
        if (!isChoosing && !isAtChoicePoint && (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.E) ||
            Input.GetMouseButtonDown(0)))
        {
            NextPhrase();
        }
    }

    public void StartDialogue(bool letter)
    {
        // ВАЖНО: сначала закрываем предыдущий диалог, если он активен
        if (isRunning)
        {
            ForceCloseDialogue();
        }

        if (!isRunning)
        {
            isLetter = letter;
            currentDialogueIndex = 0;
            currentPhraseIndex = 0;
            isChoosing = false;

            // Проверяем, есть ли диалог для этого типа
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
        print(currentDialogueIndex);
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

    // ДОБАВЛЕНО: метод для принудительного закрытия диалога
    public void ForceCloseDialogue()
    {
        if (!isRunning) return;

        Debug.Log("Force closing dialogue");
        isRunning = false;
        isLetter = false;
        isChoosing = false;
        currentDialogueIndex = 0;
        currentPhraseIndex = 0;

        if (dialogueUI != null)
            dialogueUI.ForceHide();
    }

    // ДОБАВЛЕНО: метод для сброса состояния
    public void ResetDialogue()
    {
        isRunning = false;
        isLetter = false;
        isChoosing = false;
        currentDialogueIndex = 0;
        currentPhraseIndex = 0;

        if (dialogueUI != null)
            dialogueUI.ForceHide();
    }
    private void Start()
    {
        dialogueUI = GetComponentInChildren<DialogueUI>(true);
    }
}