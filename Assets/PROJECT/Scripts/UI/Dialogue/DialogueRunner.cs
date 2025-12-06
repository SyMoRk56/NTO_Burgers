using UnityEngine;

public class DialogueRunner : MonoBehaviour
{
    [Header("Настройки диалога")]
    public string ownerName;
    public DialogueScriptableObject[] defaultDialogues;
    public DialogueScriptableObject[] letterDialogues;
    public bool random;

    [Header("UI и эмоции")]
    public DialogueUI dialogueUI;
    public Face face;

    [Header("Аудио")]
    public AudioSource audioSource; // Источник для звуков фраз

    int currentDialogueIndex;
    int currentPhraseIndex;

    private bool isLetter;
    private bool isRunning;
    private bool isChoosing = false;

    public bool IsDialogueActive => isRunning;

    void Start()
    {
        dialogueUI = GetComponentInChildren<DialogueUI>(true);

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

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
            // Останавливаем NPC
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

            // Проигрываем звук фразы
            if (block.voiceOver != null && currentPhraseIndex < block.voiceOver.Length)
            {
                AudioClip clip = block.voiceOver[currentPhraseIndex];
                if (clip != null && audioSource != null)
                {
                    audioSource.PlayOneShot(clip);
                }
            }

            if (face != null)
            {
                if (block.emotions.Count != 0 && currentPhraseIndex < block.emotions.Count)
                {
                    face.SetFace(block.emotions[currentPhraseIndex]);
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

        var npc = GetComponent<NPCBehaviour>();
        if (npc != null)
        {
            npc.dialogueActive = false;
            npc.Resume();
        }
    }
}
