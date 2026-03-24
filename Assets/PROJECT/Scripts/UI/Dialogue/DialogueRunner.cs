using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DialogueRunner : MonoBehaviour, IInteractObject
{
    [Header("Настройки диалога")]
    public string ownerName;
    public DialogueScriptableObject[] defaultDialogues;
    public DialogueScriptableObject[] letterDialogues;
    public bool random;

    public int CurrentDialogueIndex => currentDialogueIndex;
    public bool CurrentIsLetter => isLetter;

    [Header("UI и эмоции")]
    public DialogueUI dialogueUI;
    public Face face;

    [Header("Аудио")]
    public AudioSource audioSource;

    private int currentDialogueIndex;
    private int currentPhraseIndex;
    private bool isLetter;
    private bool isRunning;
    private bool isChoosing = false;
    public AudioClip clip;
    public bool IsDialogueActive => isRunning;

    private TreeCutscene treeScene;
    private Bench benchScene;
    private FlowerTriggerHandler flowerHandler;
    private bool wasApplePhraseSpoken = false;


    void Start()
    {
        benchScene = GetComponent<Bench>();
        if (benchScene == null)
            benchScene = GetComponentInChildren<Bench>();

        flowerHandler = GetComponent<FlowerTriggerHandler>();
        if (flowerHandler == null)
            flowerHandler = GetComponentInChildren<FlowerTriggerHandler>();

        // ── ИСПРАВЛЕНИЕ: не перезаписываем dialogueUI если уже назначен в инспекторе ──
        if (dialogueUI == null)
        {
            dialogueUI = GetComponentInChildren<DialogueUI>(true);
            if (dialogueUI == null)
                Debug.LogWarning($"[DialogueRunner] DialogueUI не найден на {gameObject.name}");
        }

        if (dialogueUI != null)
        {
            dialogueUI.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => CheckSkip());
        }

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        treeScene = GetComponent<TreeCutscene>();
        if (treeScene != null)
        {
            Debug.Log("Найден treecastscene");
        }
        else
        {
            treeScene = GetComponentInChildren<TreeCutscene>();
            if (treeScene != null)
                Debug.Log("Найден treecastscene в дочерних объектах");
            else
                Debug.LogWarning("treecastscene не найден");
        }
    }

    void Update()
    {
        if (!isRunning) return;

        // ── ИСПРАВЛЕНИЕ: обнуляем velocity только если диалог активен И игрок на земле ──
        // Не трогаем Rigidbody.linearVelocity — это ломает физику
        var pm = PlayerManager.instance;
        if (pm != null && pm.playerMovement != null)
        {
            pm.playerMovement.targetVelocity = Vector3.zero;
            pm.playerMovement.currentVelocity = Vector3.zero;
        }

        var block = isLetter ? letterDialogues[currentDialogueIndex] : defaultDialogues[currentDialogueIndex];
        bool isAtChoicePoint = currentPhraseIndex >= block.phrases.Length;

        if (!isChoosing &&
   (Input.GetKeyDown(KeyCode.Space) ||
    Input.GetKeyDown(KeyCode.E)))
        {
            CheckSkip();
        }

    }

    private void CheckSkip()
    {
        if (dialogueUI.IsTyping)
        {
            dialogueUI.CompleteTypingInstantly();
        }
        else
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
            StopAllAnimations(true);

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
            wasApplePhraseSpoken = false;

            var dialogues = letter ? letterDialogues : defaultDialogues;
            if (dialogues.Length == 0)
            {
                Debug.LogError($"No dialogues found for type: letter={letter}");
                return;
            }

            if (dialogueUI == null)
            {
                Debug.LogError($"[DialogueRunner] dialogueUI == null на {gameObject.name}! Назначь в инспекторе.");
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
                    PlayerManager.instance.playerMovement.moveInput = Vector2.zero;
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
            string phrase = block.phrases[currentPhraseIndex];
            dialogueUI.ShowPhrase(ownerName, phrase);

            if (clip != null && audioSource != null)
                audioSource.PlayOneShot(clip);

            if (face != null)
            {
                if (block.emotions.Count != 0 && currentPhraseIndex < block.emotions.Count)
                    face.SetFace(block.emotions[currentPhraseIndex]);
            }

            CheckForTreeSequence(phrase);
        }
        else
        {
            isChoosing = true;
            dialogueUI.ShowChoices(block.choices);
        }
    }

    void CheckForTreeSequence(string phrase)
    {
        phrase = LocalizationManager.Instance.Get(phrase);
        print("Check for phrase " + phrase);

        if (phrase.Contains("Яблоня") || phrase.Contains("яблоня") || phrase.Contains("Apple tree"))
        {
            Debug.Log($"DialogueRunner: Найдена фраза про яблоню: '{phrase}'");
            wasApplePhraseSpoken = true;
            if (treeScene != null)
                StartCoroutine(DelayedTreeSequence());
        }

        if (phrase.Contains("Скамейка") || phrase.Contains("скамейка") || phrase.Contains("bench"))
        {
            Debug.Log($"DialogueRunner: Найдена фраза про скамейку: '{phrase}'");
            if (benchScene != null)
                benchScene.TriggerCameraSequence();
            else
                Debug.LogWarning("DialogueRunner: benchScene не найден!");
        }
    }

    IEnumerator DelayedTreeSequence()
    {
        yield return new WaitForSeconds(2f);
        if (treeScene != null && wasApplePhraseSpoken)
        {
            Debug.Log("DialogueRunner: Запускаем кинопоследовательность с деревом...");
            treeScene.TriggerCameraSequence();
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
        wasApplePhraseSpoken = false;

        StopAllAnimations(false);

        var npc = GetComponent<NPCBehaviour>();
        if (npc != null)
        {
            npc.dialogueActive = false;
            npc.Resume();
        }

        var player = GameManager.Instance.GetPlayer();
        if (player != null)
        {
            var playerManager = player.GetComponent<PlayerManager>();
            if (playerManager != null)
            {
                playerManager.ShowCursor(false);
                playerManager.CanMove = true;
            }
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
        wasApplePhraseSpoken = false;

        StopAllAnimations(true);

        if (dialogueUI != null)
            dialogueUI.ForceHide();

        var npc = GetComponent<NPCBehaviour>();
        if (npc != null)
        {
            npc.dialogueActive = false;
            npc.Resume();
        }

        var player = GameManager.Instance.GetPlayer();
        if (player != null)
        {
            var playerManager = player.GetComponent<PlayerManager>();
            if (playerManager != null)
            {
                playerManager.ShowCursor(false);
                playerManager.CanMove = true;
            }
        }
    }

    private void StopAllAnimations(bool fullReset)
    {
        if (treeScene != null)
        {
            treeScene.StopAllCoroutines();
            if (fullReset) treeScene.ResetScene();
            else treeScene.StopScene();
        }

        if (benchScene != null)
        {
            benchScene.StopAllCoroutines();
            if (fullReset) benchScene.ResetScene();
            else benchScene.StopScene();
        }

        if (flowerHandler != null)
        {
            flowerHandler.StopAllCoroutines();
            if (fullReset)
            {
                var flowerField = flowerHandler.GetType().GetField("currentFlower",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (flowerField != null)
                {
                    GameObject currentFlower = (GameObject)flowerField.GetValue(flowerHandler);
                    if (currentFlower != null) Destroy(currentFlower);
                }

                var sneezeVFXField = flowerHandler.GetType().GetField("sneezeVFX",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (sneezeVFXField != null)
                {
                    ParticleSystem sneezeVFX = (ParticleSystem)sneezeVFXField.GetValue(flowerHandler);
                    if (sneezeVFX != null && sneezeVFX.isPlaying)
                        sneezeVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }
        }
    }

    public int InteractPriority() => -1;

    public bool CheckInteract()
    {
        if (TryGetComponent(out MailBox box))
            return box.CheckInteract();
        else
            return true;
    }

    public void Interact()
    {
        if (TryGetComponent(out MailBox box))
            box.Interact();
        else
            StartDialogue(false);
    }

    public void OnBeginInteract()
    {
        var npc = GetComponent<NPCBehaviour>();
        if (npc != null)
        {
            npc.dialogueActive = true;
            npc.Stop();
        }
    }

    public void OnEndInteract(bool succes)
    {
        if (succes) return;
        var npc = GetComponent<NPCBehaviour>();
        if (npc != null)
        {
            npc.dialogueActive = false;
            npc.Resume();
        }
    }

    public bool CheckDistance()
    {
        return GetComponentInChildren<InteractionUI>().CheckDistance();
    }
}