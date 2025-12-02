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

    // Флаг, указывающий, что сейчас показываются варианты выбора
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

        // Если показываются варианты - никакой автоматической обработки ввода
        // Кнопки сами обработают нажатия через onClick
    }

    public void StartDialogue(bool letter)
    {
        if (!isRunning)
        {
            isLetter = letter;
            currentDialogueIndex = 0;
            currentPhraseIndex = 0;
            isChoosing = false;
            dialogueUI.gameObject.SetActive(true);
            ShowCurrentPhrase();
            isRunning = true;
            GameManager.Instance.GetPlayer().GetComponent<PlayerManager>().ShowCursor(true);
            GameManager.Instance.GetPlayer().GetComponent<PlayerManager>().CanMove = false;
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
}